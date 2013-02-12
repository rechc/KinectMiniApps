using Database;
using Database.DAO;
using System;
using System.Windows.Controls;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : UserControl, ISwitchableUserControl
    {
        public SplashScreen()
        {
            InitializeComponent();
            SetSpashScreenOffer(new TravelOfferDao().SelectRandomTopOffer());
        }

        public void SetSpashScreenOffer(TravelOffer offer)
        {
            Category.Text = offer.Category.CategoryName;
            Rating.Text = "Bewertung: " + offer.HotelRating;
            HotelName.Text = offer.HotelName;
            Place.Text = offer.Place;
            PricePerPerson.Text = offer.PricePerPerson + "€\n pro Person";
            TravelInfo.Text = offer.DayCount + " tägige " + offer.TravelType + ", inkl. " + offer.BoardType;
            string extInfo = "";
            foreach (ExtendedInformation information in offer.ExtendedInformation)
                extInfo += ("-" + information.Information + "\n");
            ExtendetInfo.Text = extInfo;
        }

        public Database.TravelOffer StopDisplay()
        {
            throw new NotImplementedException();
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            throw new NotImplementedException();
        }
    }
}
