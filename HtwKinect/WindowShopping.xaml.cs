using System.Windows;
using PeopleDetector;
using HtwKinect.StateViews;
using System.Windows.Controls;
using System;
using System.Windows.Input;
using Database;
using Database.DAO;

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für FrameWindow.xaml
    /// </summary>
    public partial class FrameWindow : Window
    {
        private bool _debugOnlyScreen4;
        private bool _debugOnlyScreen2;

        private PeoplePositionDetector _peopleDetector;
        private ScreenMode _currentScreen = ScreenMode.Splash;

        public enum ScreenMode
        {
            Splash,
            Walk,
            WalkandLook,
            MainScreen,
            Unknown
        }

        private LoopScreen _mainWindow;
        private StateViews.SplashScreen _sscreen;
        private WalkScreen _walkScreen;
        private WalkAndLookScreen _walkLookScreen;

        public FrameWindow()
        {
            InitializeComponent();
            StartSplashScreen();           
        }

        private void ChangeScreen()
        {
            if (_debugOnlyScreen4) {
                if (_currentScreen != ScreenMode.MainScreen) {
                    StartMainScreen();
                }
                return;
            }

            if (_debugOnlyScreen2)
            {
                if (_currentScreen != ScreenMode.Walk)
                {
                    StartWalkScreen();
                }
                return;
            }


            if (_peopleDetector.GetPositionOnlyPeople().Count == 0 && _peopleDetector.GetTrackedPeople().Count == 0 && _currentScreen != ScreenMode.Splash) //Zustand 1
            {
                StartSplashScreen();
            }
            else  // Zustand 2-4
            {
                if (_peopleDetector.GetWalkingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count == 0 && _currentScreen != ScreenMode.Walk) // Zustand 2
                {
                    StartWalkScreen();
                }
                else if (_peopleDetector.GetWalkingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count != 0 && _currentScreen != ScreenMode.WalkandLook) // Zustand 3
                {
                    StartWalkandLookScreen();
                }
                else if (_peopleDetector.GetStayingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count != 0 && _currentScreen != ScreenMode.MainScreen) // Zustand 4
                {
                    StartMainScreen();
                }
            }
        }

        private void StartSplashScreen() 
        {
            RemoveOldScreen();
            _currentScreen = ScreenMode.Splash;
            if (_sscreen == null) 
            { 
                _sscreen = new StateViews.SplashScreen();       
            }
            _sscreen.StartDisplay(StopLastScreenAndGetLastTravel());
            _sscreen.StartNewOfferTimer(60000/6); //todo set better time intervall, now its 1/6 minutes
            Grid.SetRow(_sscreen, 1);
            GridX.Children.Add(_sscreen);
        }

        private void StartWalkScreen()
        {
            RemoveOldScreen();
            _currentScreen = ScreenMode.Walk;
            if (_walkScreen == null) { _walkScreen = new WalkScreen(); }
            _walkScreen.StartDisplay(StopLastScreenAndGetLastTravel());
            Grid.SetRow(_walkScreen, 1);
            GridX.Children.Add(_walkScreen);
        }

        private void StartWalkandLookScreen()
        {
            RemoveOldScreen();
            _currentScreen = ScreenMode.WalkandLook;
            if (_walkLookScreen == null) { _walkLookScreen = new WalkAndLookScreen(); }
            _walkLookScreen.StartDisplay(StopLastScreenAndGetLastTravel());
            Grid.SetRow(_walkLookScreen, 1);
            GridX.Children.Add(_walkLookScreen);
        }

        private void StartMainScreen()
        {
            RemoveOldScreen();
            _currentScreen = ScreenMode.MainScreen;
            if (_mainWindow == null) { _mainWindow = new LoopScreen(); }
            _mainWindow.StartDisplay(StopLastScreenAndGetLastTravel());
            Grid.SetRow(_mainWindow, 1);
            GridX.Children.Add(_mainWindow);
        }

        /**
         * returns last Offer, if nothing available Random Top-TravelOffer 
         */
        private TravelOffer StopLastScreenAndGetLastTravel()
        {
            TravelOffer lastOffer = null;
            switch (_currentScreen)
            {
                case ScreenMode.Splash:
                    lastOffer = _sscreen.StopDisplay();
                    break;
                case ScreenMode.Walk:
                    lastOffer = _walkScreen.StopDisplay();
                    break;
                case ScreenMode.WalkandLook:
                    lastOffer = _walkLookScreen.StopDisplay();
                    break;
                case ScreenMode.MainScreen:
                    lastOffer = _mainWindow.StopDisplay();
                    break;
                default:
                    break;
            }

            if (lastOffer != null)
            { 
                return lastOffer; 
            }
            else
            {
                return new TravelOfferDao().SelectRandomTopOffer();
            }
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
                        case Key.Space:
                            _debugOnlyScreen4 = !_debugOnlyScreen4;
                            _debugOnlyScreen2 = false;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                        case Key.NumPad4:
                            _debugOnlyScreen4 = !_debugOnlyScreen4;
                            _debugOnlyScreen2 = false;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                        case Key.NumPad2:
                            _debugOnlyScreen4 = false;
                            _debugOnlyScreen2 = !_debugOnlyScreen2;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            if (e.Handled == false && _currentScreen==ScreenMode.MainScreen && _mainWindow!=null) 
            {
              _mainWindow.DelegateKeyEvent(e);
            }

        }

        #region PeopleDetector and Window start exit

        /// <summary>
        /// Wird nach dem Laden des Fensters aufgerufen.
        /// </summary>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _peopleDetector = new PeoplePositionDetector();
            try
            {
                KinectHelper kh = KinectHelper.Instance;
                kh.ReadyEvent += PeopleDetectorSkeletonEvent;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        /// <summary>
        /// Wird beim Schliessen des Fensters aufgerufen.
        /// </summary>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Joke: Fenster Klose");
        }

        /// <summary>
        /// Event Skeleton für PeopleDetector.
        /// </summary>
        private void PeopleDetectorSkeletonEvent(object sender, EventArgs e)
        {
            _peopleDetector.TrackSkeletons(KinectHelper.Instance.Skeletons);
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
