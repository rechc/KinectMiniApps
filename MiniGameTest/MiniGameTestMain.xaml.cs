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
    /// Interaktionslogik für MiniGameTestMain.xaml
    /// </summary>
    public partial class MiniGameTestMain : Window
    {
        public MiniGameTestMain()
        {
            InitializeComponent();
            miniGame.Start(KinectHelper.Instance.Sensor);
            KinectHelper.Instance.ReadyEvent += Instance_ReadyEvent;
        }

        void Instance_ReadyEvent(object sender, EventArgs e)
        {
            miniGame.MinigameSkeletonEvent(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.DepthImagePixels, KinectHelper.Instance.ColorPixels);
            KinectHelper.Instance.SetTransform(miniGame);
        }
   }
}
