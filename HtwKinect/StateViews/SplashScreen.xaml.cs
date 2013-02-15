using Database;
using Database.DAO;
using System;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : UserControl, ISwitchableUserControl
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        private TravelOffer _currentOffer;

        public SplashScreen()
        {
            InitializeComponent();
            SetSpashScreenOffer(new TravelOfferDao().SelectRandomTopOffer());
        }

        public void StartNewOfferTimer(int milliseconds)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
            _timer.IsEnabled = true;
            _timer.Tick += SelectNewRandomOffer;
            _timer.Start();
        }

        private void SelectNewRandomOffer(object sender, EventArgs e)
        {
            SetSpashScreenOffer(new TravelOfferDao().SelectRandomTopOffer());
        }

        public void StopNewOfferTimer()
        {
            _timer.IsEnabled = false;
            _timer.Stop();
        }

        public void SetSpashScreenOffer(TravelOffer offer)
        {
            if (offer != null) 
            {
                _currentOffer = offer;
            }
            Category.Text = offer.Category.CategoryName;
            Rating.Text = "Bewertung: " + offer.HotelRating;
            HotelName.Text = offer.HotelName;
            Place.Text = offer.Place;
            PricePerPerson.Text = offer.PricePerPerson + ",-\n pro Person";
            TravelInfo.Text = offer.DayCount + " tägige " + offer.TravelType + ", inkl. " + offer.BoardType;
            string extInfo = "";
            foreach (ExtendedInformation information in offer.ExtendedInformation)
                extInfo += ("-" + information.Information + "\n");
            ExtendedInfo.Text = extInfo;
        }

        public Database.TravelOffer StopDisplay()
        {
            //TODO could anything be disposed ? 
            return _currentOffer;
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            if (lastTravel != null)
            {
                SetSpashScreenOffer(lastTravel);
            }
            else 
            {
                SetSpashScreenOffer(new TravelOfferDao().SelectRandomTopOffer());
            }
        }

        private void UnloadWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            StopNewOfferTimer();
        }
    }
}
