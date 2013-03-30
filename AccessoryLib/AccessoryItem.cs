using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AccessoryLib
{
    /**
	 * Enum für Items 
     */
    public enum AccessoryPositon
    {
        Hat,
        Glasses,
        Beard
    }

    /**
	 * Klasse für die Accessories Items
     */
    public class AccessoryItem
    {
		// Pfad zu den Bildern
        private const String PATH = "../../../HtwKinect/Images/Accessories/";

        /**
	 	 * Konstruktor
         */
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
                    switch (new Random().Next(0, 2))
                    {
                        case 1:
                            imagePath += "Hat_Ski2.png";
                            width = 0.17;
                            break;
                        default:
                            imagePath += "Hat_Ski.png";
                            width = 0.255;
                            break;
                    }
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
                        width = 0.215;
                    }
                    break;
                // Wander
                case 4:
                    switch (new Random().Next(0, 2))
                    {
                        case 1:
                            imagePath += "Hat_Wander2.png";
                            width = 0.3;
                            break;
                        default:
                            imagePath += "Hat_Wander.png";
                            width = 0.27;
                            break;
                    }
                    break;
                default:
                    imagePath += "Hat_Default.png";
                    width = 0.225;
                    break;
            }
            Image = new BitmapImage(new Uri(@imagePath, UriKind.RelativeOrAbsolute));
            Width = width;
        }

        /**
	 	 * Konstruktor, für die AccessorySample Projekt
         */
        public AccessoryItem(AccessoryPositon position, String path)
        {
            Position = position;
            String imagePath = PATH + path;
            Image = new BitmapImage(new Uri(@imagePath, UriKind.RelativeOrAbsolute));
            Width = 0.2;
        }

        public AccessoryPositon Position { get; private set; }
        public ImageSource Image { get; private set; }
        public double Width { get; private set; }
    }
}
