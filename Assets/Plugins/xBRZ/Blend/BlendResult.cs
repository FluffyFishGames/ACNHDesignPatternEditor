namespace xBRZNet.Blend
{
    internal class BlendResult
    {
        public char F { get; set; }
        public char G { get; set; }
        public char J { get; set; }
        public char K { get; set; }

        public void Reset()
        {
            F = G = J = K = (char)0;
        }
    }
}
