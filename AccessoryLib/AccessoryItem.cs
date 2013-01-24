using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AccessoryLib
{
    public enum AccessoryPositon
    {
        Hat,
        Glasses,
        Beard
    }

    public class AccessoryItem
    {
        /// <param name="width">Breite in m.</param>
        public AccessoryItem(AccessoryPositon position, string imagePath, double width)
        {
            Position = position;
            Image = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            Width = width;
        }

        public AccessoryPositon Position { get; private set; }
        public ImageSource Image { get; private set; }
        public double Width { get; private set; }
    }
}
