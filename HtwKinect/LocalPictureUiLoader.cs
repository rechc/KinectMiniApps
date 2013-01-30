using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace HtwKinect
{
    /*Diese Klasse lädt lokale Testbilder in die LoopList*/
    class LocalPictureUiLoader : IUiLoader
    {

        private static BitmapImage LoadImage(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }

        public void LoadElementsIntoList(KinectProjectUiBuilder kinectProjectUiBuilder)
        {
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Top");
            List<FrameworkElement> list = new List<FrameworkElement> ();
            for (int i = 0; i < paths.Count(); i++) {
                list.Add(BuildGreenScreen(paths[i]));
            }
            kinectProjectUiBuilder.AddRow("Top", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Beach");
            for (int i = 0; i < paths.Count(); i++)
            {
                list.Add(BuildGreenScreen(paths[i]));
            }
            kinectProjectUiBuilder.AddRow("Beach", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Snow");

            for (int i = 0; i < paths.Count(); i++)
            {
                list.Add(BuildGreenScreen(paths[i]));
            }
           
            kinectProjectUiBuilder.AddRow("Snow", list);

        }

        private Grid BuildGreenScreen(string s)
        {
            var img = new Image {Source = LoadImage(s), Stretch = Stretch.Fill};
            var grid = new Grid();
            grid.Children.Add(img);
            try
            {
                var gsc = new GreenScreenControl.GreenScreenControl();
                grid.Children.Add(gsc);
                var instance = KinectHelper.Instance;
                gsc.Start(instance.Sensor, true);
                instance.ReadyEvent += (sender, args) => RenderGreenScreen(gsc);
            }
            catch
            {
                //Dieser Try Catch ist dazu da, damit die Bilder geladen werden können, auch wenn kein Kinectsensor angeschloßen ist.
            }
            return grid;
        }

        private void RenderGreenScreen(GreenScreenControl.GreenScreenControl greenScreenControl)
        {
            if (((Grid)greenScreenControl.Parent).Parent == null)
            {
                return; //nur auf dingen die auch angezeigt werden bitte, danke.
            }
            var instance = KinectHelper.Instance;
            greenScreenControl.InvalidateVisual(instance.DepthImagePixels, instance.ColorPixels); 
        }

        
    }
}
