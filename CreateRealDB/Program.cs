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

        public void CreateOffers(int price, String place, int rating, String hotelname, String anfahrt, int daycount, String futtertyp, CategoryEnum category, String image, bool top)
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

            dao.Save(offer);
        }

        public void CreateMyOffers() 
        {
            CreateOffers(110, "Amsterdam", 3 , "Hotel-Amsterdam", "Bus", 3, "Halbpension", CategoryEnum.City, "Citys\amsterdam.jpg", false);
        
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
