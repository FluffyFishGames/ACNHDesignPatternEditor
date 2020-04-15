using System;
using xBRZNet.Scalers;

namespace xBRZNet.Color
{
    internal class ColorEq : ColorDist
    {
        public ColorEq(ScalerCfg cfg) : base(cfg) { }

        public bool IsColorEqual(int color1, int color2)
        {
            var eqColorThres = Math.Pow(Cfg.EqualColorTolerance, 2);
            return DistYCbCr(color1, color2) < eqColorThres;
        }
    }
}
