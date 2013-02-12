using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Database;

namespace Generate_DB_Data
{
    class CreateData
    {
        private static string ConnectionString
        {
            get
            {
                // Specify the provider name, server and database.
                string providerName = "System.Data.SqlClient";
                string serverName = "localhost";
                string databaseName = "AdventureWorks";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = true;

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = "res://*/";

                return entityBuilder.ToString();
            }
        }

        private Model1Container _context;
        private readonly Random _random = new Random();

        public CreateData(int entryCount)
        {
            //string connectionString = ConnectionStringHelper.GetSqlCeConnectionString(@"C:\Users\Christian\workspace\KinectMiniApps\Generate DB Data\bin\Debug\Database1.sdf");

            using (var con = new Model1Container())
            {
                _context = con;
                FlushDbData();
                CreateCountryEntries();
                SaveToDb();

                for (int i = 0; i < entryCount; i++)
                {
                    CreateTravelOffer(i);
                }
                SaveToDb();
            }
        }

        private void CreateCountryEntries()
        {
            string[] countries = {"Spanien", "Deutschland", "USA", "Mallorca", "Frankreich"};
            foreach (var countryName in countries)
            {
                var c = new Categorie() {CategoryName = countryName};
                _context.CategorieSet.Add(c);
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
                                HotelRating = GetRandomInteger(1,5),
                                CategoryId = GetRandomInteger(1, _context.CategorieSet.Count()),
                                ImgPath = "path" //todo set path
                            };

            var count = GetRandomInteger(0, 4);
            for (int j = 0; j < count; j++)
            {
                offer.ExtendedInformation.Add(CreateExtendedInfo(j));
            }
            _context.TravelOfferSet.Add(offer);
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

        private void SaveToDb()
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

        private void FlushDbData()
        {
            using (var context = new Model1Container())
            {
                if (context.Database.Exists())
                {
                    //context.Database.ExecuteSqlCommand("alter database " + context.Database.Connection.Database + " set SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    context.Database.Delete();
                }
                context.Database.CreateIfNotExists();
            }
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
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
