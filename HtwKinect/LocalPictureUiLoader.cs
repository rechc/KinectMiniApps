using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LoopList;

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

        private FrameworkElement BuildGrid(string path)
        {
            Grid grid = new Grid();
            Image img = new Image
            {
                Stretch = Stretch.Fill,
                Source = LoadImage(path)
            };
            grid.Children.Add(img);
            return grid;
        }

        public void LoadElementsIntoList(KinectProjectUiBuilder kinectProjectUiBuilder)
        {
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Top");
            List<FrameworkElement> list = new List<FrameworkElement> ();
            for (int i = 0; i < paths.Count(); i++) {
                list.Add(BuildGrid(paths[i]));
            }
            kinectProjectUiBuilder.AddRow("Top", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Beach");
            for (int i = 0; i < paths.Count(); i++)
            {
                list.Add(BuildGrid(paths[i]));
            }
            kinectProjectUiBuilder.AddRow("Beach", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Snow");
            for (int i = 0; i < paths.Count(); i++)
            {
                if (i == paths.Count() -1)
                {
                    var img = new Image(){Source = LoadImage(paths[i])};
                    var grid = new Grid();
                    grid.Children.Add(img);

                    var gsc = new GreenScreenControl.GreenScreenControl();
                    grid.Children.Add(gsc);
                    gsc.Width = 640;
                    gsc.Height = 480;
                    var instance = KinectHelper.GetInstance();
                    gsc.Start(instance.GetSensor(), instance.DepthImageFormat, instance.ColorImageFormat);
                    instance.AllFramesDispatchedEvent += (sender, args) => RenderGreenScreen(gsc);
                    list.Add(grid);
                }
                else
                {
                    list.Add(BuildGrid(paths[i]));
                }
            }
           
            kinectProjectUiBuilder.AddRow("Snow", list);

        }

        private void RenderGreenScreen(GreenScreenControl.GreenScreenControl greenScreenControl)
        {
            var instance = KinectHelper.GetInstance();
           greenScreenControl.InvalidateVisual(instance.GetDepthImagePixels(), instance.GetColorPixels()); 
        }

        
    }
}
