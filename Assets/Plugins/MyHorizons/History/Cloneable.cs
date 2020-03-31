using System;

namespace MyHorizons.History
{
    public abstract class Cloneable : ICloneable
    {
        public virtual object Clone() => MemberwiseClone();
    }
}
