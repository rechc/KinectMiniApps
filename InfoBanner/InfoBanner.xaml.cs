using Database;
using System.Windows.Controls;
using System.Linq;
using System;


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

        /// <summary>
        /// Writes the informations given by the object to the info banner screen
        /// </summary>
        /// <param name="offer">Offer which like to show</param>
        public void Start(TravelOffer offer)
        {

            char star = '\u2605';
            String bullet = Convert.ToString('\u2023');
            String euro = Convert.ToString('\u20AC');
            String ratingStars = "";

            for (int i = 0; i <= offer.HotelRating; i++)
            {
                ratingStars += Convert.ToString(star);
            }
            if (offer.Category.CategoryName == "Wandern")
                Category.Text = "Wanderurlaub";
            else
                Category.Text = offer.Category.CategoryName + "urlaub";
            Stars.Text = ratingStars;
            HotelName.Text = offer.HotelName;
            Place.Text = offer.Place;
            PricePerPerson.Text = offer.PricePerPerson + ",- "+ euro +"\npro Person";
            TravelInfo.Text = offer.DayCount + "-tägige " + offer.TravelType + "\ninkl. " + offer.BoardType;

            string extInfo = "";
            foreach (ExtendedInformation information in offer.ExtendedInformation)
                extInfo += (bullet + " " + information.Information + "\n");
            ExtendetInfo.Text = extInfo;

            //var iterator = offer.ExtendedInformation.GetEnumerator();
            ////ExtendetInfoImg1.Source = new BitmapImage(){ Source = new BitmapImage(new Uri(Directory.GetFiles(Environment.CurrentDirectory + @"\Images\Arrow.png").First(), UriKind.RelativeOrAbsolute)), Stretch = Stretch.Fill };
            //ExtendetInfo1.Text = (iterator.MoveNext()) ? iterator.Current.Information : "";
            //ExtendetInfo2.Text = (iterator.MoveNext()) ? iterator.Current.Information : "";
            //ExtendetInfo3.Text = (iterator.MoveNext()) ? iterator.Current.Information : "";
            //ExtendetInfo4.Text = (iterator.MoveNext()) ? iterator.Current.Information : "";
        }
    }
}
