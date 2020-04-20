using System;
using xBRZNet.Blend;
using xBRZNet.Color;
using xBRZNet.Common;
using xBRZNet.Scalers;

namespace xBRZNet
{
    //http://intrepidis.blogspot.com/2014/02/xbrz-in-java.html
    /*
        -------------------------------------------------------------------------
        | xBRZ: "Scale by rules" - high quality image upscaling filter by Zenju |
        -------------------------------------------------------------------------
        using a modified approach of xBR:
        http://board.byuu.org/viewtopic.php?f=10&t=2248
        - new rule set preserving small image features
        - support multithreading
        - support 64 bit architectures
        - support processing image slices
    */

    /*
        -> map source (srcWidth * srcHeight) to target (scale * width x scale * height)
        image, optionally processing a half-open slice of rows [yFirst, yLast) only
        -> color format: ARGB (BGRA char order), alpha channel unused
        -> support for source/target pitch in chars!
        -> if your emulator changes only a few image slices during each cycle
        (e.g. Dosbox) then there's no need to run xBRZ on the complete image:
        Just make sure you enlarge the source image slice by 2 rows on top and
        2 on bottom (this is the additional range the xBRZ algorithm is using
        during analysis)
        Caveat: If there are multiple changed slices, make sure they do not overlap
        after adding these additional rows in order to avoid a memory race condition 
        if you are using multiple threads for processing each enlarged slice!

        THREAD-SAFETY: - parts of the same image may be scaled by multiple threads
        as long as the [yFirst, yLast) ranges do not overlap!
        - there is a minor inefficiency for the first row of a slice, so avoid
        processing single rows only
        */

    /*
        Converted to Java 7 by intrepidis. It would have been nice to use
        Java 8 lambdas, but Java 7 is more ubiquitous at the time of writing,
        so this code uses anonymous local classes instead.
        Regarding multithreading, each thread should have its own instance
        of the xBRZ class.
    */

    // ReSharper disable once InconsistentNaming
    public class xBRZScaler
    {
        public void ScaleImage(int scaleSize, int[] src, int[] trg, int srcWidth, int srcHeight, ScalerCfg cfg, int yFirst, int yLast)
        {
            _scaler = scaleSize.ToIScaler();
            _cfg = cfg;
            _colorDistance = new ColorDist(_cfg);
            _colorEqualizer = new ColorEq(_cfg);
            ScaleImageImpl(src, trg, srcWidth, srcHeight, yFirst, yLast);
        }

        private ScalerCfg _cfg;
        private IScaler _scaler;
        private OutputMatrix _outputMatrix;
        private readonly BlendResult _blendResult = new BlendResult();

        private ColorDist _colorDistance;
        private ColorEq _colorEqualizer;

        //fill block with the given color
        private static void FillBlock(int[] trg, int trgi, int pitch, int col, int blockSize)
        {
            for (var y = 0; y < blockSize; ++y, trgi += pitch)
            {
                for (var x = 0; x < blockSize; ++x)
                {
                    trg[trgi + x] = col;
                }
            }
        }

