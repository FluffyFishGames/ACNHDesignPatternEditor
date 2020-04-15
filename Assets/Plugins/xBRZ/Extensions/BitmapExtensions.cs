using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace xBRZNet.Extensions
{
    internal static class BitmapExtensions
    {
        //http://stackoverflow.com/a/2016509/294804
        public static Bitmap ChangeFormat(this Bitmap image, PixelFormat format)
        {
            var newFormatImage = new Bitmap(image.Width, image.Height, format);
            using (var gr = Graphics.FromImage(newFormatImage))
            {
                gr.DrawImage(image, new Rectangle(0, 0, newFormatImage.Width, newFormatImage.Height));
            }
            return newFormatImage;
        }

        //https://msdn.microsoft.com/en-us/library/ms229672(v=vs.90).aspx
        public static int[] ToIntArray(this Bitmap image)
        {
            // Lock the bitmap's bits.
            var rectangle = new Rectangle(0, 0, image.Width, image.Height);
            var bitmapData = image.LockBits(rectangle, ImageLockMode.ReadWrite, image.PixelFormat);
            // Get the address of the first line.
            var bitmapPointer = bitmapData.Scan0;
            //http://stackoverflow.com/a/13273799/294804
            if (bitmapData.Stride < 0)
            {
                bitmapPointer += bitmapData.Stride * (image.Height - 1);
            }
            //http://stackoverflow.com/a/1917036/294804
            // Declare an array to hold the bytes of the bitmap. 
            var intCount = bitmapData.Stride * image.Height / 4;
            var values = new int[intCount];
            // Copy the RGB values into the array.
            Marshal.Copy(bitmapPointer, values, 0, intCount);
            // Unlock the bits.
            image.UnlockBits(bitmapData);

            return values;
        }

        public static Bitmap ToBitmap(this int[] bitmapData, int width, int height, PixelFormat format)
        {
            var newImage = new Bitmap(width, height, format);
            var rectangle = new Rectangle(0, 0, newImage.Width, newImage.Height);
            var newBitmapData = newImage.LockBits(rectangle, ImageLockMode.ReadWrite, format);
            // Get the address of the first line.
            var newBitmapPointer = newBitmapData.Scan0;
            //http://stackoverflow.com/a/1917036/294804
            var intCount = newBitmapData.Stride * newImage.Height / 4;
            // Copy the RGB values back to the bitmap
            Marshal.Copy(bitmapData, 0, newBitmapPointer, intCount);
            // Unlock the bits.
            newImage.UnlockBits(newBitmapData);

            return newImage;
        }
    }
}
