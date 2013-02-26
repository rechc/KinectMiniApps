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
        private const String PATH = "../../../HtwKinect/Images/Accessories/";

        /// <param name="width">Breite in m.</param>
        public AccessoryItem(AccessoryPositon position, int category, bool female)
        {
            Position = position;
            String imagePath = PATH;
            double width;
            switch (category)
            {
                // Beach
                case 1:
                    imagePath += "Hat_Beach.png";
                    width = 0.24;
                    break;
                // Ski
                case 2:
                    imagePath += "Hat_Ski.png";
                    width = 0.255;
                    break;
                // City
                case 3:
                    if (female)
                    {
                        imagePath += "Hat_City_Female.png";
                        width = 0.225;
                    }
                    else
                    {
                        imagePath += "Hat_City_Male.png";
                        width = 0.2;
                    }
                    break;
                // Wander
                case 4:
                    imagePath += "Hat_Wander.png";
                    width = 0.27;
                    break;
                default:
                    imagePath += "Hat_Default.png";
                    width = 0.225;
                    break;
            }
            Image = new BitmapImage(new Uri(@imagePath, UriKind.RelativeOrAbsolute));
            Width = width;
        }

        public AccessoryPositon Position { get; private set; }
        public ImageSource Image { get; private set; }
        public double Width { get; private set; }
    }
}
