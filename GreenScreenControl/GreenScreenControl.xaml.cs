using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        private int colorToDepthDivisor;
        private int depthWidth;
        private int depthHeight;
        private WriteableBitmap colorBitmap;
        private WriteableBitmap playerOpacityMaskImage = null;

        public DepthImagePixel[] DepthPixels { get; set; }
        public byte[] ColorPixels { get; set; }

        //Variables for antialiasing
        private int[] opaqueMatrix = new int[16];
        private int opaqueMatrixLenghtSqrt;
        private int[] border;
        private int borderCounter = 0;

        private const int TOP = 1;
        private const int RIGHT = 2;
        private const int BOTTOM = 4;
        private const int LEFT = 8;

        private const int TOP_RIGHT = TOP + RIGHT;
        private const int RIGHT_BOTTOM = RIGHT + BOTTOM;
        private const int BOTTOM_LEFT = BOTTOM + LEFT;
        private const int LEFT_TOP = LEFT + TOP;

        private const int OPAQUE_POINT = -1;
        private const int BORDER_POINT = -2;
        private const int CORNER_POINT = -3; // BROWN POINT
        private const int LINE_POINT = -4;
        private const int STAIR_POINT = -5;

        private bool antialiasing = true;

        private DepthImageFormat depthImageFormat;
        private ColorImageFormat colorImageFormat;

        public void InvalidateVisual(DepthImagePixel[] depthPixels, byte[] colorPixels)
        {
            DepthPixels = depthPixels;
            ColorPixels = colorPixels;
            InvalidateVisual();
        }

        public GreenScreenControl()
        {
            InitializeComponent();
        }

        public void Start(KinectSensor sensor, DepthImageFormat depthFormat, ColorImageFormat colorFormat)
        {
            this.Sensor = sensor;
            this.depthImageFormat = depthFormat;
            this.colorImageFormat = colorFormat;

            opaqueMatrixLenghtSqrt = Convert.ToInt32(Math.Sqrt(opaqueMatrix.Length));

            this.depthWidth = Sensor.DepthStream.FrameWidth;
            this.depthHeight = Sensor.DepthStream.FrameHeight;

            int colorWidth = Sensor.ColorStream.FrameWidth;
            int colorHeight = Sensor.ColorStream.FrameHeight;

            this.greenScreenPixelData = new int[Sensor.DepthStream.FramePixelDataLength];

            this.colorToDepthDivisor = colorWidth / this.depthWidth;

            this.colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            border = new int[this.greenScreenPixelData.Length];

            greenScreenPixelData = new int[Sensor.DepthStream.FramePixelDataLength];
            colorCoordinates = new ColorImagePoint[Sensor.DepthStream.FramePixelDataLength];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // Nicht ueber den Rand des Controls hinaus zeichnen.
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));
            if(DepthPixels != null && ColorPixels != null)
                Antialiasing(drawingContext);
        }

        public void Antialiasing(DrawingContext drawingContext) //DepthImagePixel[] depthPixels, byte[] colorPixels,DepthImageFormat depthFormat, ColorImageFormat colorFormat)
        {
            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
                this.Sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    depthImageFormat,
                    this.DepthPixels,
                    colorImageFormat,
                    this.colorCoordinates);

                Array.Clear(this.greenScreenPixelData, 0, this.greenScreenPixelData.Length);

                borderCounter = 0;

                // loop over each row and column of the depth
                for (int y = 0; y < this.depthHeight; ++y)
                {
                    for (int x = 0; x < this.depthWidth; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * this.depthWidth);

                        DepthImagePixel depthPixel = this.DepthPixels[depthIndex];

                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, do green screen
                        if (player > 0)
                        {
                            //found = true;

                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = this.colorCoordinates[depthIndex];

                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
                            int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;

                            // make sure the depth pixel maps to a valid point in color space
                            // check y > 0 and y < depthHeight to make sure we don't write outside of the array
                            // check x > 0 instead of >= 0 since to fill gaps we set opaque current pixel plus the one to the left
                            // because of how the sensor works it is more correct to do it this way than to set to the right
                            if (colorInDepthX > 0 && colorInDepthX < this.depthWidth && colorInDepthY >= 0 && colorInDepthY < this.depthHeight)
                            {
                                // calculate index into the green screen pixel array
                                int greenScreenIndex = colorInDepthX + (colorInDepthY * this.depthWidth);

                                if (antialiasing)
                                {
                                    AddBorderPixels(greenScreenIndex);
                                    this.greenScreenPixelData[greenScreenIndex] = OPAQUE_POINT;
                                }
                                else
                                {

                                    this.greenScreenPixelData[greenScreenIndex] = OPAQUE_POINT;
                                    this.greenScreenPixelData[greenScreenIndex - 1] = OPAQUE_POINT;
                                }
                            }
                        }
                    }
                }

                if (antialiasing)
                {
                    Antialiasing();
                    //HidePixels();
                }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.ColorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);

                if (this.playerOpacityMaskImage == null)
                {
                    this.playerOpacityMaskImage = new WriteableBitmap(
                        this.depthWidth,
                        this.depthHeight,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null);

                    //MaskedColor.OpacityMask = new ImageBrush { ImageSource = this.playerOpacityMaskImage };
                    drawingContext.PushOpacityMask(new ImageBrush() { ImageSource = this.playerOpacityMaskImage});
                }

                this.playerOpacityMaskImage.WritePixels(
                    new Int32Rect(0, 0, this.depthWidth, this.depthHeight),
                    this.greenScreenPixelData,
                    this.depthWidth * ((this.playerOpacityMaskImage.Format.BitsPerPixel + 7) / 8),
                    0);

            drawingContext.DrawImage(playerOpacityMaskImage, new Rect(0, 0, depthWidth, depthHeight));
        }

        private void AddBorderPixels(int greenScreenIndex)
        {
            int leftPixel = greenScreenIndex - 1;
            int rightPixel = greenScreenIndex + 1;
            int topPixel = greenScreenIndex - this.depthWidth;
            int bottomPixel = greenScreenIndex + this.depthWidth;

            int leftTopPixel = topPixel - 1;
            int rightTopPixel = topPixel + 1;
            int leftBottomPixel = bottomPixel - 1;
            int rightBottomPixel = bottomPixel + 1;

            if (this.greenScreenPixelData[leftPixel] != OPAQUE_POINT)
            {
                this.greenScreenPixelData[leftPixel] = BORDER_POINT;
            }

            if (greenScreenPixelData.Length > rightPixel)
            {
                this.greenScreenPixelData[rightPixel] = BORDER_POINT;
            }


            if (this.greenScreenPixelData[leftTopPixel] != OPAQUE_POINT)
            {
                this.greenScreenPixelData[leftTopPixel] = BORDER_POINT;
            }

            if (this.greenScreenPixelData[rightTopPixel] != OPAQUE_POINT)
            {
                this.greenScreenPixelData[rightTopPixel] = BORDER_POINT;
            }

            if (this.greenScreenPixelData[topPixel] != OPAQUE_POINT)
            {
                this.greenScreenPixelData[topPixel] = BORDER_POINT;
            }

            if (greenScreenPixelData.Length > bottomPixel)
            {
                this.greenScreenPixelData[bottomPixel] = BORDER_POINT;
                this.greenScreenPixelData[leftBottomPixel] = BORDER_POINT;
            }

            if (greenScreenPixelData.Length > rightBottomPixel)
            {

                this.greenScreenPixelData[rightBottomPixel] = BORDER_POINT;
            }
        }

        private void HidePixels()
        {
            for (int i = 0; i < this.greenScreenPixelData.Length; i++)
            {
                if (this.greenScreenPixelData[i] == -1 || this.greenScreenPixelData[i] == BORDER_POINT)
                {
                    this.greenScreenPixelData[i] = 0;
                }
            }
        }

        private void Antialiasing()
        {
            for (int i = 0; i < this.greenScreenPixelData.Length; i++)
            {
                if (this.greenScreenPixelData[i] == -2)
                {
                    this.border[borderCounter++] = i;
                }
            }

            for (int i = 0; i < borderCounter; i++)
            {
                int opaqueFound = 0;
                int idx = border[i];
                if (idx > this.depthWidth && idx < (this.depthWidth - 1) * depthHeight)
                {
                    if (this.greenScreenPixelData[idx - this.depthWidth] == OPAQUE_POINT)
                    {
                        // Top 1
                        opaqueFound += TOP;
                    }
                    if (this.greenScreenPixelData[idx + 1] == OPAQUE_POINT)
                    {
                        // Right 2
                        opaqueFound += RIGHT;
                    }
                    if (this.greenScreenPixelData.Length > idx + this.depthWidth && this.greenScreenPixelData[idx + this.depthWidth] == OPAQUE_POINT)
                    {
                        // Bottom 4
                        opaqueFound += BOTTOM;
                    }
                    if (this.greenScreenPixelData[idx - 1] == OPAQUE_POINT)
                    {
                        // Left 8
                        opaqueFound += LEFT;
                    }
                    if (opaqueFound >= 2)
                    {
                        this.greenScreenPixelData[idx] = CORNER_POINT;
                        int hLength = 0;
                        int vLength = 0;
                        double diff = 0;
                        int pointsToDraw = 0;
                        switch (opaqueFound)
                        {
                            case TOP_RIGHT:
                                hLength = LeftSearch(idx, -1);
                                vLength = BottomSearch(idx, 1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            this.greenScreenPixelData[idx + ((y + 1) * this.depthWidth) - 1 - x] = STAIR_POINT;
                                        }
                                    }
                                }
                                break;
                            case RIGHT_BOTTOM:
                                hLength = LeftSearch(idx, 1);
                                vLength = TopSearch(idx, 1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            this.greenScreenPixelData[idx - ((y + 1) * this.depthWidth) - 1 - x] = STAIR_POINT;
                                        }
                                    }
                                }
                                break;
                            case BOTTOM_LEFT:
                                hLength = RightSearch(idx, -1);
                                vLength = TopSearch(idx, -1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            this.greenScreenPixelData[idx - ((y + 1) * this.depthWidth) + 1 + x] = STAIR_POINT;
                                        }
                                    }
                                }
                                break;
                            case LEFT_TOP:
                                hLength = RightSearch(idx, -1);
                                vLength = BottomSearch(idx, -1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            this.greenScreenPixelData[idx + ((y + 1) * this.depthWidth) + 1 + x] = STAIR_POINT;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        this.greenScreenPixelData[idx] = 0;
                    }
                }
            }
        }

        private int TopSearch(int index, int site)
        {
            Boolean found = false;
            int x = 0;
            int idx = index - (this.depthWidth * x) + site;
            while (!found && idx < greenScreenPixelData.Length)
            {
                if (this.greenScreenPixelData[idx] == -1)
                {
                    this.greenScreenPixelData[idx - site] = LINE_POINT;
                    x++;
                    idx -= this.depthWidth;
                }
                else
                {
                    found = true;
                }
            }
            return (x - 1);
        }

        private int RightSearch(int index, int site)
        {
            Boolean found = false;
            int x = 0;
            while (!found)
            {
                if (this.greenScreenPixelData[index + x + (site * this.depthWidth)] == -1)
                {
                    this.greenScreenPixelData[index + x] = LINE_POINT;
                    x++;
                }
                else
                {
                    found = true;
                }
            }
            return (x - 1);
        }

        private int BottomSearch(int index, int site)
        {
            Boolean found = false;
            int x = 0;
            int idx = index + (this.depthWidth * x) + site;
            while (!found && idx < this.greenScreenPixelData.Length)
            {

                if (this.greenScreenPixelData[idx] == -1)
                {
                    this.greenScreenPixelData[idx - site] = LINE_POINT;
                    x++;
                    idx += this.depthWidth;
                }
                else
                {
                    found = true;
                }
            }
            return (x - 1);
        }

        private int LeftSearch(int index, int site)
        {
            Boolean found = false;
            int x = 0;
            while (!found)
            {
                if (this.greenScreenPixelData[index - x + (site * this.depthWidth)] == -1)
                {
                    this.greenScreenPixelData[index - x] = LINE_POINT;
                    x++;
                }
                else
                {
                    found = true;
                }
            }
            return (x - 1);
        }
    }
}