        //detect blend direction
        private void PreProcessCorners( Kernel4x4 ker)
        {
            _blendResult.Reset();

            if ((ker.F == ker.G && ker.J == ker.K) || (ker.F == ker.J && ker.G == ker.K)) return;

            var dist = _colorDistance;
            
            const int weight = 4;
            var jg = dist.DistYCbCr(ker.I, ker.F) + dist.DistYCbCr(ker.F, ker.C) + dist.DistYCbCr(ker.N, ker.K) + dist.DistYCbCr(ker.K, ker.H) + weight * dist.DistYCbCr(ker.J, ker.G);
            var fk = dist.DistYCbCr(ker.E, ker.J) + dist.DistYCbCr(ker.J, ker.O) + dist.DistYCbCr(ker.B, ker.G) + dist.DistYCbCr(ker.G, ker.L) + weight * dist.DistYCbCr(ker.F, ker.K);

            if (jg < fk)
            {
                var dominantGradient = _cfg.DominantDirectionThreshold * jg < fk;
                if (ker.F != ker.G && ker.F != ker.J)
                {
                    _blendResult.F = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
                if (ker.K != ker.J && ker.K != ker.G)
                {
                    _blendResult.K = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
            }
            else if (fk < jg)
            {
                var dominantGradient = _cfg.DominantDirectionThreshold * fk < jg;
                if (ker.J != ker.F && ker.J != ker.K)
                {
                    _blendResult.J = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
                if (ker.G != ker.F && ker.G != ker.K)
                {
                    _blendResult.G = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
            }
        }

        /*
            input kernel area naming convention:
            -------------
            | A | B | C |
            ----|---|---|
            | D | E | F | //input pixel is at position E
            ----|---|---|
            | G | H | I |
            -------------
            blendInfo: result of preprocessing all four corners of pixel "e"
        */
        private void ScalePixel(IScaler scaler, int rotDeg, Kernel3x3 ker, int trgi, char blendInfo)
        {
            // int a = ker._[Rot._[(0 << 2) + rotDeg]];
            var b = ker._[Rot._[(1 << 2) + rotDeg]];
            var c = ker._[Rot._[(2 << 2) + rotDeg]];
            var d = ker._[Rot._[(3 << 2) + rotDeg]];
            var e = ker._[Rot._[(4 << 2) + rotDeg]];
            var f = ker._[Rot._[(5 << 2) + rotDeg]];
            var g = ker._[Rot._[(6 << 2) + rotDeg]];
            var h = ker._[Rot._[(7 << 2) + rotDeg]];
            var i = ker._[Rot._[(8 << 2) + rotDeg]];

            var blend = blendInfo.Rotate((RotationDegree)rotDeg);

            if ((BlendType)blend.GetBottomR() == BlendType.None) return;

            var eq = _colorEqualizer;
            var dist = _colorDistance;

            bool doLineBlend;

            if (blend.GetBottomR() >= (char)BlendType.Dominant)
            {
                doLineBlend = true;
            }
            //make sure there is no second blending in an adjacent
            //rotation for this pixel: handles insular pixels, mario eyes
            //but support double-blending for 90� corners
            else if (blend.GetTopR() != (char)BlendType.None && !eq.IsColorEqual(e, g))
            {
                doLineBlend = false;
            }
            else if (blend.GetBottomL() != (char)BlendType.None && !eq.IsColorEqual(e, c))
            {
                doLineBlend = false;
            }
            //no full blending for L-shapes; blend corner only (handles "mario mushroom eyes")
            else if (eq.IsColorEqual(g, h) && eq.IsColorEqual(h, i) && eq.IsColorEqual(i, f) && eq.IsColorEqual(f, c) && !eq.IsColorEqual(e, i))
            {
                doLineBlend = false;
            }
            else
            {
                doLineBlend = true;
            }

            //choose most similar color
            var px = dist.DistYCbCr(e, f) <= dist.DistYCbCr(e, h) ? f : h;

            var out_ = _outputMatrix;
            out_.Move(rotDeg, trgi);

            if (!doLineBlend)
            {
                scaler.BlendCorner(px, out_);
                return;
            }

            //test sample: 70% of values max(fg, hc) / min(fg, hc)
            //are between 1.1 and 3.7 with median being 1.9
             var fg = dist.DistYCbCr(f, g);
             var hc = dist.DistYCbCr(h, c);

             var haveShallowLine = _cfg.SteepDirectionThreshold * fg <= hc && e != g && d != g;
             var haveSteepLine = _cfg.SteepDirectionThreshold * hc <= fg && e != c && b != c;

            if (haveShallowLine)
            {
                if (haveSteepLine)
                {
                    scaler.BlendLineSteepAndShallow(px, out_);
                }
                else
                {
                    scaler.BlendLineShallow(px, out_);
                } 
            }
            else
            {
                if (haveSteepLine)
                {
                    scaler.BlendLineSteep(px, out_);
                }
                else
                {
                    scaler.BlendLineDiagonal(px, out_);
                }
            }
        }

        //scaler policy: see "Scaler2x" reference implementation
        private void ScaleImageImpl(int[] src, int[] trg, int srcWidth, int srcHeight, int yFirst, int yLast)
        {
            yFirst = Math.Max(yFirst, 0);
            yLast = Math.Min(yLast, srcHeight);

            if (yFirst >= yLast || srcWidth <= 0) return;

            var trgWidth = srcWidth * _scaler.Scale;

            //temporary buffer for "on the fly preprocessing"
            var preProcBuffer = new char[srcWidth];

            var ker4 = new Kernel4x4();

            //initialize preprocessing buffer for first row:
            //detect upper left and right corner blending
            //this cannot be optimized for adjacent processing
            //stripes; we must not allow for a memory race condition!
            if (yFirst > 0)
            {
                var y = yFirst - 1;

                var sM1 = srcWidth * Math.Max(y - 1, 0);
                var s0 = srcWidth * y; //center line
                var sP1 = srcWidth * Math.Min(y + 1, srcHeight - 1);
                var sP2 = srcWidth * Math.Min(y + 2, srcHeight - 1);

                for (var x = 0; x<srcWidth; ++x)
                {
                    var xM1 = Math.Max(x - 1, 0);
                    var xP1 = Math.Min(x + 1, srcWidth - 1);
                    var xP2 = Math.Min(x + 2, srcWidth - 1);

                    //read sequentially from memory as far as possible
                    ker4.A = src[sM1 + xM1];
                    ker4.B = src[sM1 + x];
                    ker4.C = src[sM1 + xP1];
                    ker4.D = src[sM1 + xP2];

                    ker4.E = src[s0 + xM1];
                    ker4.F = src[s0 + x];
                    ker4.G = src[s0 + xP1];
                    ker4.H = src[s0 + xP2];

                    ker4.I = src[sP1 + xM1];
                    ker4.J = src[sP1 + x];
                    ker4.K = src[sP1 + xP1];
                    ker4.L = src[sP1 + xP2];

                    ker4.M = src[sP2 + xM1];
                    ker4.N = src[sP2 + x];
                    ker4.O = src[sP2 + xP1];
                    ker4.P = src[sP2 + xP2];

                    PreProcessCorners(ker4); // writes to blendResult
                    /*
                    preprocessing blend result:
                    ---------
                    | F | G | //evalute corner between F, G, J, K
                    ----|---| //input pixel is at position F
                    | J | K |
                    ---------
                    */
                    preProcBuffer[x] = preProcBuffer[x].SetTopR(_blendResult.J);

                    if (x + 1 < srcWidth)
                    {
                        preProcBuffer[x + 1] = preProcBuffer[x + 1].SetTopL(_blendResult.K);
                    }
                }
            }

            _outputMatrix = new OutputMatrix(_scaler.Scale, trg, trgWidth);

            var ker3 = new Kernel3x3();

            for (var y = yFirst; y<yLast; ++y)
            {
                //consider MT "striped" access
                var trgi = _scaler.Scale * y * trgWidth;

                var sM1 = srcWidth * Math.Max(y - 1, 0);
                var s0 = srcWidth * y; //center line
                var sP1 = srcWidth * Math.Min(y + 1, srcHeight - 1);
                var sP2 = srcWidth * Math.Min(y + 2, srcHeight - 1);

                var blendXy1 = (char)0;

                for (var x = 0; x < srcWidth; ++x, trgi += _scaler.Scale)
                {
                    var xM1 = Math.Max(x - 1, 0);
                    var xP1 = Math.Min(x + 1, srcWidth - 1);
                    var xP2 = Math.Min(x + 2, srcWidth - 1);

                    //evaluate the four corners on bottom-right of current pixel
                    //blend_xy for current (x, y) position
                    
                    //read sequentially from memory as far as possible
                    ker4.A = src[sM1 + xM1];
                    ker4.B = src[sM1 + x];
                    ker4.C = src[sM1 + xP1];
                    ker4.D = src[sM1 + xP2];

                    ker4.E = src[s0 + xM1];
                    ker4.F = src[s0 + x];
                    ker4.G = src[s0 + xP1];
                    ker4.H = src[s0 + xP2];

                    ker4.I = src[sP1 + xM1];
                    ker4.J = src[sP1 + x];
                    ker4.K = src[sP1 + xP1];
                    ker4.L = src[sP1 + xP2];

                    ker4.M = src[sP2 + xM1];
                    ker4.N = src[sP2 + x];
                    ker4.O = src[sP2 + xP1];
                    ker4.P = src[sP2 + xP2];

                    PreProcessCorners(ker4); // writes to blendResult

                    /*
                        preprocessing blend result:
                        ---------
                        | F | G | //evaluate corner between F, G, J, K
                        ----|---| //current input pixel is at position F
                        | J | K |
                        ---------
                    */

                    //all four corners of (x, y) have been determined at
                    //this point due to processing sequence!
                    var blendXy = preProcBuffer[x].SetBottomR(_blendResult.F);

                    //set 2nd known corner for (x, y + 1)
                    blendXy1 = blendXy1.SetTopR(_blendResult.J);
                    //store on current buffer position for use on next row
                    preProcBuffer[x] = blendXy1;

                    //set 1st known corner for (x + 1, y + 1) and
                    //buffer for use on next column
                    blendXy1 = ((char)0).SetTopL(_blendResult.K);

                    if (x + 1 < srcWidth)
                    {
                        //set 3rd known corner for (x + 1, y)
                        preProcBuffer[x + 1] = preProcBuffer[x + 1].SetBottomL(_blendResult.G);
                    }

                    //fill block of size scale * scale with the given color
                    //  //place *after* preprocessing step, to not overwrite the
                    //  //results while processing the the last pixel!
                    FillBlock(trg, trgi, trgWidth, src[s0 + x], _scaler.Scale);

                    //blend four corners of current pixel
                    if (blendXy == 0) continue;

                    const int a = 0, b = 1, c = 2, d = 3, e = 4, f = 5, g = 6, h = 7, i = 8;

                    //read sequentially from memory as far as possible
                    ker3._[a] = src[sM1 + xM1];
                    ker3._[b] = src[sM1 + x];
                    ker3._[c] = src[sM1 + xP1];

                    ker3._[d] = src[s0 + xM1];
                    ker3._[e] = src[s0 + x];
                    ker3._[f] = src[s0 + xP1];

                    ker3._[g] = src[sP1 + xM1];
                    ker3._[h] = src[sP1 + x];
                    ker3._[i] = src[sP1 + xP1];

                    ScalePixel(_scaler, (int)RotationDegree.R0, ker3, trgi, blendXy);
                    ScalePixel(_scaler, (int)RotationDegree.R90, ker3, trgi, blendXy);
                    ScalePixel(_scaler, (int)RotationDegree.R180, ker3, trgi, blendXy);
                    ScalePixel(_scaler, (int)RotationDegree.R270, ker3, trgi, blendXy);
               }
            }
        }
    }
}
