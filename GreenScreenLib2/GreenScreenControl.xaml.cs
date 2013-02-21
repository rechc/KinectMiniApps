using Microsoft.Kinect;
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

namespace GreenScreenLib2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GreenScreenControl : UserControl
    {
        private KinectSensor _sensor;

        private int[] _greenScreenPixelData;
        private ColorImagePoint[] _colorCoordinates;

        private byte[] _bitmapBits;

        private int _colorToDepthDivisor;
        private int _depthWidth;
        private int _depthHeight;
        private WriteableBitmap _colorBitmap;
        private WriteableBitmap _playerOpacityMaskImage;

        private DepthImagePixel[] _depthPixels;
        private byte[] _colorPixels;

        //Variables for antialiasing
        private int[] _border;
        private int _borderCounter;

        private const int Top = 1;
        private const int Right = 2;
        private const int Bottom = 4;
        private const int Left = 8;

        private const int TopRight = Top + Right;
        private const int RightBottom = Right + Bottom;
        private const int BottomLeft = Bottom + Left;
        private const int LeftTop = Left + Top;

        private const int OpaquePoint = -1;
        private const int BorderPoint = -2;
        private const int CornerPoint = -3; // BROWN POINT
        private const int LinePoint = -4;
        private const int StairPoint = -5;

        private bool _antialiasing;

        public GreenScreenControl()
        {
            InitializeComponent();
        }

        /*
         * Called once at start
         */
        public void Start(KinectSensor sensor, bool antialiasing)
        {
            _antialiasing = antialiasing;
            _sensor = sensor;

            _depthWidth = _sensor.DepthStream.FrameWidth;
            _depthHeight = _sensor.DepthStream.FrameHeight;

            int colorWidth = _sensor.ColorStream.FrameWidth;
            int colorHeight = _sensor.ColorStream.FrameHeight;

            _greenScreenPixelData = new int[_sensor.DepthStream.FramePixelDataLength];

            _bitmapBits = new byte[colorWidth * colorHeight * 4];

            _colorToDepthDivisor = colorWidth / _depthWidth;

            _colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            _border = new int[_greenScreenPixelData.Length];

            _greenScreenPixelData = new int[_sensor.DepthStream.FramePixelDataLength];
            _colorCoordinates = new ColorImagePoint[_sensor.DepthStream.FramePixelDataLength];
        }


        public void RenderImageData(DepthImagePixel[] depthPixels, byte[] colorPixels)
        {
            _depthPixels = depthPixels;
            _colorPixels = colorPixels;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // Nicht ueber den Rand des Controls hinaus zeichnen.
            //drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            if (_depthPixels != null && _colorPixels != null)
                Antialiasing(drawingContext);
        }

        public void Antialiasing(DrawingContext drawingContext)
        {
            _sensor.CoordinateMapper.MapDepthFrameToColorFrame(_sensor.DepthStream.Format, 
                                                    _depthPixels, 
                                                    _sensor.ColorStream.Format, 
                                                    _colorCoordinates);

            Array.Clear(_greenScreenPixelData, 0, _greenScreenPixelData.Length);
            _borderCounter = 0;

            for (int i = 0; i < _depthPixels.Length; i++)
            {
                int depthVal = _depthPixels[i].Depth; // >> DepthImageFrame.PlayerIndexBitmaskWidth;
                int player = _depthPixels[i].PlayerIndex;

                // Put in the overlay of, say, depth values < 2,5 meters.       
                if (player > 0 && (depthVal < 2500) && (depthVal > 400))
                {
                    ColorImagePoint colorImagePoint = _colorCoordinates[i];

                    // scale color coordinates to depth resolution
                    int colorInDepthX = colorImagePoint.X / _colorToDepthDivisor;
                    int colorInDepthY = colorImagePoint.Y / _colorToDepthDivisor;

                    if (colorInDepthX > 0 && colorInDepthX < _depthWidth && colorInDepthY >= 0 && colorInDepthY < _depthHeight)
                    //if ((colorImagePoint.X >= 0 && colorImagePoint.X < _depthWidth) && (colorImagePoint.Y >= 0 && colorImagePoint.Y < _depthHeight))
                    {
                        // calculate index into the green screen pixel array
                        int greenScreenIndex = colorInDepthX + (colorInDepthY * _depthWidth);

                        if (_antialiasing)
                        {
                            AddBorderPixels(greenScreenIndex);
                            _greenScreenPixelData[greenScreenIndex] = OpaquePoint;
                        }
                        else
                        {

                            _greenScreenPixelData[greenScreenIndex] = OpaquePoint;
                            _greenScreenPixelData[greenScreenIndex - 1] = OpaquePoint;
                        }
                    }
                }
            }

            if (_antialiasing)
            {
                Antialiasing();
                //HidePixels();
            }

            _colorBitmap.WritePixels(
                        new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
                        _colorPixels,
                        _colorBitmap.PixelWidth * sizeof(int), 0);

            if (_playerOpacityMaskImage == null)
            {
                _playerOpacityMaskImage = new WriteableBitmap(
                    _depthWidth,
                    _depthHeight,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null);
            }

            _playerOpacityMaskImage.WritePixels(
                new Int32Rect(0, 0, _depthWidth, _depthHeight),
                _greenScreenPixelData,
                _depthWidth * ((_playerOpacityMaskImage.Format.BitsPerPixel) / 8),
                0);

            drawingContext.PushOpacityMask(new ImageBrush { ImageSource = _playerOpacityMaskImage});
            drawingContext.DrawImage(_colorBitmap, new Rect(0, 0, ActualWidth, ActualHeight)); 
        }

          private void AddBorderPixels(int greenScreenIndex)
        {
            int leftPixel = greenScreenIndex - 1;
            int rightPixel = greenScreenIndex + 1;
            int topPixel = greenScreenIndex - _depthWidth;
            int bottomPixel = greenScreenIndex + _depthWidth;

            int leftTopPixel = topPixel - 1;
            int rightTopPixel = topPixel + 1;
            int leftBottomPixel = bottomPixel - 1;
            int rightBottomPixel = bottomPixel + 1;

            if (_greenScreenPixelData[leftPixel] != OpaquePoint)
            {
                _greenScreenPixelData[leftPixel] = BorderPoint;
            }

            if (_greenScreenPixelData.Length > rightPixel)
            {
                _greenScreenPixelData[rightPixel] = BorderPoint;
            }


            if (_greenScreenPixelData[leftTopPixel] != OpaquePoint)
            {
                _greenScreenPixelData[leftTopPixel] = BorderPoint;
            }

            if (_greenScreenPixelData[rightTopPixel] != OpaquePoint)
            {
                _greenScreenPixelData[rightTopPixel] = BorderPoint;
            }

            if (_greenScreenPixelData[topPixel] != OpaquePoint)
            {
                _greenScreenPixelData[topPixel] = BorderPoint;
            }

            if (_greenScreenPixelData.Length > bottomPixel)
            {
                _greenScreenPixelData[bottomPixel] = BorderPoint;
                _greenScreenPixelData[leftBottomPixel] = BorderPoint;
            }

            if (_greenScreenPixelData.Length > rightBottomPixel)
            {

                _greenScreenPixelData[rightBottomPixel] = BorderPoint;
            }
        }

        private void HidePixels()
        {
            for (int i = 0; i < _greenScreenPixelData.Length; i++)
            {
                if (_greenScreenPixelData[i] == -1 || _greenScreenPixelData[i] == BorderPoint)
                {
                    _greenScreenPixelData[i] = 0;
                }
            }
        }

        private void Antialiasing()
        {
            for (int i = 0; i < _greenScreenPixelData.Length; i++)
            {
                if (_greenScreenPixelData[i] == -2)
                {
                    _border[_borderCounter++] = i;
                }
            }

            for (int i = 0; i < _borderCounter; i++)
            {
                int opaqueFound = 0;
                int idx = _border[i];
                if (idx > _depthWidth && idx < (_depthWidth - 1) * _depthHeight)
                {
                    if (_greenScreenPixelData[idx - _depthWidth] == OpaquePoint)
                    {
                        // Top 1
                        opaqueFound += Top;
                    }
                    if (_greenScreenPixelData[idx + 1] == OpaquePoint)
                    {
                        // Right 2
                        opaqueFound += Right;
                    }
                    if (_greenScreenPixelData.Length > idx + _depthWidth && _greenScreenPixelData[idx + _depthWidth] == OpaquePoint)
                    {
                        // Bottom 4
                        opaqueFound += Bottom;
                    }
                    if (_greenScreenPixelData[idx - 1] == OpaquePoint)
                    {
                        // Left 8
                        opaqueFound += Left;
                    }
                    if (opaqueFound >= 2)
                    {
                        _greenScreenPixelData[idx] = CornerPoint;
                        int hLength;
                        int vLength;
                        double diff;
                        int pointsToDraw;
                        switch (opaqueFound)
                        {
                            case TopRight:
                                hLength = LeftSearch(idx, -1);
                                vLength = BottomSearch(idx, 1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / (double)vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            _greenScreenPixelData[idx + ((y + 1) * _depthWidth) - 1 - x] = StairPoint;
                                        }
                                    }
                                }
                                break;
                            case RightBottom:
                                hLength = LeftSearch(idx, 1);
                                vLength = TopSearch(idx, 1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / (double)vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            _greenScreenPixelData[idx - ((y + 1) * _depthWidth) - 1 - x] = StairPoint;
                                        }
                                    }
                                }
                                break;
                            case BottomLeft:
                                hLength = RightSearch(idx, -1);
                                vLength = TopSearch(idx, -1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / (double)vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            _greenScreenPixelData[idx - ((y + 1) * _depthWidth) + 1 + x] = StairPoint;
                                        }
                                    }
                                }
                                break;
                            case LeftTop:
                                hLength = RightSearch(idx, -1);
                                vLength = BottomSearch(idx, -1);
                                // DRAW stairs
                                if (hLength != 0 && vLength != 0)
                                {
                                    diff = hLength / (double)vLength;
                                    for (int y = 0; y < vLength - 1; y++)
                                    {
                                        pointsToDraw = (int)Math.Round((vLength - y) * diff) - 1;
                                        for (int x = 0; x < pointsToDraw; x++)
                                        {
                                            _greenScreenPixelData[idx + ((y + 1) * _depthWidth) + 1 + x] = StairPoint;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        _greenScreenPixelData[idx] = 0;
                    }
                }
            }
        }

        private int TopSearch(int index, int site)
        {
            Boolean found = false;
            int x = 0;
            int idx = index - (_depthWidth * x) + site;
            while (!found && idx < _greenScreenPixelData.Length)
            {
                if (_greenScreenPixelData[idx] == -1)
                {
                    _greenScreenPixelData[idx - site] = LinePoint;
                    x++;
                    idx -= _depthWidth;
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
                if (_greenScreenPixelData[index + x + (site * _depthWidth)] == -1)
                {
                    _greenScreenPixelData[index + x] = LinePoint;
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
            int idx = index + (_depthWidth * x) + site;
            while (!found && idx < _greenScreenPixelData.Length)
            {

                if (_greenScreenPixelData[idx] == -1)
                {
                    _greenScreenPixelData[idx - site] = LinePoint;
                    x++;
                    idx += _depthWidth;
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
                if (_greenScreenPixelData[index - x + (site * _depthWidth)] == -1)
                {
                    _greenScreenPixelData[index - x] = LinePoint;
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

