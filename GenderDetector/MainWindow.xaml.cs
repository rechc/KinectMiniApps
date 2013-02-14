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
    /**
     * Klasse zur Alterbestimmung
     */
    public partial class MainWindow
    {
        private KinectSensor _kinectSensor;
        private WriteableBitmap _colorBitmap;
        private FCClient _client;
        private FCResult _result;
        private Skeleton _activeSkeleton;

        public String Gender { get; set; }
        public String Confidence { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }



        public void Start(KinectSensor sensor)
        {
            _kinectSensor = sensor;
            _colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.Image.Source = _colorBitmap;
        }

        /**
         * EventHandler zum Zeichnen des Bildes
         */
        public void SensorColorFrameReady(Skeleton skeleton, byte[] colorImagePoints)
        {
            // Write the pixel data into our bitmap

            _colorBitmap.WritePixels(
                new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
                colorImagePoints,
                _colorBitmap.PixelWidth * sizeof(int),
                0);
            _activeSkeleton = skeleton;

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
                    CroppedBitmap colorBitmap = CropBitmap(_colorBitmap);
                    if (colorBitmap == null)
                    {
                        path = SaveScreenshot(_colorBitmap);
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
            _client = new FCClient("5f228f0a0ce14e86a7c901f62ca5a569", "1231e9f2e95d4d90adf436e3de20f0f6");
            _result = _client.Account.EndAuthenticate(_client.Account.BeginAuthenticate(null, null));
        }

        /**
         * Schneidet den Kopf aus der colorBitmap wenn eine Person erkannt wird
         */
        private CroppedBitmap CropBitmap(WriteableBitmap colorBitmap) 
        {
            int width = 120;
            int height = 120;

            // Ersten Player selektieren
            if (_activeSkeleton != null)
            {
                // Punkte des Kopfes auf ColorPoints mappen
                var point = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(_activeSkeleton.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    
                // Überprüfung das nicht außerhalb des Bildes ausgenschnitten wird
                if ((int)point.X - 70 <= 0 || (int)point.Y - 70 >= 470)
                    return null;
                // Array für auszuschneidendes Bild initialisierne
                Int32Rect cropRect =
                    new Int32Rect((int)point.X - 70, (int)point.Y - 70, width, height + 20);

                // Neues Bild erstellen und zurückliefern
                return new CroppedBitmap(_colorBitmap, cropRect);
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
            _result = _client.Faces.EndDetect(_client.Faces.BeginDetect(null, new Stream[] { stream }, Detector.Normal, Attributes.Gender, null, null));
            stream.Close();
        }

        /**
         * Setzt die Attribute und die WPF Elemente nach einem Request
         */
        private void SetAttributes()
        {
            if (_result.Photos[0].Tags.Count == 0)
            {
                this.GenderText.Text = "No face tracked";
            }
            else
            {
                Gender = _result.Photos[0].Tags[0].Attributes.Gender.Value + "";
                Confidence = _result.Photos[0].Tags[0].Attributes.Gender.Confidence + "";

                this.GenderText.Text = Gender + "\t" + Confidence;
            }
        }
    }
}
