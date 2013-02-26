using Microsoft.Kinect;
using SkyBiometry.Client.FC;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenderDetector
{
    /// <summary>
    /// Klasse zur Alterbestimmung.
    /// </summary>
    public partial class GenderDetectorControl
    {
        private KinectSensor _kinectSensor;
        private WriteableBitmap colorBitmap;

        private FCClient client;

        private FCResult result;

        private String gender { get; set; }

        private String confidence { get; set; }

        private Skeleton activeSkeleton;

        public GenderDetectorControl()
        {
            InitializeComponent();
        }



        public void Start(KinectSensor sensor)
        {
            _kinectSensor = sensor;
            this.colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.Image.Source = this.colorBitmap;
        }

        /// <summary>
        /// EventHandler zum Zeichnen des Bildes.
        /// </summary>
        public void SensorColorFrameReady(Skeleton skeleton, byte[] colorImagePoints)
        {
            // Write the pixel data into our bitmap

            this.colorBitmap.WritePixels(
                new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                colorImagePoints,
                this.colorBitmap.PixelWidth * sizeof(int),
                0);
            activeSkeleton = skeleton;

        }

        /// <summary>
        /// Hauptfunction zur Altersbestimmung.
        /// </summary>
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

        /// <summary>
        /// Initialisiert den Rest Service.
        /// </summary>
        private void InitializeService()
        {
            client = new FCClient("5f228f0a0ce14e86a7c901f62ca5a569", "1231e9f2e95d4d90adf436e3de20f0f6");
            result = client.Account.EndAuthenticate(client.Account.BeginAuthenticate(null, null));
        }

        /// <summary>
        /// Schneidet den Kopf aus der colorBitmap wenn eine Person erkannt wird.
        /// </summary>
        private CroppedBitmap CropBitmap(WriteableBitmap colorBitmap) 
        {
            int width = 240;
            int height = 340;

            // Ersten Player selektieren
            if (activeSkeleton != null)
            {
                // Punkte des Kopfes auf ColorPoints mappen
                var point = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(activeSkeleton.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution1280x960Fps12);
                    
                // Überprüfung das nicht außerhalb des Bildes ausgenschnitten wird
                if ((int)point.X - 140 <= 0 || (int)point.Y - 140 >= 960)
                    return null;
                // Array für auszuschneidendes Bild initialisierne
                Int32Rect cropRect =
                    new Int32Rect((int)point.X - 140, (int)point.Y - 140, width, height);

                // Neues Bild erstellen und zurückliefern
                return new CroppedBitmap(this.colorBitmap, cropRect);
            }
            return null;
        }

        /// <summary>
        /// Speichert den Screenshort zur späteren Verwendung.
        /// </summary>
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
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fs);
            }

            return path;
        }

        /// <summary>
        /// Sendet das Bild an den Rest Service und speichert das Ergebnis.
        /// </summary>
        private void CalculateGender(String path)
        {
            Stream stream = System.IO.File.OpenRead(path);
            result = client.Faces.EndDetect(client.Faces.BeginDetect(null, new Stream[] { stream }, Detector.Normal, Attributes.All, null, null));
            stream.Close();
        }

        /// <summary>
        /// Setzt die Attribute und die WPF Elemente nach einem Request.
        /// </summary>
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
