using System;

namespace SimplePaletteQuantizer.Quantizers.DistinctSelection
{
    /// <summary>
    /// Stores all the informations about single color only once, to be used later.
    /// </summary>
    public class DistinctColorInfo
    {
        private const Int32 Factor = 5000000;

        /// <summary>
        /// The original color.
        /// </summary>
        public Int32 Color { get; private set; }

        /// <summary>
        /// The pixel presence count in the image.
        /// </summary>
        public Int32 Count { get; private set; }

        /// <summary>
        /// A hue component of the color.
        /// </summary>
        public Int32 Hue { get; private set; }

        /// <summary>
        /// A saturation component of the color.
        /// </summary>
        public Int32 Saturation { get; private set; }

        /// <summary>
        /// A brightness component of the color.
        /// </summary>
        public Int32 Brightness { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctColorInfo"/> struct.
        /// </summary>
        public DistinctColorInfo(TextureBitmap.Color color)
        {
            Color = color.ToARGB();
            Count = 1;

            Hue = Convert.ToInt32(color.GetHue()*Factor);
            Saturation = Convert.ToInt32(color.GetSaturation()*Factor);
            Brightness = Convert.ToInt32(color.GetBrightness()*Factor);
        }

        /// <summary>
        /// Increases the count of pixels of this color.
        /// </summary>
        public DistinctColorInfo IncreaseCount()
        {
            Count++;
            return this;
        }
    }
}
