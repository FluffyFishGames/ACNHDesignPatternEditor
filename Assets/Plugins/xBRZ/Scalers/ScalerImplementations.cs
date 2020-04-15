using xBRZNet.Common;

namespace xBRZNet.Scalers
{
    internal interface IScaler
    {
        int Scale { get; }
        void BlendLineSteep(int col, OutputMatrix out_);
        void BlendLineSteepAndShallow(int col, OutputMatrix out_);
        void BlendLineShallow(int col, OutputMatrix out_);
        void BlendLineDiagonal(int col, OutputMatrix out_);
        void BlendCorner(int col, OutputMatrix out_);
    }

    internal abstract class ScalerBase
    {
        protected static void AlphaBlend(int n, int m, IntPtr dstPtr, int col)
        {
            //assert n < 256 : "possible overflow of (col & redMask) * N";
            //assert m < 256 : "possible overflow of (col & redMask) * N + (dst & redMask) * (M - N)";
            //assert 0 < n && n < m : "0 < N && N < M";

            //this works because 8 upper bits are free
            var dst = dstPtr.Get();
            var redComponent = BlendComponent(Mask.Red, n, m, dst, col);
            var greenComponent = BlendComponent(Mask.Green, n, m, dst, col);
            var blueComponent = BlendComponent(Mask.Blue, n, m, dst, col);
            var blend = (redComponent | greenComponent | blueComponent);
            dstPtr.Set(blend | unchecked((int)0xff000000)); // MJY: Added required cast but will throw an exception if the asserts at the top are not checked.
        }

        private static int BlendComponent(int mask, int n, int m, int inPixel, int setPixel)
        {
            var inChan = inPixel & mask;
            var setChan = setPixel & mask;
            var blend = setChan * n + inChan * (m - n);
            var component = mask & (blend / m);
            return component;
        }
    }

