﻿using Database;
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
            Category.Text = offer.Category.CategoryName;
            Rating.Text = "Bewertung: " + offer.HotelRating;
            HotelName.Text = offer.HotelName;
            Place.Text = offer.Place;
            PricePerPerson.Text = offer.PricePerPerson + "€\n pro Person";
            TravelInfo.Text = offer.DayCount + " tägige " + offer.TravelType + "\ninkl. " + offer.BoardType;
            string extInfo = "";
            foreach (ExtendedInformation information in offer.ExtendedInformation)
                extInfo += ("-" + information.Information + "\n");
            ExtendetInfo.Text = extInfo;
        }
    }
}
