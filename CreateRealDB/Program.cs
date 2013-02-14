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
            CreateOffers(100, "Amsterdam", 3 , "HotelAmsterdam", "Bus", 3, "Halbpension", CategoryEnum.City, "Citys\amsterdam.jpg", false);
        
        }

        public void FlushDbData()
        {
            using (var context = new Model1Container())
            {
                if (context.Database.Exists())
                {
                  context.Database.Delete();
                }
                context.Database.CreateIfNotExists();
            }
        }

        static void Main(string[] args)
        {
            DBFill dbf = new DBFill();
            dbf.FlushDbData();
            dbf.CreateMyOffers();
            
        }
    }
}
