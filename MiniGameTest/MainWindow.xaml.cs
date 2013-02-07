using HtwKinect;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MiniGameTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<GridObjects> gridObjects= new List<GridObjects>();

        private FallWorker fallWorker;

        private Viewbox playerBox;

        private String path;

        private const String winter = "Bilder/Winter";
        private const String summer = "Bilder/Summer";

        private bool twoObjectsInOneColumn = false;

        private Thread fallThread;

        private KinectHelper kh;

        private Skeleton playerSkeleton = null;

        private bool play = false;

        /**
         * Konstruktor
         */
        public MainWindow()
        {
            InitializeComponent();
            // Keyboard Handler
            //this.KeyDown += new KeyEventHandler(KeyDownHandler);
            // Kinect Sensor initalisieren
            InitializeSensor();
        }

        private void InitializeSensor()
        {
            kh = KinectHelper.Instance;
            kh.ReadyEvent += this.MinigameSkeletonEvent;
        }

        private void MinigameSkeletonEvent(object sender, EventArgs e)
        {
            playerSkeleton = kh.GetFixedSkeleton();

            if (play)
            {
                PlayerHandler();
            }
            if (!play && playerSkeleton != null)
            {
                GameStart(1);
                play = true;
            }
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
            fallWorker.eventFallen += new MiniGameTest.FallWorker.FallWorkerEventHandler(FallHandler);
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

            Thread.Sleep(200);
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
            Image i = new Image();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path + "/" + new Random().Next(1, 3) + ".png", UriKind.Relative);
            bi.EndInit();
            i.Source = bi;
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
            var gsc = new GreenScreenControl.GreenScreenControl(); 
            gsc.RenderTransform = kh.CreateTransform();
            gsc.Width = kh.Sensor.ColorStream.FrameWidth;
            gsc.Height = kh.Sensor.ColorStream.FrameHeight;
            gsc.Start(kh.Sensor, false);

            playerBox = new Viewbox();
            playerBox.Child = gsc;
            playerBox.Stretch = Stretch.Fill;

            kh.ReadyEvent += (sender, args) => RenderGreenScreen(gsc);
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

        private void RenderGreenScreen(GreenScreenControl.GreenScreenControl greenScreenControl)
        {
            greenScreenControl.InvalidateVisual(kh.DepthImagePixels, kh.ColorPixels);
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
            String p = System.IO.Directory.GetCurrentDirectory();
            p = p.Replace("\\bin\\Debug", "");

            ImageBrush img = new ImageBrush();
            img.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(p + "\\" + path + "\\" + "background.jpg");
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
