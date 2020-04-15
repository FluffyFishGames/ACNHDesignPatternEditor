using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MitchellSampling : ISampling
{
	public Bitmap Resample(Bitmap image, int width, int height)
	{
		ResamplingService resamplingService = new ResamplingService();
		resamplingService.Filter = ResamplingFilters.Mitchell;
		ushort[][,] input = ResamplingFilter.ConvertBitmapToArray((Bitmap) image);
		ushort[][,] output = resamplingService.Resample(input, width, height);
		Bitmap result = (Bitmap) ResamplingFilter.ConvertArrayToBitmap(output);
		return result;
	}
}
