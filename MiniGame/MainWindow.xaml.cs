using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MiniGame
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

   	private List<GridObjects> gridObjects= new List<GridObjects>();

        private FallWorker fallWorker;

        private Viewbox playerBox;
        private Viewbox objectBox;

        private String path;

        private const String winter = "images/Bilder/Winter";
        private const String summer = "images/Bilder/Summer";

        private bool twoObjectsInOneColumn = false;

        private Thread fallThread;

        private KinectSensor _sensor;

        private Skeleton playerSkeleton = null;

        private bool play = false;

        private GreenScreenControl.GreenScreenControl _gsc;
        private DepthImagePixel[] _depthImagePixels;
        private byte[] _colorPixels;

        /**
         * Konstruktor
         */
        public MainWindow()
        {
            InitializeComponent();
	        // Keyboard Handler
            //this.KeyDown += new KeyEventHandler(KeyDownHandler);
            // Kinect Sensor initalisieren
        }

	    public void Start(KinectSensor sensor)
        {
            _sensor = sensor;
        }

        public void MinigameSkeletonEvent(Skeleton activeSkeleton, DepthImagePixel[] depthImagePixels, byte[] colorPixels)
        {
            playerSkeleton = activeSkeleton;
            _depthImagePixels = depthImagePixels;
            _colorPixels = colorPixels;
            if (play && playerSkeleton != null)
            {
                PlayerHandler();
            }
            if (!play && playerSkeleton != null)
            {
                GameStart(1);
                play = true;
            }
            if (_gsc != null)
                RenderGreenScreen();
        }

        /**
         * Key-Handler zum Bewegen des Players
         */
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (Grid.GetColumn(this.playerBox) != 3)
                {
                    Grid.SetColumn(this.playerBox, Grid.GetColumn(this.playerBox) + 1);
                }
            }
            else if (e.Key == Key.Left)
            {
                if (Grid.GetColumn(this.playerBox) != 1)
                {
                    Grid.SetColumn(this.playerBox, Grid.GetColumn(this.playerBox) - 1);
                }
            }
        }

        /**
         * Player-Handler zum Bewegen des Players
         */
        private void PlayerHandler()
        {
            if (playerSkeleton.Joints[JointType.ShoulderCenter].Position.X < -0.25)
            {
                Grid.SetColumn(this.playerBox, 1);
            }
            else if (playerSkeleton.Joints[JointType.ShoulderCenter].Position.X > 0.25)
            {
                Grid.SetColumn(this.playerBox, 3);
            }
            else
            {
                Grid.SetColumn(this.playerBox, 2);
            }
        }
        
        /**
         * Startet das Spiel
         */
        private void GameStart(int mode)
        {            
            RemoveAllObjects();
            RemovePlayer();
            switch (mode)
            {
                case 0:
                    path = winter;
                    break;
                case 1:
                    path = summer;
                    break;
            }
            twoObjectsInOneColumn = false;
            SetBackgroundImage();
            AddPlayer();
            fallWorker = new FallWorker(false);
            fallWorker.eventFallen += new MiniGame.FallWorker.FallWorkerEventHandler(FallHandler);
            fallThread = new Thread(fallWorker.InvokeFalling);
            fallThread.Start();
            //new Thread(fallWorker.InvokeFalling).Start();
        }

        /**
         * Stopt das Spiel und entfernt die Objekte
         */
        private void GameStop()
        {
            this.Status.Visibility = Visibility.Visible;
            fallThread.Abort();
        }

        /**
         * Handler der die Objekte nach unten bewegt, neue Objekte erzeugt und auf Spielende prüft
         */
        public void FallHandler(object sender, EventArgs ea)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                // Position checken
                PlayerHandler();
                // GameOver überprüfen
                CheckGameOver();
                // Objekte nach unten setzen
                if (gridObjects.Count() > 0 && !fallWorker.GameOver)
                {
                    ShiftObjects(true);
                }
                // GameOver überprüfen
                CheckGameOver();
                // Neue Objekte generieren
                if (!fallWorker.GameOver)
                {
                    if (gridObjects.Count() < 5)
                    {
                        AddObjects();
                    }
                }
                
            }));

            Thread.Sleep(300);
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!fallWorker.GameOver)
                {                     
                    ShiftObjects(false);
                }
                else
                {
                GameStop();
                }
            }));

        }

        /**
         * Schiebt die Objekte um eins nach unten im Grid
         */
        private void ShiftObjects(bool withShift)
        {
            List<GridObjects> tempGridObjects = new List<GridObjects>();
            foreach (GridObjects go in gridObjects)
            {
                if (go.row == 4)
                {
                    MiniGameGrid.Children.Remove(go.image);
                }
                else
                {
                    if (withShift)
                    {
                        go.row++;
                        Grid.SetRow(go.image, go.row);
                    }
                    tempGridObjects.Add(go);
                }
            }
            gridObjects = tempGridObjects;
        }

        /**
         * Überprüft ob der Player und ein Objekt an gleicher Stelle liegt
         */
        private void CheckGameOver()
        {
            foreach (GridObjects go in gridObjects)
            {
                if (go.row == 4 && go.column == Grid.GetColumn(this.playerBox))
                {
                    fallWorker.GameOver=true;
                    break;
                }
            }
        }

        /**
         * Fügt ein Objekt dem Grid hinzu
         */
        private void AddObjects()
        {
            int x = AddObject(new Random().Next(1, 4));
            if (twoObjectsInOneColumn == false && 0== new Random().Next(0,2))
            {
                
                int y = new Random().Next(1, 4);
                while (x == y)
                {
                    y = new Random().Next(1, 4);
                }
                x = AddObject(y);
                twoObjectsInOneColumn = true;

            }
            else
            {
                twoObjectsInOneColumn = false;
            }
        }

        private int AddObject( int column)
        {
            Grid i = new Grid();
            BitmapImage bi = new BitmapImage(new Uri(path + "/" + new Random().Next(1, 3) + ".png", UriKind.RelativeOrAbsolute));
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.Uniform;
            ib.ImageSource = bi;
            i.Background = ib;
          
            
            MiniGameGrid.Children.Add(i);

            Grid.SetColumn(i, column);
            Grid.SetRow(i, 1);
            gridObjects.Add(new GridObjects(i, column, 1));
            
            return column;

        }

        /**
         * Löscht alle Objekte vom Grid, außer den Player
         */
        private void RemoveAllObjects()
        {
            foreach (GridObjects go in gridObjects)
            {
                MiniGameGrid.Children.Remove(go.image);
            }
            gridObjects.Clear();
        }

        /**
         * Fügt den Player dem Grid hinzu
         */
        private void AddPlayer()
        {
            _gsc = new GreenScreenControl.GreenScreenControl(); 
            //gsc.RenderTransform = kh.CreateTransform();
            _gsc.Width = _sensor.ColorStream.FrameWidth;
            _gsc.Height = _sensor.ColorStream.FrameHeight;
            _gsc.Start(_sensor, false);

            playerBox = new Viewbox();
            playerBox.Child = _gsc;
            playerBox.Stretch = Stretch.Fill;

            this.playerBox.SetValue(Panel.ZIndexProperty, 1);
            MiniGameGrid.Children.Add(playerBox);

            Debug.WriteLine(playerSkeleton.Joints[JointType.ShoulderCenter].Position.X);
            if (playerSkeleton.Joints[JointType.ShoulderCenter].Position.X < -0.25)
            {
                Grid.SetColumn(playerBox, 1);
            }
            else if (playerSkeleton.Joints[JointType.ShoulderCenter].Position.X > 0.25)
            {
                Grid.SetColumn(playerBox, 3);
            }
            else
            {
                Grid.SetColumn(playerBox, 2);
            }

           
            Grid.SetRow(playerBox, 4);

        }

        private void RenderGreenScreen()
        {
            _gsc.InvalidateVisual(_depthImagePixels, _colorPixels);
        }
        /**
         * Löscht den Player vom Grid
         */
        private void RemovePlayer()
        {
            MiniGameGrid.Children.Remove(this.playerBox);
        }

        /**
         * Setzt das Backgroundbild abhängig vom Modus
         */
        private void SetBackgroundImage()
        {

            ImageBrush img = new ImageBrush();
            //img.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(p + "\\" + path + "\\" + "background.jpg");
            img.ImageSource = new BitmapImage(new Uri(path + "/background.jpg", UriKind.RelativeOrAbsolute));
            MiniGameGrid.Background = img;
        }

        /**
         * Click Handler
         */
        private void Status_Click_1(object sender, RoutedEventArgs e)
        {
            play = false;
            this.Status.Visibility = Visibility.Hidden;
        }  
    }
}
