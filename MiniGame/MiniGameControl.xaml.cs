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
    /// Interaktionslogik für MiniGameControl.xaml
    /// </summary>
    public partial class MiniGameControl
    {

        private List<GridObjects> _gridObjects= new List<GridObjects>();
        private FallWorker _fallWorker;
        private Viewbox _playerBox;
        private String _imagePath;
        private Thread _fallThread;
        private KinectSensor _sensor;
        private Skeleton _playerSkeleton = null;

        private bool _twoObjectsInOneColumn = false;
        private bool _play = false;
       
        private GreenScreenControl.GreenScreenControl _gsc;
        private DepthImagePixel[] _depthImagePixels;
        private byte[] _colorPixels;


        private const String Winter = "images/Bilder/Winter/Winter";
        private const String Summer = "images/Bilder/Summer/Summer";
        private const String Street = "images/Bilder/Street/Street";
        private const String Mountain = "images/Bilder/Mountain/Mountain";

        /**
         * Variablen zum ausfuehren von MinigameTest 
         */
        //private const String Winter = "Bilder/Winter/Winter";
        //private const String Summer = "Bilder/Summer/Summer";
        //private const String Street = "images/Bilder/Street/Street";
        //private const String Mountain = "images/Bilder/Mountain/Mountain";

        /**
         * Konstruktor
         */
        public MiniGameControl()
        {
            InitializeComponent();
        }

        public void Start(KinectSensor sensor)
        {
            _sensor = sensor;
        }

        /**
         * Event mit Uebergabeparametern von KinectHelper
         */
        public void MinigameSkeletonEvent(Skeleton activeSkeleton, DepthImagePixel[] depthImagePixels, byte[] colorPixels)
        {
            _playerSkeleton = activeSkeleton;
            if (_playerSkeleton == null)
            {
                GameStop();
            }
            else
            {

                _depthImagePixels = depthImagePixels;
                _colorPixels = colorPixels;
                if (_play && _playerSkeleton != null)
                {
                    PlayerHandler();
                }
                if (!_play && _playerSkeleton != null)
                {
                    GameStart(new Random().Next(0, 4));
                    _play = true;
                }
                if (_gsc != null)
                    RenderGreenScreen();
            }
        }

        /**
         * Player-Handler zum Bewegen des Players
         */
        private void PlayerHandler()
        {
            if (_playerSkeleton.Joints[JointType.ShoulderCenter].Position.X < -0.25)
            {
                Grid.SetColumn(_playerBox, 1);
            }
            else if (_playerSkeleton.Joints[JointType.ShoulderCenter].Position.X > 0.25)
            {
                Grid.SetColumn(_playerBox, 3);
            }
            else
            {
                Grid.SetColumn(_playerBox, 2);
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
                    _imagePath = Winter;
                    break;
                case 1:
                    _imagePath = Summer;
                    break;
                case 2:
                    _imagePath = Mountain;
                    break;
                case 3:
                    _imagePath = Street;
                    break;
            }
            _twoObjectsInOneColumn = false;
            SetBackgroundImage();
            AddPlayer();
            _fallWorker = new FallWorker(false);
            _fallWorker.eventFallen += new MiniGame.FallWorker.FallWorkerEventHandler(FallHandler);
            _fallThread = new Thread(_fallWorker.InvokeFalling);
            _fallThread.Start();
        }

        /**
         * Stopt das Spiel und entfernt die Objekte
         */
        private void GameStop()
        {
            this.Status.Visibility = Visibility.Visible;
            if (_fallThread != null)
            {
                _fallThread.Abort();
            }
        }

        /**
         * Handler der die Objekte nach unten bewegt, neue Objekte erzeugt und auf Spielende prueft
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
                if (_gridObjects.Count() > 0 && !_fallWorker.GameOver)
                {
                    ShiftObjects(true);
                }
                // GameOver überprüfen
                CheckGameOver();
                // Neue Objekte generieren
                if (!_fallWorker.GameOver)
                {
                    //Nicht mehr als 5 Objekte auf dem Screen
                    if (_gridObjects.Count() < 5)
                    {
                        AddObjectsToGrid();
                    }
                }
                
            }));

            //FallWorker sleep
            Thread.Sleep(300);
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!_fallWorker.GameOver)
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
            foreach (GridObjects go in _gridObjects)
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
            _gridObjects = tempGridObjects;
        }

        /**
         * Ueberprueft ob der Player und ein Objekt an gleicher Stelle liegt
         */
        private void CheckGameOver()
        {
            foreach (GridObjects go in _gridObjects)
            {
                if (go.row == 4 && go.column == Grid.GetColumn(this._playerBox))
                {
                    _fallWorker.GameOver=true;
                    break;
                }
            }
        }

        /**
         * Fuegt ein Objekt dem Grid hinzu
         */
        private void AddObjectsToGrid()
        {
            int x = CreateNewObject(new Random().Next(1, 4));
            //Falls keine 2 Objekte in der vorherigen Spalten waren und Random 0 ist
            if (_twoObjectsInOneColumn == false && 0== new Random().Next(0,2))
            {
                
                int y = new Random().Next(1, 4);
                while (x == y)
                {
                    y = new Random().Next(1, 4);
                }
                x = CreateNewObject(y);
                _twoObjectsInOneColumn = true;

            }
            else
            {
                _twoObjectsInOneColumn = false;
            }
        }

        /**
         * Erstellt ein neues Objekt
         */

        private int CreateNewObject( int column)
        {
            Grid i = new Grid();
            BitmapImage bi = new BitmapImage(new Uri(_imagePath + "Object" + new Random().Next(1, 7) + ".png", UriKind.RelativeOrAbsolute));
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.Uniform;
            ib.ImageSource = bi;
            i.Background = ib;
          
            
            MiniGameGrid.Children.Add(i);

            Grid.SetColumn(i, column);
            Grid.SetRow(i, 1);
            _gridObjects.Add(new GridObjects(i, column, 1));
            
            return column;

        }

        /**
         * Loescht alle Objekte vom Grid, außer den Player
         */
        private void RemoveAllObjects()
        {
            foreach (GridObjects go in _gridObjects)
            {
                MiniGameGrid.Children.Remove(go.image);
            }
            _gridObjects.Clear();
        }

        /**
         * Fuegt den Player dem Grid hinzu
         */
        private void AddPlayer()
        {
            _gsc = new GreenScreenControl.GreenScreenControl(); 
            _gsc.Width = _sensor.ColorStream.FrameWidth;
            _gsc.Height = _sensor.ColorStream.FrameHeight;
            _gsc.Start(_sensor, false);

            _playerBox = new Viewbox();
            _playerBox.Child = _gsc;
            _playerBox.Stretch = Stretch.Fill;
            _playerBox.SetValue(Panel.ZIndexProperty, 1);
            
            MiniGameGrid.Children.Add(_playerBox);

            Debug.WriteLine(_playerSkeleton.Joints[JointType.ShoulderCenter].Position.X);
            if (_playerSkeleton.Joints[JointType.ShoulderCenter].Position.X < -0.25)
            {
                Grid.SetColumn(_playerBox, 1);
            }
            else if (_playerSkeleton.Joints[JointType.ShoulderCenter].Position.X > 0.25)
            {
                Grid.SetColumn(_playerBox, 3);
            }
            else
            {
                Grid.SetColumn(_playerBox, 2);
            }

           
            Grid.SetRow(_playerBox, 4);

        }

        private void RenderGreenScreen()
        {
            _gsc.RenderImageData(_depthImagePixels, _colorPixels);
        }
        
        /**
         * Loescht den Player vom Grid
         */
        private void RemovePlayer()
        {
            MiniGameGrid.Children.Remove(_playerBox);
        }

        /**
         * Setzt das Backgroundbild abhaengig vom Modus
         */
        private void SetBackgroundImage()
        {

            ImageBrush img = new ImageBrush();
            img.ImageSource = new BitmapImage(new Uri(_imagePath + "Background" + new Random().Next(1, 3) + ".jpg", UriKind.RelativeOrAbsolute));
            MiniGameGrid.Background = img;
        }

        /**
         * Click Handler
         */
        private void Status_Click_1(object sender, RoutedEventArgs e)
        {
            _play = false;
            this.Status.Visibility = Visibility.Hidden;
        }  
    }
}