    internal class Scaler2X : ScalerBase, IScaler
    {
        public int Scale { get; } = 2;

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(Scale - 1, 0), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 1, 1), col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, Scale - 1), col);
            AlphaBlend(3, 4, out_.Ref(1, Scale - 1), col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(1, 0), col);
            AlphaBlend(1, 4, out_.Ref(0, 1), col);
            AlphaBlend(5, 6, out_.Ref(1, 1), col); //[!] fixes 7/8 used in xBR
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 2, out_.Ref(1, 1), col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(21, 100, out_.Ref(1, 1), col); //exact: 1 - pi/4 = 0.2146018366
        }
    }

    internal class Scaler3X : ScalerBase, IScaler
    {
        public int Scale { get; } = 3;

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(Scale - 2, 2), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 1, 1), col);
            out_.Ref(Scale - 1, 2).Set(col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, Scale - 2), col);
            AlphaBlend(3, 4, out_.Ref(1, Scale - 1), col);
            out_.Ref(2, Scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(2, 0), col);
            AlphaBlend(1, 4, out_.Ref(0, 2), col);
            AlphaBlend(3, 4, out_.Ref(2, 1), col);
            AlphaBlend(3, 4, out_.Ref(1, 2), col);
            out_.Ref(2, 2).Set(col);
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 8, out_.Ref(1, 2), col);
            AlphaBlend(1, 8, out_.Ref(2, 1), col);
            AlphaBlend(7, 8, out_.Ref(2, 2), col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(45, 100, out_.Ref(2, 2), col); //exact: 0.4545939598
                                                      //alphaBlend(14, 1000, out.ref(2, 1), col); //0.01413008627 -> negligable
                                                      //alphaBlend(14, 1000, out.ref(1, 2), col); //0.01413008627
        }
    }

    internal class Scaler4X : ScalerBase, IScaler
    {
        public int Scale { get; } = 4;

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(Scale - 2, 2), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 1, 1), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 2, 3), col);
            out_.Ref(Scale - 1, 2).Set(col);
            out_.Ref(Scale - 1, 3).Set(col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, Scale - 2), col);
            AlphaBlend(3, 4, out_.Ref(1, Scale - 1), col);
            AlphaBlend(3, 4, out_.Ref(3, Scale - 2), col);
            out_.Ref(2, Scale - 1).Set(col);
            out_.Ref(3, Scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(3, 4, out_.Ref(3, 1), col);
            AlphaBlend(3, 4, out_.Ref(1, 3), col);
            AlphaBlend(1, 4, out_.Ref(3, 0), col);
            AlphaBlend(1, 4, out_.Ref(0, 3), col);
            AlphaBlend(1, 3, out_.Ref(2, 2), col); //[!] fixes 1/4 used in xBR
            out_.Ref(3, 3).Set(col);
            out_.Ref(3, 2).Set(col);
            out_.Ref(2, 3).Set(col);
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 2, out_.Ref(Scale - 1, Scale / 2), col);
            AlphaBlend(1, 2, out_.Ref(Scale - 2, Scale / 2 + 1), col);
            out_.Ref(Scale - 1, Scale - 1).Set(col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(68, 100, out_.Ref(3, 3), col); //exact: 0.6848532563
            AlphaBlend(9, 100, out_.Ref(3, 2), col); //0.08677704501
            AlphaBlend(9, 100, out_.Ref(2, 3), col); //0.08677704501
        }
    }

    internal class Scaler5X : ScalerBase, IScaler
    {
        public int Scale { get; } = 5;

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(Scale - 2, 2), col);
            AlphaBlend(1, 4, out_.Ref(Scale - 3, 4), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 1, 1), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 2, 3), col);
            out_.Ref(Scale - 1, 2).Set(col);
            out_.Ref(Scale - 1, 3).Set(col);
            out_.Ref(Scale - 1, 4).Set(col);
            out_.Ref(Scale - 2, 4).Set(col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, Scale - 2), col);
            AlphaBlend(1, 4, out_.Ref(4, Scale - 3), col);
            AlphaBlend(3, 4, out_.Ref(1, Scale - 1), col);
            AlphaBlend(3, 4, out_.Ref(3, Scale - 2), col);
            out_.Ref(2, Scale - 1).Set(col);
            out_.Ref(3, Scale - 1).Set(col);
            out_.Ref(4, Scale - 1).Set(col);
            out_.Ref(4, Scale - 2).Set(col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, Scale - 2), col);
            AlphaBlend(3, 4, out_.Ref(1, Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(Scale - 2, 2), col);
            AlphaBlend(3, 4, out_.Ref(Scale - 1, 1), col);
            out_.Ref(2, Scale - 1).Set(col);
            out_.Ref(3, Scale - 1).Set(col);
            out_.Ref(Scale - 1, 2).Set(col);
            out_.Ref(Scale - 1, 3).Set(col);
            out_.Ref(4, Scale - 1).Set(col);
            AlphaBlend(2, 3, out_.Ref(3, 3), col);
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
            AlphaBlend(1, 8, out_.Ref(Scale - 1, Scale / 2), col);
            AlphaBlend(1, 8, out_.Ref(Scale - 2, Scale / 2 + 1), col);
            AlphaBlend(1, 8, out_.Ref(Scale - 3, Scale / 2 + 2), col);
            AlphaBlend(7, 8, out_.Ref(4, 3), col);
            AlphaBlend(7, 8, out_.Ref(3, 4), col);
            out_.Ref(4, 4).Set(col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(86, 100, out_.Ref(4, 4), col); //exact: 0.8631434088
            AlphaBlend(23, 100, out_.Ref(4, 3), col); //0.2306749731
            AlphaBlend(23, 100, out_.Ref(3, 4), col); //0.2306749731
                                                        //alphaBlend(8, 1000, out.ref(4, 2), col); //0.008384061834 -> negligable
                                                        //alphaBlend(8, 1000, out.ref(2, 4), col); //0.008384061834
        }
    }
}
