using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Database;

namespace Generate_DB_Data
{
    class CreateData
    {
        private Model1Container _context;
        private readonly Random _random = new Random();

        public CreateData(int entryCount)
        {
            using (var con = new Model1Container())
            {
                for (int i = 0; i < entryCount; i++)
                {
                    CreateTravelOffer(i);
                }
                
            }
        }

        private void CreateTravelOffer(int i)
        {
            string[] travelType = {"Flugzeug", "Boot", "Busfahrt"};

            var offer = new TravelOffer()
                            {
                                BoardType = GetRandomBool() ? "Vollpension" : "Halbpension",
                                DayCount = GetRandomInteger(1, 14),
                                HotelName = "Hotel " + i,
                                Place = "Place " + 1,
                                PricePerPerson = GetRandomInteger(50, 1000),
                                TravelType = travelType[GetRandomInteger(0, travelType.Count() - 1)],
                                HotelRating = GetRandomInteger(1,5), //todo set all attributes
                            };

            var count = GetRandomInteger(0, 4);
            for (int j = 0; j < count; j++)
            {
                offer.ExtendedInformation.Add(CreateExtendedInfo(j));
            }

        }

        private ExtendedInformation CreateExtendedInfo(int j)
        {
            string info = "leer";
            switch (GetRandomInteger(1, 3))
            {
                case 1:
                     info = "Dies ist ein laaaaaaaaaaaaaaaaanger teeeeext " + j;
                    break;
                case 2:
                    info = "Mittel langer text " + j;
                    break;
                case 3:
                    info = "kurz " + j;
                    break;
            }
            return new ExtendedInformation(){Information = info};
        }

        private void SaveChanges()
        {
            using (var transaction = new TransactionScope())
            {
                try
                {
                    _context.SaveChanges();
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Error in entry. Start db rollback");
                }
            }
        }

        private void Flush_DB()
        {
            using (var context = new Model1Container())
            {
                if (context.Database.Exists())
                {
                    context.Database.ExecuteSqlCommand("alter database " + context.Database.Connection.Database + " set SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    context.Database.Delete();
                }
                context.Database.CreateIfNotExists();
            }
        }

        private int GetRandomInteger(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        private bool GetRandomBool()
        {
            return GetRandomInteger(0, 1) == 0 ? true : false;
        }

        private DateTime GetRandomDate()
        {
            var start = new DateTime(1995, 1, 1);

            var range = ((TimeSpan)(DateTime.Today - start)).Days;
            return start.AddDays(_random.Next(range));
        }

        private DateTime GetRandomDate(int startYear, int endYear)
        {
            var start = new DateTime(startYear, 1, 1);
            var end = new DateTime(endYear, 12, 31);

            var range = ((TimeSpan)(end - start)).Days;
            return start.AddDays(_random.Next(range));
        }
    }
}
