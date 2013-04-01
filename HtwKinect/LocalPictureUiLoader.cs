using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AccessoryLib;
using Database;
using Database.DAO;
using System.Diagnostics;

namespace HtwKinect
{
    /// <summary>
    /// Diese Klasse lädt die Angebote in die Loop-Liste
    /// </summary>
    class LocalPictureUiLoader : IUiLoader
    {
        /// <summary>
        /// Loads offers from db and add it to loop list
        /// </summary>
        /// <param name="kinectProjectUiBuilder"></param>
        /// <param name="firstShownOffer">offer which should shown first</param>
        public void LoadElementsIntoList(KinectProjectUiBuilder kinectProjectUiBuilder, TravelOffer firstShownOffer)
        {
            var offerDao = new TravelOfferDao();
            List<LoopListEntry> list = new List<LoopListEntry> ();
            List<TravelOffer> dbList = offerDao.SelectAllTopOffers();
            foreach (var offer in dbList)
            {
                Grid grid = new Grid();
                BuildBackground(grid, offer.ImgPath);
                BuildInfoBox(grid, offer);

                LoopListEntry entry = new LoopListEntry { FrameworkElement = grid, Id = offer.OfferId };

                if(offer.OfferId == firstShownOffer.OfferId)
                    list.Insert(0, entry);
                else
                    list.Add(entry); 
            }
            try
            {
                MiniGame.MiniGameControl mg = new MiniGame.MiniGameControl();
                mg.Start(KinectHelper.Instance.Sensor);
                KinectHelper.Instance.ReadyEvent += (sender, _) => Instance_ReadyEvent(mg, kinectProjectUiBuilder.GetLoopList());

                LoopListEntry entry = new LoopListEntry { FrameworkElement = mg, Id = -1 };
                list.Add(entry);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            kinectProjectUiBuilder.AddRow("Top", list);

            //todo maybe ask db for categories and not enum
            foreach (CategoryEnum category in Enum.GetValues(typeof(CategoryEnum)).Cast<CategoryEnum>()) 
            {
                list = new List<LoopListEntry>();
                dbList = offerDao.SelectOfferyByCategory(category);
                foreach (var offer in dbList)
                {
                    Grid grid = new Grid();
                    BuildBackground(grid, offer.ImgPath);
                    BuildInfoBox(grid, offer);
                    LoopListEntry entry = new LoopListEntry { FrameworkElement = grid, Id = offer.OfferId };
                    list.Add(entry);
                }
                kinectProjectUiBuilder.AddRow(dbList.First().Category.CategoryName, list);
            }
        }

        void Instance_ReadyEvent(MiniGame.MiniGameControl mg, LoopList.LoopList loopList)
        {
            if (loopList.IsShowing(mg))
            {
                mg.MinigameSkeletonEvent(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.DepthImagePixels, KinectHelper.Instance.ColorPixels);
                KinectHelper.Instance.SetTransform(mg);
            }
            else
            {
                mg.Stop();
            }
           
        }

        #region BackgroundPicture
        /// <summary>
        /// Adds background picture to grid
        /// </summary>
        private void BuildBackground(Grid grid, string imgPath)
        {
            grid.Background = new ImageBrush(new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute)));
        }
        #endregion

        #region InfoBox
        /// <summary>
        /// Creates the InfoBox for the current offer
        /// </summary>
        private void BuildInfoBox(Grid grid, TravelOffer offer)
        {
            try
            {
                var infoBanner = new InfoBanner.InfoBanner();
                infoBanner.HorizontalAlignment = HorizontalAlignment.Left;
                infoBanner.Start(offer);
                grid.Children.Add(infoBanner);
            }
            catch
            {
                Console.WriteLine("Error in LocalPictureUiLoader");
            }
        }

        #endregion

    }
}
