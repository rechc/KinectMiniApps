using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace GreenScreenControl
{
    /// <summary>
    /// Interaktionslogik für GreenScreenControl.xaml
    /// </summary>
    public partial class GreenScreenControl : UserControl
    {
        public KinectSensor Sensor { get; set; }

        private int[] greenScreenPixelData;
        private ColorImagePoint[] colorCoordinates;

        private int opaquePixelValue = -1;

        private int[] opaqueMatrix = new int[16];
        private int opaqueMatrixLenghtSqrt;
        private int widthRange;
        private double opaqueRange;

        private int colorToDepthDivisor;
        private int depthWidth;
        private int depthHeight;
        private WriteableBitmap colorBitmap;
        private WriteableBitmap playerOpacityMaskImage = null;

        public GreenScreenControl()
        {
            InitializeComponent();
        }

        public void Start(KinectSensor sensor)
        {
            this.Sensor = sensor;
            opaqueMatrixLenghtSqrt = Convert.ToInt32(Math.Sqrt(opaqueMatrix.Length));
            widthRange = opaqueMatrixLenghtSqrt - 1;
            opaqueRange = 3.0 / 4.0 * opaqueMatrix.Length;

            this.depthWidth = Sensor.DepthStream.FrameWidth;
            this.depthHeight = Sensor.DepthStream.FrameHeight;

            int colorWidth = Sensor.ColorStream.FrameWidth;
            int colorHeight = Sensor.ColorStream.FrameHeight;

            this.greenScreenPixelData = new int[Sensor.DepthStream.FramePixelDataLength];

            this.colorToDepthDivisor = colorWidth / this.depthWidth;

            this.colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            greenScreenPixelData = new int[Sensor.DepthStream.FramePixelDataLength];
            colorCoordinates = new ColorImagePoint[Sensor.DepthStream.FramePixelDataLength];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // Nicht ueber den Rand des Controls hinaus zeichnen.
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));
            if(depthPixels != null && colorPixels != null)
                Antialiasing(drawingContext);
        }

        public DepthImagePixel[] depthPixels { get; set; }
        public byte[] colorPixels { get; set; }

        public void Antialiasing(DrawingContext drawingContext) //DepthImagePixel[] depthPixels, byte[] colorPixels,DepthImageFormat depthFormat, ColorImageFormat colorFormat)
        {
                Sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthImageFormat.Resolution320x240Fps30,
                    depthPixels,
                    ColorImageFormat.RgbResolution640x480Fps30,
                    colorCoordinates);

            Array.Clear(greenScreenPixelData, 0, greenScreenPixelData.Length);

                // loop over each row and column of the depth
                for (int y = 0; y < depthHeight; ++y)
                {
                    for (int x = 0; x < depthWidth; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * depthWidth);

                        DepthImagePixel depthPixel = depthPixels[depthIndex];

                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, do green screen
                        if (player > 0)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = colorCoordinates[depthIndex];

                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
                            int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;

                            // make sure the depth pixel maps to a valid point in color space
                            // check y > 0 and y < depthHeight to make sure we don't write outside of the array
                            // check x > 0 instead of >= 0 since to fill gaps we set opaque current pixel plus the one to the left
                            // because of how the sensor works it is more correct to do it this way than to set to the right
                            if (colorInDepthX > 0 && colorInDepthX < depthWidth && colorInDepthY >= 0 && colorInDepthY < depthHeight)
                            {
                                // calculate index into the green screen pixel array
                                int greenScreenIndex = colorInDepthX + (colorInDepthY * depthWidth);

                                // set opaque
                                this.greenScreenPixelData[greenScreenIndex] = opaquePixelValue;

                                // compensate for depth/color not corresponding exactly by setting the pixel 
                                // to the left to opaque as well
                                this.greenScreenPixelData[greenScreenIndex - 1] = opaquePixelValue;
                            }
                        }
                    }

                    for (int i = 0; i < this.greenScreenPixelData.Length - (depthWidth * widthRange) - widthRange; i++)
                    {

                        //ToDo Werte können im Randbereich liegen

                        for (int j = 0; j < opaqueMatrixLenghtSqrt; j++)
                        {
                            for (int k = 0; k < opaqueMatrixLenghtSqrt; k++)
                            {
                                opaqueMatrix[j * opaqueMatrixLenghtSqrt + k] = i + k + depthWidth * j;

                            }
                        }

                        int counterFound = 0;
                        int counterNotFound = 0;

                        for (int j = 0; j < opaqueMatrix.Length; j++)
                        {
                            var p = this.greenScreenPixelData[opaqueMatrix[j]];
                            if (p == -1)
                            {
                                counterFound++;
                            }
                            else
                            {
                                counterNotFound++;
                            }
                            if (counterFound >= opaqueRange)
                            {
                                this.greenScreenPixelData[opaqueMatrix[0]] = -1;
                                break;
                            }
                            if (counterNotFound > opaqueMatrixLenghtSqrt)
                            {
                                break;
                            }
                        }
                    }
            }

                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    colorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);

                if (this.playerOpacityMaskImage == null)
                {
                    this.playerOpacityMaskImage = new WriteableBitmap(
                        depthWidth,
                        depthHeight,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null);

                    drawingContext.DrawImage(playerOpacityMaskImage, new Rect(0, 0, depthWidth, depthHeight));
                    //var img = new ImageBrush { ImageSource = this.playerOpacityMaskImage };
                }

                this.playerOpacityMaskImage.WritePixels(
                    new Int32Rect(0, 0, depthWidth, depthHeight),
                    this.greenScreenPixelData,
                    depthWidth * ((this.playerOpacityMaskImage.Format.BitsPerPixel + 7) / 8),
                    0);

            InvalidateVisual();
        }
    }
}
