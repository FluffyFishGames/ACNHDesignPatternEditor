namespace xBRZNet.Blend
{
    internal enum BlendType
    {
        // These blend types must fit into 2 bits.
        None = 0, //do not blend
        Normal = 1,//a normal indication to blend
        Dominant = 2 //a strong indication to blend
    }
}
