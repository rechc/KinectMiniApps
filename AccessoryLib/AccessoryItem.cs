using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AccessoryLib
{
    public class AccessoryItem
    {
        public AccessoryItem(AccessoryPositon position, string imagePath)
        {
            Position = position;
            Image = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
        }

        public AccessoryPositon Position { get; private set; }
        public ImageSource Image { get; private set; }
    }
}
