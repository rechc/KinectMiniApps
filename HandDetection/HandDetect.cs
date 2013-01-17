using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HandDetection
{
    public class HandDetect
    {

        private Color PixelColor(ImageSource img, int pixelX, int pixelY)
        {
            CroppedBitmap cb = new CroppedBitmap((BitmapSource)img, new Int32Rect(pixelX, pixelY, 1, 1));
            byte[] pix = new byte[4];
            cb.CopyPixels(pix, 4, 0);
            return Color.FromRgb(pix[2], pix[1], pix[0]);
        }

        public bool IsMakingAFist(ImageSource imgHand)
        {
            bool wasBlack = false;
            int blackWidth = 0;
            int blackTimes = 0;

            for (int yy = 10; yy < imgHand.Height - 10; yy += 10)
            {
                for (int xx = 3; xx </*MaxX*/ imgHand.Width; xx++)
                {
                    if (PixelColor(imgHand, xx, yy) == Colors.Black)
                    {
                        if (!wasBlack)
                        {
                            if (blackWidth > 1 && blackWidth < 15)
                                blackTimes++;
                            blackWidth = 0;
                        }
                        else
                        {
                            blackWidth++;
                        }
                        wasBlack = true;
                    }
                    else
                    {
                        wasBlack = false;
                    }
                }
                if (blackTimes > 1) 
                    return false;
            }
            return true;
        }
    }
}
