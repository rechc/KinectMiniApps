using HtwKinect;
using Microsoft.Kinect;
using SkyBiometry.Client.FC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /**
     * Klasse zur Alterbestimmung
     */
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

        /**
         * Initialisiert die verschiedenen Sensoren und Attribute
         */
        private void InitializeSensor()
        {
            kh = KinectHelper.GetInstance();
            this.colorBitmap = new WriteableBitmap(kh.GetSensor().ColorStream.FrameWidth, kh.GetSensor().ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.Image.Source = this.colorBitmap;
            kh.AllFramesDispatchedEvent += SensorColorFrameReady;
        }

        /**
         * EventHandler zum Zeichnen des Bildes
         */
        private void SensorColorFrameReady(object sender, EventArgs e)
        {
            // Write the pixel data into our bitmap
            this.colorBitmap.WritePixels(
                new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                kh.GetColorPixels(),
                this.colorBitmap.PixelWidth * sizeof(int),
                0);
        }

        /**
         * Hauptfunction zur Altersbestimmung
         */
        private void GenderCheck(object sender, RoutedEventArgs e)
        {
            new Thread((ThreadStart)delegate
            {
                // Rest Service initaliesieren
                InitializeService();

                // Bild speichern
                String path = "";
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    CroppedBitmap colorBitmap = CropBitmap(this.colorBitmap);
                    if (colorBitmap == null)
                    {
                        path = SaveScreenshot(this.colorBitmap);
                    }
                    else
                    {
                        path = SaveScreenshot(colorBitmap);
                    }
                }));

                // Warten bis Bild gespeichert wurde
                while (path == "") { }

                // Auswertung des Bildes
                CalculateGender(path);

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    // Attribute setzen
                    SetAttributes();
                    // Bild wieder löschen
                    File.Delete(path);
                }));
            }).Start();
        }

        /**
         * Initialisiert den Rest Service
         */
        private void InitializeService()
        {
            client = new FCClient("5f228f0a0ce14e86a7c901f62ca5a569", "1231e9f2e95d4d90adf436e3de20f0f6");
            result = client.Account.EndAuthenticate(client.Account.BeginAuthenticate(null, null));
        }

        /**
         * Schneidet den Kopf aus der colorBitmap wenn eine Person erkannt wird
         */
        private CroppedBitmap CropBitmap(WriteableBitmap colorBitmap) 
        {
            int width = 120;
            int height = 120;

            if (kh.GetSkeletons().Count(t => t.TrackingState == SkeletonTrackingState.Tracked) > 0)
            {
                // Ersten Player selektieren
                Skeleton player = kh.GetSkeletons().First(p => p.TrackingState == SkeletonTrackingState.Tracked);
                if (player != null)
                {
                    // Punkte des Kopfes auf ColorPoints mappen
                    var point = kh.GetSensor().CoordinateMapper.MapSkeletonPointToColorPoint(player.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    
                    // Überprüfung das nicht außerhalb des Bildes ausgenschnitten wird
                    if ((int)point.X - 70 <= 0 || (int)point.Y - 70 >= 470)
                        return null;
                    // Array für auszuschneidendes Bild initialisierne
                    Int32Rect cropRect =
                     new Int32Rect((int)point.X - 70, (int)point.Y - 70, width, height + 20);

                    // Neues Bild erstellen und zurückliefern
                    return new CroppedBitmap(this.colorBitmap, cropRect);
                }
            }
            return null;
        }

        /**
         * Speichert den Screenshort zur späteren Verwendung
         */
        private String SaveScreenshot(BitmapSource colorBitmap)
        {
            // Erzeugt den Encoder zum Speichern eines JPG
            BitmapEncoder encoder = new JpegBitmapEncoder();

            // Schreibt die colorBitmap in den Encoder
            encoder.Frames.Add(BitmapFrame.Create(colorBitmap));

            // Speicherpfad erzuegen
            String time = System.DateTime.Now.ToString("hh'-'mm'-'ss");
            String myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            String path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // Speichern des Bildes
            FileStream fs;
            using (fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }
            fs.Close();

            return path;
        }

        /**
         * Sendet das Bild an den Rest Service und speichert das Ergebnis
         */
        private void CalculateGender(String path)
        {
            Stream stream = System.IO.File.OpenRead(path);
            result = client.Faces.EndDetect(client.Faces.BeginDetect(null, new Stream[] { stream }, Detector.Normal, Attributes.Gender, null, null));
            stream.Close();
        }

        /**
         * Setzt die Attribute und die WPF Elemente nach einem Request
         */
        private void SetAttributes()
        {
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
    }
}
