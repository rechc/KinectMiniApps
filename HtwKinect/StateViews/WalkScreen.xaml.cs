using Database;
using Database.DAO;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für WalkScreen.xaml
    /// </summary>
    public partial class WalkScreen : UserControl , ISwitchableUserControl
    {

        private TravelOffer _currentOffer;

        public WalkScreen()
        {
            InitializeComponent();
        }


        private void PaintImage(string imgPath)
        {
            try
            {
                var img = new Image { Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/"+imgPath)), Stretch = Stretch.Fill };
                BgImage.Source = img.Source; 
            }
            catch
            {
                Console.WriteLine("can't load or display the background image: " + imgPath);
            }
        }

        public Database.TravelOffer StopDisplay()
        {
            //TODO could anything be disposed ? 
            return _currentOffer;
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            _currentOffer = lastTravel;
            PaintImage(_currentOffer.ImgPath);
           
        }


    }
}
