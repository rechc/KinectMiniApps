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
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images", "tele*");

            List<FrameworkElement> list = new List<FrameworkElement>
                    {
                        BuildGrid(paths[0]),
                        BuildGrid(paths[1]),
                    };
            kinectProjectUiBuilder.AddRow("Ebene1", list);
            list = new List<FrameworkElement>
                    {
                        BuildGrid(paths[2]),
                        BuildGrid(paths[3]),
                        BuildGrid(paths[4]),
                    };
            kinectProjectUiBuilder.AddRow("Ebene2", list);
            list = new List<FrameworkElement>
                    {
                        BuildGrid(paths[4]),
                        BuildGrid(Environment.CurrentDirectory + @"\images\mokup.jpg"),
                    };
            kinectProjectUiBuilder.AddRow("Ebene3", list);
        }
    }
}
