namespace xBRZNet
{
    public class ScalerCfg
    {
        // These are the default values:
        public double LuminanceWeight { get; set; } = 1;
        public double EqualColorTolerance { get; set; } = 30;
        public double DominantDirectionThreshold { get; set; } = 3.6;
        public double SteepDirectionThreshold { get; set; } = 2.2;
    }
}
