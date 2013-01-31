using LoopList;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using PeopleDetector;
using System.IO;
using HtwKinect.StateViews;

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class FrameWindow : Window
    {
        private PeoplePositionDetector peopleDetector;
        int oldstate = 1;

        MainWindow mw = null;
        HtwKinect.StateViews.SplashScreen sscreen = null;
        WalkScreen ws = null;
        WalkAndLookScreen wals = null;

        TextLoopList tll = null; // das weg und die anderen einbinden


        public FrameWindow()
        {
            InitializeComponent();            
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
           // ChangeScreen();
        }

        private void ChangeScreen()
        {
            if (peopleDetector.GetPositionOnlyPeople().Count == 0 && peopleDetector.GetTrackedPeople().Count == 0 && oldstate != 1) //Zustand 1
            {
                RemoveOldScreen();
                oldstate = 1;
                if(tll ==null){tll = new TextLoopList();}
                Grid.SetRow(tll, 1);
                GridX.Children.Add(tll);
            }
            else  // Zustand 2-4
            {
                if (peopleDetector.GetWalkingPeople().Count != 0 && peopleDetector.GetLookingPeople().Count == 0 && oldstate != 2) // Zustand 2
                {
                    RemoveOldScreen();
                    oldstate = 2;
                }
                else if (peopleDetector.GetWalkingPeople().Count != 0 && peopleDetector.GetLookingPeople().Count != 0 && oldstate != 3) // Zustand 3
                {
                    RemoveOldScreen();
                    oldstate = 3;
                }
                else if (peopleDetector.GetStayingPeople().Count != 0 && peopleDetector.GetLookingPeople().Count != 0 && oldstate != 4) // Zustand 4
                {
                    RemoveOldScreen();
                    oldstate = 4;
                    if (mw == null) { mw = new MainWindow(); }
                    Grid.SetRow(mw, 1);
                    GridX.Children.Add(mw);
                }
            }
        }

        private void RemoveOldScreen()
        {
            if (GridX.Children.Count > 2)
            {
                GridX.Children.RemoveAt(2);
            }
        }

        #region PeopleDetector and Window start exit

        /**
         * Wird beim Windowstart aufgerufen
         */
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            peopleDetector = new PeoplePositionDetector();
            KinectHelper kh = KinectHelper.Instance;
            kh.ReadyEvent+= this.PeopleDetectorSkeletonEvent;
        }

        /**
         * Wird beim beenden aufgerufen
         */
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {          
        }

        /**
         * Event Skeleton für PeopleDetector
         */ 
        private void PeopleDetectorSkeletonEvent(object sender, EventArgs e)
        {
            peopleDetector.Skeletons = KinectHelper.Instance.Skeletons;
            OutputLabelX.Content =
            "Erkannt:" + peopleDetector.GetPositionOnlyPeople().Count +
                " Tracked:" + peopleDetector.GetTrackedPeople().Count +
                " Walking:" + peopleDetector.GetWalkingPeople().Count +
                " Standing:" + peopleDetector.GetStayingPeople().Count
                + " Looking:" + peopleDetector.GetLookingPeople().Count;
            ChangeScreen();
        }
        #endregion PeopleDetector and Window start exit
    }
}
