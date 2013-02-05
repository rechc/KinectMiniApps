using Database;
using System.Windows.Controls;
using System.Linq;


namespace InfoBanner
{
    /// <summary>
    /// Interaction logic for InfoBanner.xaml
    /// </summary>
    public partial class InfoBanner : UserControl
    {
        public InfoBanner()
        {
            InitializeComponent();
        }

        public void Start(TravelOffer offer)
        {
            Categorie.Content = offer.Category.CategoryName;
            Rating.Content = "Bewertung: " + offer.HotelRating;
            HotelName.Content = offer.HotelName;
            Place.Content = offer.Place;
            PricePerPerson.Content = offer.PricePerPerson + "€\n pro Person";
            TravelInfo.Content = offer.DayCount + " tägige " + offer.TravelType + "\ninkl. " + offer.BoardType;
            string extInfo = offer.ExtendedInformation.Aggregate("", (current, info) => current + ("-" + info.Information + "\n"));
            ExtendetInfo.Content = extInfo;
        }
    }
}
