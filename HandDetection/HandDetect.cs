using Microsoft.Kinect;
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
           // Color farbe = Color.FromRgb(pix[2], pix[1], pix[0]);
           // Console.WriteLine(farbe.ToString());
            return Color.FromRgb(pix[2], pix[1], pix[0]);
        }

        public bool IsMakingAFist(DepthImagePixel[] imgHand, DepthImagePoint handPos)
        {
            //Console.WriteLine(Colors.Gray.ToString());
            bool wasBlack = false;
            int blackWidth = 0;
            int blackTimes = 0;
            int ystart = handPos.Y-20;
            int yend = handPos.Y + 20;
            int xstart = handPos.X - 20;
            int xend = handPos.X + 20;

            for (int yy = ystart; yy < yend - 10; yy += 10)
            {
                for (int xx = xstart; xx < xend; xx++)
                {
                    int depthIndex = xx + (yy * 320);
                    DepthImagePixel depthPixel = imgHand[depthIndex];
                    int player = depthPixel.PlayerIndex;
                    if (player > 0)
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
