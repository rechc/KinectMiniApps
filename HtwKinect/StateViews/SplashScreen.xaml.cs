using Database;
using Database.DAO;
using System;
using System.Timers;
using System.Windows.Controls;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : UserControl, ISwitchableUserControl
    {
        Timer _timer = new Timer();

        public SplashScreen()
        {
            InitializeComponent();
            SetSpashScreenOffer(new TravelOfferDao().SelectRandomTopOffer());
        }

        public void StartNewOfferTimer(int miliseconds)
        {
            _timer.Interval = miliseconds;
            _timer.Enabled = true;
            _timer.Elapsed += SelectNewRandomOffer;
            _timer.Start();
        }

        private void SelectNewRandomOffer(object sender, ElapsedEventArgs e)
        {
            SetSpashScreenOffer(new TravelOfferDao().SelectRandomTopOffer());
        }

        public void StopNewOfferTimer()
        {
            _timer.Enabled = false;
            _timer.Stop();
        }

        public void SetSpashScreenOffer(TravelOffer offer)
        {
            Category.Dispatcher.BeginInvoke(
                    new Action(() => Category.Text = offer.Category.CategoryName));
            Rating.Dispatcher.BeginInvoke(
                    new Action(() => Rating.Text = "Bewertung: " + offer.HotelRating));
            HotelName.Dispatcher.BeginInvoke(
                    new Action(() => HotelName.Text = offer.HotelName));
            Place.Dispatcher.BeginInvoke(
                    new Action(() => Place.Text = offer.Place));
            PricePerPerson.Dispatcher.BeginInvoke(
                    new Action(() => PricePerPerson.Text = offer.PricePerPerson + ",-\n pro Person"));
            TravelInfo.Dispatcher.BeginInvoke(
                    new Action(() => TravelInfo.Text = offer.DayCount + " tägige " + offer.TravelType + ", inkl. " + offer.BoardType));
            string extInfo = "";
            foreach (ExtendedInformation information in offer.ExtendedInformation)
                extInfo += ("-" + information.Information + "\n");
            ExtendetInfo.Dispatcher.BeginInvoke(
                    new Action(() => ExtendetInfo.Text = extInfo));
        }

        public Database.TravelOffer StopDisplay()
        {
            throw new NotImplementedException();
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            throw new NotImplementedException();
        }

        private void UnloadWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            StopNewOfferTimer();
        }
    }
}
