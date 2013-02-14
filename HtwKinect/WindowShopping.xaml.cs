using System.Windows;
using PeopleDetector;
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
        private bool _debugOnlyScreen1;

        private PeoplePositionDetector _peopleDetector;
        private ScreenMode _currentScreen = ScreenMode.Splash; // durch enum noch ersetzen
        public enum ScreenMode
        {
            Splash,
            Walk,
            WalkandLook,
            MainScreen,
            Unknown
        }

        private MainWindow _mainWindow;
        private StateViews.SplashScreen _sscreen;
        private WalkScreen _walkScreen;
        private WalkAndLookScreen _walkLookScreen;

        public FrameWindow()
        {
            InitializeComponent();
            StartFirstScreen();           
        }

        private void ChangeScreen()
        {
            if (_debugOnlyScreen1) {
                if (_currentScreen != ScreenMode.MainScreen) {
                    RemoveOldScreen();
                    _currentScreen = ScreenMode.MainScreen;
                    if (_mainWindow == null) { _mainWindow = new MainWindow(); }
                    Grid.SetRow(_mainWindow, 1);
                    GridX.Children.Add(_mainWindow);
                }
                return;
            }


            if (_peopleDetector.GetPositionOnlyPeople().Count == 0 && _peopleDetector.GetTrackedPeople().Count == 0 && _currentScreen != ScreenMode.Splash) //Zustand 1
            {
                StartFirstScreen();
            }
            else  // Zustand 2-4
            {
                if (_peopleDetector.GetWalkingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count == 0 && _currentScreen != ScreenMode.Walk) // Zustand 2
                {
                    RemoveOldScreen();
                    _currentScreen = ScreenMode.Walk;
                    if (_walkScreen == null) { _walkScreen = new WalkScreen(); }
                    Grid.SetRow(_walkScreen, 1);
                    GridX.Children.Add(_walkScreen);
                }
                else if (_peopleDetector.GetWalkingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count != 0 && _currentScreen != ScreenMode.WalkandLook) // Zustand 3
                {
                    RemoveOldScreen();
                    _currentScreen = ScreenMode.WalkandLook;
                    if (_walkLookScreen == null) { _walkLookScreen = new WalkAndLookScreen(); }
                    Grid.SetRow(_walkLookScreen, 1);
                    GridX.Children.Add(_walkLookScreen);
                }
                else if (_peopleDetector.GetStayingPeople().Count != 0 && _peopleDetector.GetLookingPeople().Count != 0 && _currentScreen != ScreenMode.MainScreen) // Zustand 4
                {
                    RemoveOldScreen();
                    _currentScreen = ScreenMode.MainScreen;
                    if (_mainWindow == null) { _mainWindow = new MainWindow(); }
                    Grid.SetRow(_mainWindow, 1);
                    GridX.Children.Add(_mainWindow);
                }
            }
        }

        private void StartFirstScreen() 
        {
            RemoveOldScreen();
            _currentScreen = ScreenMode.Splash;
            if (_sscreen == null) 
            { 
                _sscreen = new StateViews.SplashScreen();       
            }
            _sscreen.StartNewOfferTimer(180000/50); //todo set better time intervall, now its 3/50 minutes
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
                        case Key.Space:
                            _debugOnlyScreen1 = true;
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

        /**
         * Wird beim Windowstart aufgerufen
         */
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _peopleDetector = new PeoplePositionDetector();
            KinectHelper kh = KinectHelper.Instance;
            kh.ReadyEvent += PeopleDetectorSkeletonEvent;
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
