using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AccessoryLib;
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
            Grid grid;
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Top");
            List<FrameworkElement> list = new List<FrameworkElement> ();
            for (int i = 0; i < paths.Count(); i++) {
                grid = BuildBackground(new Grid(), paths[i]);
                grid = BuildGreenScreen(grid);
                grid = BuildAccessoryScreen(grid);
                list.Add(grid);
            }
            kinectProjectUiBuilder.AddRow("Top", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Beach");
            for (int i = 0; i < paths.Count(); i++)
            {
                grid = BuildBackground(new Grid(), paths[i]);
                grid = BuildGreenScreen(grid);
                grid = BuildAccessoryScreen(grid);
                list.Add(grid);
            }
            kinectProjectUiBuilder.AddRow("Beach", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Snow");

            for (int i = 0; i < paths.Count(); i++)
            {
                grid = BuildBackground(new Grid(), paths[i]);
                grid = BuildGreenScreen(grid);
                grid = BuildAccessoryScreen(grid);
                list.Add(grid);
            }
           
            kinectProjectUiBuilder.AddRow("Snow", list);

        }

        #region BackgroundPicture
        private Grid BuildBackground(Grid grid, string imgPath)
        {
            try
            {
                var img = new Image { Source = LoadImage(imgPath), Stretch = Stretch.Fill };
                grid.Children.Add(img);
            }
            catch
            {
            }
            return grid;
        }
        #endregion

        #region GreenScreen
        private Grid BuildGreenScreen(Grid grid)
        {
            try
            {
                var gsc = new GreenScreenControl.GreenScreenControl();
                grid.Children.Add(gsc);
                var instance = KinectHelper.Instance;
                gsc.Start(instance.Sensor, false);
                instance.ReadyEvent += (sender, args) => RenderGreenScreen(gsc);
            }
            catch
            {
                //TODO logging
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
        #endregion

        #region AccessoryLib
        private Grid BuildAccessoryScreen(Grid grid)
        {
            try
            {
                AccessoryItem hat = new AccessoryItem(AccessoryPositon.Hat, @"images\Accessories\Hat.png", 0.25);
                var accessoryControl = new AccessoryControl();
                accessoryControl.AccessoryItems.Add(hat);
                grid.Children.Add(accessoryControl);
                var instance = KinectHelper.Instance;
                accessoryControl.Start(instance.Sensor);
                instance.ReadyEvent += (sender, args) => RenderAccessoryItems(accessoryControl);

            }
            catch
            {
            }
            return grid;
        }

        private void RenderAccessoryItems(AccessoryControl accessoryControl)
        {
            if (((Grid)accessoryControl.Parent).Parent == null)
            {
                return; //nur auf dingen die auch angezeigt werden bitte, danke.
            }
            var instance = KinectHelper.Instance;
            accessoryControl.InvalidateVisual(instance.Skeletons);
        }
        #endregion

    }
}
