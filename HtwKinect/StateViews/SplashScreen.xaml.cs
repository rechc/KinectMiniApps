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
                    Category.Text = _currentOffer.Category.CategoryName;
                    Rating.Text = "Bewertung: " + _currentOffer.HotelRating;
                    HotelName.Text = _currentOffer.HotelName;
                    Place.Text = _currentOffer.Place;
                    PricePerPerson.Text = _currentOffer.PricePerPerson + ",-\n pro Person";
                    TravelInfo.Text = _currentOffer.DayCount + " tägige " + _currentOffer.TravelType + ", inkl. " + _currentOffer.BoardType;
                    string extInfo = "";
                    foreach (ExtendedInformation information in _currentOffer.ExtendedInformation)
                        extInfo += ("-" + information.Information + "\n");
                    ExtendedInfo.Text = extInfo; 
                }
        }

        public Database.TravelOffer StopDisplay()
        {
            return _currentOffer;
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            SetSpashScreenOffer(lastTravel);
        }

        private void UnloadWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            StopNewOfferTimer();
        }
    }
}
