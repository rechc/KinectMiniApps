using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class TravelOfferDao
    {

        private TravelOffer _offer;

        private bool IsNew()
        {
            return _offer.OfferId == 0;
        }

        private void Insert()
        {
            using (var con = new Model1Container())
            {
                con.TravelOfferSet.Add(_offer);
                con.SaveChanges();
            }
        }

        private void Update()
        {
            using (var con = new Model1Container())
            {

                var offerEntity = con.TravelOfferSet.Single(o => o.OfferId == _offer.OfferId);
                con.Entry(offerEntity).CurrentValues.SetValues(_offer);
                con.SaveChanges();
            } 
        }

        public List<TravelOffer> SelectAllOffers()
        {
            using (var con = new Model1Container())
            {

                return (from offer in con.TravelOfferSet.Include("Category")
                        select offer).ToList();
            } 
        }

        public TravelOffer SelectById(int offerId)
        {
            try
            {
                using (var con = new Model1Container())
                {

                    var obj = (from offer in con.TravelOfferSet
                                             .Include("Category")
                                             .Include("ExtendedInformation")
                               where offerId == offer.OfferId
                               select offer).FirstOrDefault();
                    if (obj == null)
                        throw new Exception("No entry found. Wrong Id");
                    return obj;
                }
            }
            catch (Exception)
            {
                return CreateDefaultObject();
            }
        }

        private TravelOffer CreateDefaultObject()
        {
            var exInf = new Collection<ExtendedInformation>
                            {new ExtendedInformation() {Information = "please fill database"}};
            return new TravelOffer()
                       {
                           Category = new Category(){CategoryName = "No Db data", CategoryId = 0},
                           BoardType = "no data",
                           CategoryId = 0,
                           DayCount = 0,
                           HotelName = "no data",
                           HotelRating = 0,
                           ImgPath = "empty",
                           OfferId = 0,
                           Place = "no data",
                           PricePerPerson = 123.4,
                           TravelType = "no data",
                           ExtendedInformation = exInf
                       };
        }

        public void Save(TravelOffer offer)
        {
            _offer = offer;
            if (IsNew())
                Insert();
            else
                Update();
        }
    }
}
