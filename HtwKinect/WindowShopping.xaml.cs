using LoopList;
using Microsoft.Kinect;
using System.Windows;
using PeopleDetector;
using System.IO;
using HtwKinect.StateViews;
using System.Windows.Controls;
using System;
using System.Windows.Input;

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für FrameWindow.xaml
    /// </summary>
    public partial class FrameWindow : Window
    {
        private PeoplePositionDetector _peopleDetector;
        private int _oldstate = 1;

        private MainWindow _mainWindow = null;
        private HtwKinect.StateViews.SplashScreen _sscreen = null;
        private WalkScreen _walkScreen = null;
        private WalkAndLookScreen _walkLookScreen = null;

        public FrameWindow()
        {
            InitializeComponent();
            StartFirstScreen();           
        }

        private void ChangeScreen()
        {
            if (_peopleDetector.GetPositionOnlyPeople().Count == 0 && _peopleDetector.GetTrackedPeople().Count == 0 && _oldstate != 1) //Zustand 1
            {
                StartFirstScreen();
            }
            else  // Zustand 2-4
            {
                if (_peopleDetector.GetWalkingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count == 0 && _oldstate != 2) // Zustand 2
                {
                    RemoveOldScreen();
                    _oldstate = 2;
                    if (_walkScreen == null) { _walkScreen = new WalkScreen(); }
                    Grid.SetRow(_walkScreen, 1);
                    GridX.Children.Add(_walkScreen);
                }
                else if (_peopleDetector.GetWalkingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count != 0 && _oldstate != 3) // Zustand 3
                {
                    RemoveOldScreen();
                    _oldstate = 3;
                    if (_walkLookScreen == null) { _walkLookScreen = new WalkAndLookScreen(); }
                    Grid.SetRow(_walkLookScreen, 1);
                    GridX.Children.Add(_walkLookScreen);
                }
                else if (_peopleDetector.GetStayingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count != 0 && _oldstate != 4) // Zustand 4
                {
                    RemoveOldScreen();
                    _oldstate = 4;
                    if (_mainWindow == null) { _mainWindow = new MainWindow(); }
                    Grid.SetRow(_mainWindow, 1);
                    GridX.Children.Add(_mainWindow);
                }
            }
        }

        private void StartFirstScreen() 
        {
            RemoveOldScreen();
            _oldstate = 1;
            if (_sscreen == null) { _sscreen = new HtwKinect.StateViews.SplashScreen(); }
            Grid.SetRow(_sscreen, 1);
            GridX.Children.Add(_sscreen);
        }
        
        private void RemoveOldScreen()
        {
            if (GridX.Children.Count > 1)
            {
                GridX.Children.RemoveAt(1);
            }
        }


        /*Tastensteuerung */
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                if (e.SystemKey == Key.F4)
                {
                    Application.Current.Shutdown();
                    e.Handled = true;
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.Escape:
                            Application.Current.Shutdown();
                            e.Handled = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }

        #region PeopleDetector and Window start exit

        /**
         * Wird beim Windowstart aufgerufen
         */
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _peopleDetector = new PeoplePositionDetector();
            KinectHelper kh = KinectHelper.Instance;
            kh.ReadyEvent+= this.PeopleDetectorSkeletonEvent;
        }

        /**
         * Wird beim beenden aufgerufen
         */
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Joke: Fenster Klose");
        }

        /**
         * Event Skeleton für PeopleDetector
         */ 
        private void PeopleDetectorSkeletonEvent(object sender, EventArgs e)
        {
            _peopleDetector.Skeletons = KinectHelper.Instance.Skeletons;
            OutputLabelX.Content =
            "Erkannt:" + _peopleDetector.GetPositionOnlyPeople().Count +
                " Tracked:" + _peopleDetector.GetTrackedPeople().Count +
                " Walking:" + _peopleDetector.GetWalkingPeople().Count +
                " Standing:" + _peopleDetector.GetStayingPeople().Count
                + " Looking:" + _peopleDetector.GetLookingPeople().Count;
            ChangeScreen();
        }
        #endregion PeopleDetector and Window start exit
    }
}
