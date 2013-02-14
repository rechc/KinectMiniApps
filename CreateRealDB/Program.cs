using Database;
using Database.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateRealDB
{
    class DBFill
    {
        public Model1Container _context;

        private void CreateCountryEntries()
        {
                //string[] countries = {"Spanien", "Deutschland", "USA", "Mallorca", "Frankreich"};
                foreach (var countryName in Enum.GetNames(typeof(CategoryEnum)))
                {
                    var c = new Category() { CategoryName = countryName };
                    _context.CategorySet.Add(c);
                } 
        }

        private void SaveToDb()
        {
                try
                {
                   _context.SaveChanges(); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Error in entry. Start db rollback");
                }
        }

        public void CreateOffers(int price, String place, int rating, String hotelname, String anfahrt, int daycount, String futtertyp, CategoryEnum category, String image, bool top, String zusatzinf, String zusatz2, String zusatz3)
        {
            var dao = new TravelOfferDao();
            var offer = new TravelOffer() 
            { 
                PricePerPerson = price,
                Place = place,
                HotelRating = rating,
                HotelName = hotelname,
                TravelType = anfahrt,
                DayCount = daycount,
                BoardType = futtertyp,
                CategoryId = (int) category,
                TopOffer = top,
            };
            ExtendedInformation ei = new ExtendedInformation()
            {
                Information = zusatzinf
            };
            offer.ExtendedInformation.Add(ei);
            if (zusatz2 != "")
            {
                ExtendedInformation ei2 = new ExtendedInformation()
                {
                    Information = zusatz2
                };
                offer.ExtendedInformation.Add(ei2);
            }
            if (zusatz3 != "")
            {
                ExtendedInformation ei3 = new ExtendedInformation()
                {
                    Information = zusatz3
                };
                offer.ExtendedInformation.Add(ei3);
            }
            offer.ExtendedInformation.Add(ei);
            dao.Save(offer);
        }

        public void CreateMyOffers() 
        {
            CreateOffers(169, "Amsterdam", 4, "Radisson Red", "Busreise", 3, "Frühstücksbüffet", CategoryEnum.City, "images/City/amsterdam.jpg", false, "Busabfahrt in Birkenfeld","","");
            CreateOffers(290, "Berlin", 4, "Atlona", "Flugreise", 4, "Vollpension", CategoryEnum.City, "images/City/berlin.jpg", false,"Flug ab Saarbrücken","außer Montags","");
            CreateOffers(182, "Hamburg", 3, "Hotel hinterm Hafen", "Busreise", 3, "Halbpension", CategoryEnum.City, "images/City/hamburg.jpg", false, "vorm Hafen rechts", "", "");
            CreateOffers(205, "Paris", 3, "Best Ostern", "Zugreise", 2, "Halbpension", CategoryEnum.City, "images/City/paris.jpg", true, "", "", "");
            CreateOffers(310, "Wien", 5, "Hotel Mozarto", "Flugreise", 4, "All Inclusive", CategoryEnum.City, "images/City/wien.jpg", false, "", "", "");
            CreateOffers(240, "London", 3, "Hotel Big Bang", "Flugreise", 3, "Halbpension", CategoryEnum.City, "images/City/london.jpg", false, "", "", "");
            CreateOffers(699, "LasVegas", 5, "Cesarius Palast", "Flugreise", 5, "Vollpension", CategoryEnum.City, "images/City/lasvegas.jpg", false, "Abflug Frankfurt a.M.", "", "");
            CreateOffers(549, "NewYork", 3, "Hotel Staars Residenz", "Flugreise", 6, "Halbpension", CategoryEnum.City, "images/City/newyork.jpg", false, "", "", "");
            CreateOffers(659, "SanFrancisco", 4, "Hotel Blondie", "Flugreise", 7, "Halbpension", CategoryEnum.City, "images/City/sanfrancisco.jpg", false, "", "", "");
            CreateOffers(899, "Sidney", 5, "Hotel Kängurus", "Flugreise", 14, "Halbpension", CategoryEnum.City, "images/City/sidney.jpg", true, "", "", "");

            CreateOffers(150, "Blankenberge", 3, "Hotel", "eigene Anreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/blankenberge.jpg", true, "", "", "");
            CreateOffers(299, "DominikanischeRepublik", 3, "Hotel", "Flugreise", 6, "Halbpension", CategoryEnum.Beach, "images/Beach/domrep.jpg", true, "", "", "");
            CreateOffers(329, "Fuerteventura", 3, "Hotel", "Flugreise", 5, "Vollpension", CategoryEnum.Beach, "images/Beach/fuerteventura.jpg", true, "", "", "");
            CreateOffers(199, "grancanaria", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/grancanaria.jpg", true, "", "", "");
            CreateOffers(199, "haweii", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/haweii.jpg", true, "", "", "");
            CreateOffers(199, "ibiza", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/ibiza.jpg", true, "", "", "");
            CreateOffers(199, "mallorca", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/mallorca.jpg", true, "", "", "");
            CreateOffers(199, "mauritius", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/mauritius.jpg", true, "", "", "");
            CreateOffers(199, "nordsee", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/nordsee.jpg", true, "", "", "");
            CreateOffers(199, "ruegen", 3, "Hotel", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/ruegen.jpg", true, "", "", "");

        }
       
        public void FlushDbData()
        {
                if (_context.Database.Exists())
                {
                  _context.Database.Delete();
                }
                _context.Database.CreateIfNotExists();
        }

        static void Main(string[] args)
        {
            DBFill dbf = new DBFill();
            using (var con = new Model1Container())
            {
                dbf._context = con;
                dbf.FlushDbData();
                dbf.CreateCountryEntries();
                dbf.SaveToDb(); 
            }
            dbf.CreateMyOffers();
        }
    }
}
