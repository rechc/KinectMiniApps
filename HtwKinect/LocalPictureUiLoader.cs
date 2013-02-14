using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AccessoryLib;
using Database.DAO;

namespace HtwKinect
{
    /*Diese Klasse lädt lokale Testbilder in die LoopList*/
    class LocalPictureUiLoader : IUiLoader
    {
        public void LoadElementsIntoList(KinectProjectUiBuilder kinectProjectUiBuilder)
        {
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Top");
            List<FrameworkElement> list = new List<FrameworkElement> ();
            for (int i = 0; i < paths.Count(); i++) {
                Grid grid = new Grid();
                BuildBackground(grid, paths[i]);
                BuildInfoBox(grid, i);
                list.Add(grid);
            }
            try
            {
                MiniGame.MainWindow mg = new MiniGame.MainWindow();
                mg.Start(KinectHelper.Instance.Sensor);
                KinectHelper.Instance.ReadyEvent += (sender, _) => Instance_ReadyEvent(mg);
                list.Add(mg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            kinectProjectUiBuilder.AddRow("Top", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Beach");
            for (int i = 0; i < paths.Count(); i++)
            {
                Grid grid = new Grid();
                BuildBackground(grid, paths[i]);
                BuildInfoBox(grid, i);
                list.Add(grid);
            }
            kinectProjectUiBuilder.AddRow("Beach", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Snow");

            for (int i = 0; i < paths.Count(); i++)
            {
                Grid grid = new Grid();
                BuildBackground(grid, paths[i]);
                BuildInfoBox(grid, i);
                list.Add(grid);
            }
            kinectProjectUiBuilder.AddRow("Snow", list);
        }

        void Instance_ReadyEvent(MiniGame.MainWindow mg)
        {
            mg.MinigameSkeletonEvent(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.DepthImagePixels, KinectHelper.Instance.ColorPixels);
            KinectHelper.Instance.SetTransform(mg);
        }

        #region BackgroundPicture
        private void BuildBackground(Grid grid, string imgPath)
        {
            try
            {
                var img = new Image { Source = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute)), Stretch = Stretch.Fill };
                grid.Children.Add(img);
            }
            catch
            {
            }
        }
        #endregion

        #region InfoBox
        private void BuildInfoBox(Grid grid, int dbId)
        {
            try
            {
                var infoBanner = new InfoBanner.InfoBanner();
                infoBanner.HorizontalAlignment = HorizontalAlignment.Left;
                var offer = new TravelOfferDao().SelectById(dbId + 1);
                infoBanner.Start(offer);
                grid.Children.Add(infoBanner);
            }
            catch
            {
            }
        }

        #endregion

    }
}
