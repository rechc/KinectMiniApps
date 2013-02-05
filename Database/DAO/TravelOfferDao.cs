using System;
using System.Collections.Generic;
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

                return (from offer in con.TravelOfferSet
                        select offer).ToList();
            } 
        }

        public TravelOffer SelectById(int offerId)
        {
            using (var con = new Model1Container())
            {

                var obj = (from offer in con.TravelOfferSet
                           where offerId == offer.OfferId
                           select offer).FirstOrDefault();
                if (obj == null)
                    throw new Exception("No entry found. Wrong Id");
                return obj;
            } 
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
