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

using ZXing.Common;
using ZXing.OneD;

namespace ZXing.Rendering
{
    /// <summary>
    /// Renders a <see cref="BitMatrix" /> to a <see cref="Bitmap" /> image
    /// </summary>
    public class TextureBitmapRenderer : IBarcodeRenderer<TextureBitmap>
    {
        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        /// <value>The foreground color.</value>
        public TextureBitmap.Color Foreground { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <value>The background color.</value>
        public TextureBitmap.Color Background { get; set; }

#if !WindowsCE
        /// <summary>
        /// Gets or sets the resolution which should be used to create the bitmap
        /// If nothing is set the current system settings are used
        /// </summary>
        public float? DpiX { get; set; }

        /// <summary>
        /// Gets or sets the resolution which should be used to create the bitmap
        /// If nothing is set the current system settings are used
        /// </summary>
        public float? DpiY { get; set; }
#endif

        static TextureBitmapRenderer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapRenderer"/> class.
        /// </summary>
        public TextureBitmapRenderer()
        {
            Foreground = new TextureBitmap.Color(0, 0, 0, 255);
            Background = new TextureBitmap.Color(255, 255, 255, 255);
        }

        /// <summary>
        /// Renders the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="format">The format.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public TextureBitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
        {
            return Render(matrix, format, content, null);
        }

        /// <summary>
        /// Renders the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="format">The format.</param>
        /// <param name="content">The content.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        virtual public TextureBitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
        {
            var width = matrix.Width;
            var height = matrix.Height;
            var outputContent = (options == null || !options.PureBarcode) &&
                                !String.IsNullOrEmpty(content) &&
                                (format == BarcodeFormat.CODE_39 ||
                                 format == BarcodeFormat.CODE_93 ||
                                 format == BarcodeFormat.CODE_128 ||
                                 format == BarcodeFormat.EAN_13 ||
                                 format == BarcodeFormat.EAN_8 ||
                                 format == BarcodeFormat.CODABAR ||
                                 format == BarcodeFormat.ITF ||
                                 format == BarcodeFormat.UPC_A ||
                                 format == BarcodeFormat.UPC_E ||
                                 format == BarcodeFormat.MSI ||
                                 format == BarcodeFormat.PLESSEY);

            if (options != null)
            {
                if (options.Width > width)
                {
                    width = options.Width;
                }
                if (options.Height > height)
                {
                    height = options.Height;
                }
            }

            // calculating the scaling factor
            var pixelsizeWidth = width / matrix.Width;
            var pixelsizeHeight = height / matrix.Height;

            if (pixelsizeWidth != pixelsizeHeight)
            {
                if (format == BarcodeFormat.QR_CODE ||
                    format == BarcodeFormat.AZTEC ||
                    format == BarcodeFormat.DATA_MATRIX ||
                    format == BarcodeFormat.MAXICODE ||
                    format == BarcodeFormat.PDF_417)
                {
                    // symetric scaling
                    pixelsizeHeight = pixelsizeWidth = pixelsizeHeight < pixelsizeWidth ? pixelsizeHeight : pixelsizeWidth;
                }
            }

            // create the bitmap and lock the bits because we need the stride
            // which is the width of the image and possible padding bytes
            var bmp = new TextureBitmap(width, height, false);

            unsafe
            {
                var colors = bmp.GetColors();
                var index = 0;
                var color = Background;

                // going through the lines of the matrix
                for (int y = 0; y < matrix.Height; y++)
                {
                    // stretching the line by the scaling factor
                    for (var pixelsizeHeightProcessed = 0; pixelsizeHeightProcessed < pixelsizeHeight; pixelsizeHeightProcessed++)
                    {
                        // going through the columns of the current line
                        for (var x = 0; x < matrix.Width; x++)
                        {
                            color = matrix[x, y] ? Foreground : Background;
                            // stretching the columns by the scaling factor
                            for (var pixelsizeWidthProcessed = 0; pixelsizeWidthProcessed < pixelsizeWidth; pixelsizeWidthProcessed++)
                            {
                                colors[index++] = color;
                            }
                        }
                        // fill up to the right if the barcode doesn't fully fit in 
                        for (var x = pixelsizeWidth * matrix.Width; x < width; x++)
                        {
                            colors[index++] = Background;
                        }
                    }
                }

                // fill up to the bottom if the barcode doesn't fully fit in 
                for (var y = pixelsizeHeight * matrix.Height; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        colors[index++] = Background;
                    }
                }
            }

            return bmp;
        }
    }
}
