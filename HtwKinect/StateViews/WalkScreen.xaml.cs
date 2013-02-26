using AccessoryLib;
using Database;
using Database.DAO;
using Microsoft.Kinect;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für WalkScreen.xaml
    /// </summary>
    public partial class WalkScreen : UserControl, ISwitchableUserControl
    {

        private TravelOffer _currentOffer;
        /* For not every frame a new variable to allocate */
        private KinectHelper helper;
        private Skeleton skeleton;

        public WalkScreen()
        {
            InitializeComponent();
        }


        private void PaintImage(string imgPath)
        {
            try
            {
                var img = new Image { Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/" + imgPath)), Stretch = Stretch.Fill };
                BgImage.Source = img.Source;
                BgImage.Stretch = Stretch.Fill;
            }
            catch
            {
                Console.WriteLine("can't load or display the background image: " + imgPath);
            }
        }

        public Database.TravelOffer StopDisplay()
        {
            StopGreenScreenAndHat();
            return _currentOffer;
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            _currentOffer = lastTravel;
            PaintImage(_currentOffer.ImgPath);
            StartGreenScreenAndHat();
        }

        #region GreenScreen and Accessory

        private void StopGreenScreenAndHat()
        {
            if (helper != null)
                helper.ReadyEvent -= (s, _) => HelperReady();
        }

        private void StartGreenScreenAndHat()
        {
            var helper = KinectHelper.Instance;
            GreenScreen.Start(helper.Sensor, false);// TODO wieder auf true sonst kein antialiasing
            Accessories.AccessoryItems.Clear();
            AccessoryItem hat = new AccessoryItem(AccessoryPositon.Hat, _currentOffer.Category.CategoryId, false);
            Accessories.AccessoryItems.Add(hat);
            Accessories.Start(helper.Sensor);
            helper.ReadyEvent += (s, _) => HelperReady();
        }



        /*
         * Event
         */
        private void HelperReady()
        {
            helper = KinectHelper.Instance;
            skeleton = helper.GetFixedSkeleton();
            GreenScreen.RenderImageData(helper.DepthImagePixels, helper.ColorPixels);
            Accessories.SetSkeletons(helper.Skeletons);
            KinectHelper.Instance.SetTransform(GreenScreen);
            KinectHelper.Instance.SetTransform(Accessories);
        }
        #endregion GreenScreen and Accessory
    }
}
