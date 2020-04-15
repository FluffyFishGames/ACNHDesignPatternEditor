using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ICrop
{
	void SetImage(System.Drawing.Bitmap bitmap, int desiredWidth, int desiredHeight);
	int GetHeight();
	int GetWidth();
}
