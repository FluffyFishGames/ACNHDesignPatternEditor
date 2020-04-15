using xBRZNet.Common;

namespace xBRZNet.Scalers
{
    //access matrix area, top-left at position "out" for image with given width
    internal class OutputMatrix
    {
        private readonly IntPtr _out;
        private readonly int _outWidth;
        private readonly int _n;
        private int _outi;
        private int _nr;

        private const int MaxScale = 5; // Highest possible scale
        private const int MaxScaleSquared = MaxScale * MaxScale;

        public OutputMatrix(int scale, int[] outPtr, int outWidth)
        {
            _n = (scale - 2) * (Rot.MaxRotations * MaxScaleSquared);
            _out = new IntPtr(outPtr);
            _outWidth = outWidth;
        }

        public void Move(int rotDeg, int outi)
        {
            _nr = _n + rotDeg * MaxScaleSquared;
            _outi = outi;
        }

        public IntPtr Ref(int i, int j)
        {
            var rot = MatrixRotation[_nr + i * MaxScale + j];
            _out.Position(_outi + rot.J + rot.I * _outWidth);
            return _out;
        }

        //calculate input matrix coordinates after rotation at program startup
        private static readonly IntPair[] MatrixRotation = new IntPair[(MaxScale - 1) * MaxScaleSquared * Rot.MaxRotations];

        static OutputMatrix()
        {
            for (var n = 2; n < MaxScale + 1; n++)
            {
                for (var r = 0; r < Rot.MaxRotations; r++)
                {
                    var nr = (n - 2) * (Rot.MaxRotations * MaxScaleSquared) + r * MaxScaleSquared;
                    for (var i = 0; i < MaxScale; i++)
                    {
                        for (var j = 0; j < MaxScale; j++)
                        {
                            MatrixRotation[nr + i * MaxScale + j] = BuildMatrixRotation(r, i, j, n);
                        }
                    }
                }
            }
        }

        private static IntPair BuildMatrixRotation(int rotDeg, int i, int j, int n)
        {
            int iOld, jOld;

            if (rotDeg == 0)
            {
                iOld = i;
                jOld = j;
            }
            else
            {
                //old coordinates before rotation!
                var old = BuildMatrixRotation(rotDeg - 1, i, j, n);
                iOld = n - 1 - old.J;
                jOld = old.I;
            }

            return new IntPair(iOld, jOld);
        }
    }
}
