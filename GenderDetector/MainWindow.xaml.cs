using HtwKinect;
using SkyBiometry.Client.FC;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace GenderDetector
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectHelper kh;

        private WriteableBitmap colorBitmap;

        private FCClient client;

        private FCResult result;

        private String gender { get; set; }

        private String confidence { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeSensor();
        }

        private void InitializeSensor()
        {
            kh = KinectHelper.GetInstance();
            this.colorBitmap = new WriteableBitmap(kh.GetSensor().ColorStream.FrameWidth, kh.GetSensor().ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.Image.Source = this.colorBitmap;
            kh.AllFramesDispatchedEvent += SensorColorFrameReady;
        }

        private void SensorColorFrameReady(object sender, EventArgs e)
        {
            // Write the pixel data into our bitmap
            this.colorBitmap.WritePixels(
                new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                kh.GetColorPixels(),
                this.colorBitmap.PixelWidth * sizeof(int),
                0);
        }

        private void InitializeConnection()
        {
            client = new FCClient("5f228f0a0ce14e86a7c901f62ca5a569", "1231e9f2e95d4d90adf436e3de20f0f6");
            result = client.Account.EndAuthenticate(client.Account.BeginAuthenticate(null, null));
        }

        private void SetAttributes(String path)
        {
            Stream stream = System.IO.File.OpenRead(path);
            result = client.Faces.EndDetect(client.Faces.BeginDetect(null, new Stream[] { stream }, Detector.Normal, Attributes.Gender, null, null));
            stream.Close();

            if (result.Photos[0].Tags.Count == 0)
            {
                this.GenderText.Text = "No face tracked";
            }
            else
            {
                this.gender = result.Photos[0].Tags[0].Attributes.Gender.Value + "";
                this.confidence = result.Photos[0].Tags[0].Attributes.Gender.Confidence + "";

                this.GenderText.Text = this.gender + "\t" + this.confidence;
            }
        }

        private void GenderCheck(object sender, RoutedEventArgs e)
        {
            InitializeConnection();

            String path = SaveScreenshot();

            SetAttributes(path);

            File.Delete(path);
        }

        private String SaveScreenshot()
        {
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new JpegBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            String time = System.DateTime.Now.ToString("hh'-'mm'-'ss");
            String myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            String path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            FileStream fs;
            using (fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }
            fs.Close();

            return path;
        }
    }

}
