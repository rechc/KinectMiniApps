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

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class FrameWindow : Window
    {
        int oldstate = 1;

        MainWindow mw = null;
        TextLoopList tll = null;


        public FrameWindow()
        {
            InitializeComponent();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen();
        }

        private void ChangeScreen()
        {
            if (peopleDetector.GetPositionOnlyPeople().Count == 0 && peopleDetector.GetTrackedPeople().Count == 0 && oldstate != 1) //Zustand 1
            {
                RemoveOldScreen();
                oldstate = 1;
                tll = new TextLoopList();
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
                    mw = new MainWindow();
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

        #region PeopleDetector

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        private PeoplePositionDetector peopleDetector = new PeoplePositionDetector();



        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();
                //this.sensor.SkeletonStream.AppChoosesSkeletons = true;
                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                }
            }

            peopleDetector.Skeletons = skeletons;

            OutputLabelX.Content =
            "Erkannt:" + peopleDetector.GetPositionOnlyPeople().Count +
                " Tracked:" + peopleDetector.GetTrackedPeople().Count +
                " Walking:" + peopleDetector.GetWalkingPeople().Count +
                " Standing:" + peopleDetector.GetStayingPeople().Count
                + " Looking:" + peopleDetector.GetLookingPeople().Count;
            /* this.LblErkanntOutput.Content = peopleDetector.GetPositionOnlyPeople().Count;
             this.LblTrackedOutput.Content = peopleDetector.GetTrackedPeople().Count;
             this.LblLaufenOutput.Content = peopleDetector.GetWalkingPeople().Count;
             this.LblStehenOutput.Content = peopleDetector.GetStayingPeople().Count;
             this.LblSchauenOutput.Content = peopleDetector.GetLookingPeople().Count;*/
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                /* if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                 {
                     this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                 }
                 else
                 {*/
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                /* }*/
            }
        }

        #endregion PeopleDetector

    }
}
