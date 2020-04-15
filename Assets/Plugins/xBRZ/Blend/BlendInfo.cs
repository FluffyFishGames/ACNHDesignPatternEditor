using xBRZNet.Common;

namespace xBRZNet.Blend
{
    internal static class BlendInfo
    {
        public static char GetTopL(this char b) { return (char)(b & 0x3); }
        public static char GetTopR(this char b) { return (char)((b >> 2) & 0x3); }
        public static char GetBottomR(this char b) { return (char)((b >> 4) & 0x3); }
        public static char GetBottomL(this char b) { return (char)((b >> 6) & 0x3); }

        public static char SetTopL(this char b, char bt) { return (char)(b | bt); }
        public static char SetTopR(this char b, char bt) { return (char)(b | (bt << 2)); }
        public static char SetBottomR(this char b, char bt) { return (char)(b | (bt << 4)); }
        public static char SetBottomL(this char b, char bt) { return (char)(b | (bt << 6)); }

        public static char Rotate(this char b, RotationDegree rotDeg)
        {
            var l = (int)rotDeg << 1;
            var r = 8 - l;

            return (char)(b << l | b >> r);
        }
    }
}
