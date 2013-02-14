﻿using System;
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
                BuildGreenScreen(grid);
                BuildAccessoryScreen(grid);
                BuildInfoBox(grid, i);
                list.Add(grid);
            }
            MiniGame.MainWindow mg = new MiniGame.MainWindow();
            mg.Start(KinectHelper.Instance.Sensor);
            KinectHelper.Instance.ReadyEvent += (sender, _) => Instance_ReadyEvent(mg);
            list.Add(mg);

            kinectProjectUiBuilder.AddRow("Top", list);

            list = new List<FrameworkElement>();
            paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Beach");
            for (int i = 0; i < paths.Count(); i++)
            {
                Grid grid = new Grid();
                BuildBackground(grid, paths[i]);
                BuildGreenScreen(grid);
                BuildAccessoryScreen(grid);
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
                BuildGreenScreen(grid);
                BuildAccessoryScreen(grid);
                BuildInfoBox(grid, i);
                list.Add(grid);
            }
            kinectProjectUiBuilder.AddRow("Snow", list);
        }

        void Instance_ReadyEvent(MiniGame.MainWindow mg)
        {
            mg.MinigameSkeletonEvent(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.DepthImagePixels, KinectHelper.Instance.ColorPixels);
            TansformFrameworkElement(mg);
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

        #region GreenScreen
        private void BuildGreenScreen(Grid grid)
        {
            try
            {
                var instance = KinectHelper.Instance;
                var gsc = new GreenScreenControl.GreenScreenControl();
                gsc.Start(instance.Sensor, false);
                instance.ReadyEvent += (sender, args) => RenderGreenScreen(gsc);
                grid.Children.Add(gsc);
            }
            catch
            {
                //TODO logging
                //Dieser Try Catch ist dazu da, damit die Bilder geladen werden können, auch wenn kein Kinectsensor angeschloßen ist.
            }
        }

        private void RenderGreenScreen(GreenScreenControl.GreenScreenControl greenScreenControl)
        {

            if (((FrameworkElement)greenScreenControl.Parent).Parent == null)
            {
                return; //nur auf dingen die auch angezeigt werden bitte, danke.
            }
            var instance = KinectHelper.Instance;
            greenScreenControl.InvalidateVisual(instance.DepthImagePixels, instance.ColorPixels);
            TansformFrameworkElement(greenScreenControl);
        }
        #endregion

        #region AccessoryLib
        private void BuildAccessoryScreen(Grid grid)
        {
            try
            {
                var kinectHelper = KinectHelper.Instance;
                AccessoryItem hat = new AccessoryItem(AccessoryPositon.Hat, @"images\Accessories\Hat.png", 0.25);
                var accessoryControl = new AccessoryControl();
                accessoryControl.AccessoryItems.Add(hat);
                accessoryControl.Start(kinectHelper.Sensor);
                kinectHelper.ReadyEvent += (sender, args) => RenderAccessoryItems(accessoryControl);
                grid.Children.Add(accessoryControl);
            }
            catch
            {
            }
        }

        private void RenderAccessoryItems(AccessoryControl accessoryControl)
        {
            if (((FrameworkElement)accessoryControl.Parent).Parent == null)
            {
                return; //nur auf dingen die auch angezeigt werden bitte, danke.
            }
            var instance = KinectHelper.Instance;
            accessoryControl.SetSkeletons(instance.Skeletons);
            TansformFrameworkElement(accessoryControl);
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

        private void TansformFrameworkElement(FrameworkElement frameworkElement)
        {
            KinectHelper.Instance.SetTransform(frameworkElement);
        }

    }
}
