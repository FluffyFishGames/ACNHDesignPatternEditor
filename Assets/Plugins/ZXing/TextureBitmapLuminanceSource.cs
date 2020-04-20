/*
* Copyright 2012 ZXing.Net authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Runtime.InteropServices;

namespace ZXing
{
    /// <summary>
    /// class which represents the luminance values for a bitmap object
    /// </summary>
    public partial class TextureBitmapLuminanceSource : BaseLuminanceSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapLuminanceSource"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected TextureBitmapLuminanceSource(int width, int height)
           : base(width, height)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapLuminanceSource"/> class
        /// with the image of a Bitmap instance
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        public TextureBitmapLuminanceSource(TextureBitmap bitmap)
           : base(bitmap.Width, bitmap.Height)
        {
            CalculateLuminanceValues(bitmap, luminances);
        }

        protected static void CalculateLuminanceValues(TextureBitmap bitmap, byte[] luminances)
        {
            var height = bitmap.Height;
            var width = bitmap.Width;

            CalculateLuminanceValues32BitWithAlpha(bitmap, luminances);
        }

        private static void CalculateLuminanceValues32BitWithAlpha(TextureBitmap bitmap, byte[] luminances)
        {
            var height = bitmap.Height;
            var width = bitmap.Width;
            var pixelWidth = bitmap.PixelSize;
            var maxIndex = 4 * width;
            unsafe
            {
                var ptr = bitmap.GetColors();

                for (int y = 0; y < height; y++)
                {
                    var offset = y * width;
                    for (int x = 0; x < width; x ++)
                    {
                        var col = *(ptr + x + (height - 1 - y) * width);
                        var luminance = (byte) ((BChannelWeight * (col).B +
                                                GChannelWeight * (col).G +
                                                RChannelWeight * (col).R) >> ChannelWeight);

                        var alpha = col.A;
                        luminance = (byte) (((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8) + 1);
                        luminances[x + y * width] = luminance;
                    }
                }
            }
        }

        /// <summary>
        /// Should create a new luminance source with the right class type.
        /// The method is used in methods crop and rotate.
        /// </summary>
        /// <param name="newLuminances">The new luminances.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new TextureBitmapLuminanceSource(width, height) { luminances = newLuminances };
        }
    }
}