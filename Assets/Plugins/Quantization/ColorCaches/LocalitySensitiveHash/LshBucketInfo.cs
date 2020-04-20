using System;
using System.Collections.Generic;

namespace SimplePaletteQuantizer.ColorCaches.LocalitySensitiveHash
{
    public class BucketInfo
    {
        private readonly SortedDictionary<Int32, TextureBitmap.Color> colors;

        /// <summary>
        /// Gets the colors.
        /// </summary>
        /// <value>The colors.</value>
        public IDictionary<Int32, TextureBitmap.Color> Colors
        {
            get { return colors; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketInfo"/> class.
        /// </summary>
        public BucketInfo()
        {
            colors = new SortedDictionary<Int32, TextureBitmap.Color>();
        }

        /// <summary>
        /// Adds the color to the bucket informations.
        /// </summary>
        /// <param name="paletteIndex">Index of the palette.</param>
        /// <param name="color">The color.</param>
        public void AddColor(Int32 paletteIndex, TextureBitmap.Color color)
        {
            colors.Add(paletteIndex, color);
        }
    }
}
