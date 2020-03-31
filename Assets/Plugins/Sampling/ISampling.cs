using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISampling
{
	System.Drawing.Bitmap Resample(System.Drawing.Bitmap image, int width, int height);
}
