//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.GreenScreen
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Bitmap that will hold opacity mask information
        /// </summary>
        private WriteableBitmap playerOpacityMaskImage = null;

        /// <summary>
        /// Intermediate storage for the depth data received from the sensor
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Intermediate storage for the green screen opacity mask
        /// </summary>
        private int[] greenScreenPixelData;

        /// <summary>
        /// Intermediate storage for the depth to color mapping
        /// </summary>
        private ColorImagePoint[] colorCoordinates;

        /// <summary>
        /// Inverse scaling factor between color and depth
        /// </summary>
        private int colorToDepthDivisor;

        /// <summary>
        /// Width of the depth image
        /// </summary>
        private int depthWidth;

        /// <summary>
        /// Height of the depth image
        /// </summary>
        private int depthHeight;

        /// <summary>
        /// Indicates opaque in an opacity mask
        /// </summary>
        private int opaquePixelValue = -1;
        private int colorHeight;
        private int colorWidth;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthFormat);

                this.depthWidth = this.sensor.DepthStream.FrameWidth;

                this.depthHeight = this.sensor.DepthStream.FrameHeight;

                this.sensor.ColorStream.Enable(ColorFormat);

                this.colorWidth = this.sensor.ColorStream.FrameWidth;
                this.colorHeight = this.sensor.ColorStream.FrameHeight;

                this.colorToDepthDivisor = colorWidth / this.depthWidth;

                // Turn on to get player masks
                this.sensor.SkeletonStream.Enable();

                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                this.greenScreenPixelData = new int[this.sensor.DepthStream.FramePixelDataLength];

                this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.MaskedColor.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.AllFramesReady += this.SensorAllFramesReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
                this.sensor = null;
            }
        }

        long frameCounter = 0;
        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, so nothing to do
            if (null == this.sensor)
            {
                return;
            }
            if (frameCounter++ % 2 == 0)
            {


                bool depthReceived = false;
                bool colorReceived = false;

                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        // Copy the pixel data from the image to a temporary array
                        depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                        depthReceived = true;
                    }
                }

                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        // Copy the pixel data from the image to a temporary array
                        colorFrame.CopyPixelDataTo(this.colorPixels);

                        colorReceived = true;
                    }
                }

                // do our processing outside of the using block
                // so that we return resources to the kinect as soon as possible
                if (true == depthReceived)
                {
                    this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                        DepthFormat,
                        this.depthPixels,
                        ColorFormat,
                        this.colorCoordinates);

                    Array.Clear(this.greenScreenPixelData, 0, this.greenScreenPixelData.Length);

                    // loop over each row and column of the depth
                    for (int y = 0; y < this.depthHeight; ++y)
                    {
                        for (int x = 0; x < this.depthWidth; ++x)
                        {
                            // calculate index into depth array
                            int depthIndex = x + (y * this.depthWidth);

                            DepthImagePixel depthPixel = this.depthPixels[depthIndex];
                            int player = depthPixel.PlayerIndex;

                            // if we're tracking a player for the current pixel, do green screen

                            if (player > 0)
                            //if(depthPixel.IsKnownDepth)
                            {
                                // retrieve the depth to color mapping for the current depth pixel
                                ColorImagePoint colorImagePoint = this.colorCoordinates[depthIndex];

                                /*int depthInColorX = colorImagePoint.X * 4;
                                int depthInColorY = colorImagePoint.Y * 4;
                            
                                if (depthInColorX > 0 && depthInColorX < colorWidth * 4 && depthInColorY >= 0 && depthInColorY < colorHeight * 4)
                                {
                                    colorPixels[(depthInColorX) + (depthInColorY) * colorWidth +1] = 255;
                                    colorPixels[(depthInColorX) + (depthInColorY) * colorWidth +5] = 255; 
                                }*/

                                // scale color coordinates to depth resolution
                                int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
                                int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;

                                // make sure the depth pixel maps to a valid point in color space
                                // check y > 0 and y <  to fill gaps we set opaque current pixel plus the one to the left
                                // because of how the sensor works it isdepthHeight to make sure we don't write outside of the array
                                // check x > 0 instead of >= 0 since more correct to do it this way than to set to the right
                                if (colorInDepthX > 0 && colorInDepthX < this.depthWidth && colorInDepthY >= 0 && colorInDepthY < this.depthHeight)
                                {
                                    // calculate index into the green screen pixel array
                                    int greenScreenIndex = colorInDepthX + (colorInDepthY * this.depthWidth);

                                    AddBorderPixels(greenScreenIndex);

                                    // set opaque
                                    this.greenScreenPixelData[greenScreenIndex] = opaquePixelValue;

                                    // compensate for depth/color not corresponding exactly by setting the pixel 
                                    // to the left to opaque as well
                                    this.greenScreenPixelData[greenScreenIndex - 1] = opaquePixelValue;


                                }
                            }
                        }
                    }
                }
                var borderPixelCounter = 0;
                for (int depthPixel = 0; depthPixel < greenScreenPixelData.Length; depthPixel++)
                {
                    var depthPixelValue = greenScreenPixelData[depthPixel];
                    if (depthPixelValue == BorderPoint)
                    {
                        int space = 7;
                        //if(borderPixelCounter % space == 0)
                        {
                            int topLeftDepthPixel = depthPixel - space - (depthWidth * space);
                            int topRightDepthPixel = depthPixel + space - (depthWidth * space);
                            int bottomLeftDepthPixel = depthPixel - space + (depthWidth * space);
                            int bottomRightDepthPixel = depthPixel + space + (depthWidth * space);

                            int backgroundDepthPixel = getBackgroundDepthPixel(topLeftDepthPixel, topRightDepthPixel, bottomLeftDepthPixel, bottomRightDepthPixel, depthPixel, space *2);


                            //if (isValidDepthPixel(topLeftDepthPixel) && !isPlayer(topLeftDepthPixel))
                            //{
                            //    color(topLeftDepthPixel, 255, 255, 0);
                            //}
                            //if (isValidDepthPixel(topRightDepthPixel) && !isPlayer(topRightDepthPixel))
                            //{
                            //    color(topRightDepthPixel, 255, 165, 0);
                            //}
                            //if (isValidDepthPixel(bottomLeftDepthPixel) && !isPlayer(bottomLeftDepthPixel))
                            //{
                            //    color(bottomLeftDepthPixel, 0, 255, 0);
                            //}
                            //if (isValidDepthPixel(bottomRightDepthPixel) && !isPlayer(bottomRightDepthPixel))
                            //{
                            //    color(bottomRightDepthPixel, 0, 0, 255);
                            //}

                            if (backgroundDepthPixel > -1)
                            {
                                rect(topLeftDepthPixel, backgroundDepthPixel, space * 2);
                                //color(backgroundDepthPixel, 255, 255, 255);
                                //color(depthPixel, 255, 0, 0);
                            }
                            else
                            {
                                //color(depthPixel, 0, 0, 0);
                            }
                        }

                        borderPixelCounter++;
                    }
                }

                //for (int depthPixel = 0; depthPixel < greenScreenPixelData.Length; depthPixel++)
                //{
                //    var depthPixelValue = greenScreenPixelData[depthPixel];
                //    if (depthPixelValue == NoPlayerPoint)
                //    {
                //        color(depthPixel, 255, 255, 255);
                //    }
                //}

                // do our processing outside of the using block
                // so that we return resources to the kinect as soon as possible
                if (true == colorReceived)
                {

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
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

                        MaskedColor.OpacityMask = new ImageBrush { ImageSource = this.playerOpacityMaskImage };
                    }

                    this.playerOpacityMaskImage.WritePixels(
                        new Int32Rect(0, 0, this.depthWidth, this.depthHeight),
                        this.greenScreenPixelData,
                        this.depthWidth * ((this.playerOpacityMaskImage.Format.BitsPerPixel + 7) / 8),
                        0);
                }
            }
        }

        int BackgroundPoint = 0;
        int OpaquePoint = -1;
        int BorderPoint = -2;

        private void AddBorderPixels(int greenScreenIndex)
        {
            int leftLeftPixel = greenScreenIndex - 2;
            int rightPixel = greenScreenIndex + 1;
            int topPixel = greenScreenIndex - depthWidth;
            int bottomPixel = greenScreenIndex + depthWidth;

            int leftTopPixel = topPixel - 1;
            int leftLeftTopPixrl = topPixel - 2;
            int rightTopPixel = topPixel + 1;
            int leftBottomPixel = bottomPixel - 1;
            int leftLeftBottomPixel = bottomPixel - 2;
            int rightBottomPixel = bottomPixel + 1;

            if (greenScreenPixelData[leftLeftPixel] != OpaquePoint)
            {
                greenScreenPixelData[leftLeftPixel] = BorderPoint;
            }

            if (greenScreenPixelData[leftTopPixel] != OpaquePoint)
            {
                greenScreenPixelData[leftTopPixel] = BorderPoint;
            }

            if (greenScreenPixelData[topPixel] != OpaquePoint)
            {
                greenScreenPixelData[topPixel] = BorderPoint;
            }

            if (greenScreenPixelData[rightTopPixel] != OpaquePoint)
            {
                greenScreenPixelData[rightTopPixel] = BorderPoint;
            }

            if (greenScreenPixelData[leftLeftPixel] != OpaquePoint)
            {
                greenScreenPixelData[leftLeftPixel] = BorderPoint;
            }

            if (rightPixel < greenScreenPixelData.Length)
            {
                greenScreenPixelData[rightPixel] = BorderPoint;
            }

            if (leftLeftBottomPixel < greenScreenPixelData.Length)
            {
                greenScreenPixelData[leftLeftBottomPixel] = BorderPoint;
            }

            if (leftBottomPixel < greenScreenPixelData.Length)
            {
                greenScreenPixelData[leftBottomPixel] = BorderPoint;
            }

            if (bottomPixel < greenScreenPixelData.Length)
            {
                greenScreenPixelData[bottomPixel] = BorderPoint;
            }

            if (rightBottomPixel < greenScreenPixelData.Length)
            {
                greenScreenPixelData[rightBottomPixel] = BorderPoint;
            }
        }

        private double colorDiff(int depthPixel1, int depthPixel2)
        {
            int b1Pos = depthPixel1 * 4;
            int g1Pos = b1Pos + 1;
            int r1Pos = g1Pos + 1;

            int b2Pos = depthPixel2 * 4;
            int g2Pos = b2Pos + 1;
            int r2Pos = g2Pos + 1;

            byte r1 = colorPixels[r1Pos];
            byte g1 = colorPixels[g1Pos];
            byte b1 = colorPixels[b1Pos];

            byte r2 = colorPixels[r2Pos];
            byte g2 = colorPixels[g2Pos];
            byte b2 = colorPixels[b2Pos];

            int rDiff = r1 - r2;
            int gDiff = g1 - g2;
            int bDiff = b1 - b2;
            return Math.Sqrt((rDiff * rDiff) + (gDiff * gDiff) + (bDiff* bDiff));
        }

        private void diag(int depthPixel, int direction)
        {
            int nextDepthPixel = depthPixel + direction;
            int maxSteps = 10;
            while (maxSteps > 0 && nextDepthPixel < greenScreenPixelData.Length && nextDepthPixel > 0 && colorDiff(depthPixel, nextDepthPixel) < 500)
            {
                color(nextDepthPixel, 0, 0, 0);
                maxSteps--;
                nextDepthPixel += direction;
            }
        }

        private void rect(int topLeftDepthPixel, int backgroundDepthPixel, int rectSize)
        {
            for (int y = 0; y < rectSize; y++)
            {
                for (int x = 0; x < rectSize; x++)
                {
                    int currentdepthPixel = topLeftDepthPixel + x + (y * depthWidth);
                    if (isValidDepthPixel(currentdepthPixel))
                    {
                        if (colorDiff(currentdepthPixel, backgroundDepthPixel) < 50)
                        {
                            greenScreenPixelData[currentdepthPixel] = BackgroundPoint;
                        }
                        else
                        {
                            greenScreenPixelData[currentdepthPixel] = OpaquePoint;
                        }
                    }
                }
            }
        }

        private double distance(int depthPixel1, int depthPixel2)
        {
            int dp1X = depthPixel1 % depthWidth;
            int dp1Y = depthPixel1 / depthWidth; 
            int dp2X = depthPixel2 % depthWidth;
            int dp2Y = depthPixel2 / depthWidth;

            int distX = dp1X - dp2X;
            int distY = dp1Y - dp2Y;

            return Math.Sqrt(distX * distX + distY * distY);

        }

        private int distanceToNextPlayerPixel(int depthPixel, int direction, int maxSteps)
        {
            for (int i = 0; i < maxSteps; i++)
            {
                depthPixel += direction;             
                if(!isValidDepthPixel(depthPixel) || isPlayer(depthPixel))
                {
                    return i;
                }
            }
            return maxSteps;
        }

        private bool isValidDepthPixel(int depthPixel)
        {
            return depthPixel > 0 && depthPixel < greenScreenPixelData.Length;
        }

        private bool isPlayer(int depthPixel)
        {
            return greenScreenPixelData[depthPixel] < 0;
        }

        private int getBackgroundDepthPixel(int topLeftDepthPixel, int topRightDepthPixel, int bottomLeftDepthPixel, int bottomRightDepthPixel, int referenceDepthPixel, int width)
        {

            int topLeftSteps = 0;
            int topRightSteps = 0;
            int bottomLeftSteps = 0;
            int bottomRightSteps = 0;

            if (isValidDepthPixel(topLeftDepthPixel) && !isPlayer(topLeftDepthPixel))
            {
                topLeftSteps += distanceToNextPlayerPixel(topLeftDepthPixel, +1, width);
                topLeftSteps += distanceToNextPlayerPixel(topLeftDepthPixel, +depthWidth, width);
            }
            if (isValidDepthPixel(topRightDepthPixel) && !isPlayer(topRightDepthPixel))
            {
                topRightSteps += distanceToNextPlayerPixel(topRightDepthPixel, -1, width);
                topRightSteps += distanceToNextPlayerPixel(topRightDepthPixel, +depthWidth, width);
            }
            if (isValidDepthPixel(bottomLeftDepthPixel) && !isPlayer(bottomLeftDepthPixel))
            {
                bottomLeftSteps += distanceToNextPlayerPixel(bottomLeftDepthPixel, +1, width);
                bottomLeftSteps += distanceToNextPlayerPixel(bottomLeftDepthPixel, -depthWidth, width);
            }
            if (isValidDepthPixel(bottomRightDepthPixel) && !isPlayer(bottomRightDepthPixel))
            {
                bottomRightSteps += distanceToNextPlayerPixel(bottomRightDepthPixel, -1, width);
                bottomRightSteps += distanceToNextPlayerPixel(bottomRightDepthPixel, -depthWidth, width);
            }

            int stepsMax = 0;
            int backgroundDepthPixel = -1;

            if (topLeftSteps > stepsMax)
            {
                stepsMax = topLeftSteps;
                backgroundDepthPixel = topLeftDepthPixel;
            }
            if (topRightSteps > stepsMax)
            {
                stepsMax = topRightSteps;
                backgroundDepthPixel = topRightDepthPixel;
            }
            if (bottomLeftSteps > stepsMax)
            {
                stepsMax = bottomLeftSteps;
                backgroundDepthPixel = bottomLeftDepthPixel;
            }
            if (bottomRightSteps > stepsMax)
            {
                stepsMax = bottomRightSteps;
                backgroundDepthPixel = bottomRightDepthPixel;
            }

            return backgroundDepthPixel;
        }

        private void color(int depthPixel, byte r, byte g, byte b)
        {
            int bPos = depthPixel * 4;
            int gPos = bPos + 1;
            int rPos = gPos + 1;

            colorPixels[rPos] = r;
            colorPixels[gPos] = g;
            colorPixels[bPos] = b;
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = this.sensor.ColorStream.FrameWidth;
            int colorHeight = this.sensor.ColorStream.FrameHeight;

            // create a render target that we'll render our controls to
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                // render the backdrop
                VisualBrush backdropBrush = new VisualBrush(Backdrop);
                dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

                // render the color image masked out by players
                VisualBrush colorBrush = new VisualBrush(MaskedColor);
                dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);
    
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format("{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.statusBarText.Text = string.Format("{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}