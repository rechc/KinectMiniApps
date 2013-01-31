using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class TravelOfferDao
    {
        private readonly Model1Container _context;
        private TravelOffer _offer;

        public TravelOfferDao(Model1Container context)
        {
            _context = context;
        }

        private bool IsNew()
        {
            return _offer.OfferId == 0;
        }

        private void Insert()
        {
            _context.TravelOfferSet.Add(_offer);
        }

        private void Update()
        {
            var offerEntity = _context.TravelOfferSet.Single(o => o.OfferId == _offer.OfferId);
            _context.Entry(offerEntity).CurrentValues.SetValues(_offer);
        }

        public List<TravelOffer> SelectAllOffers()
        {
            return (from offer in _context.TravelOfferSet
                    select offer).ToList();
        }

        public TravelOffer SelectById(int offerId)
        {
            var obj = (from offer in _context.TravelOfferSet
                    where offerId == offer.OfferId
                    select offer).FirstOrDefault();
            if(obj == null)
                throw new Exception("No entry found. Wrong Id");
            return obj;
        }

        public void Save(TravelOffer offer)
        {
            _offer = offer;
            if (IsNew())
                Insert();
            else
                Update();
            new CountryDao(_context).Save(offer.Country);
        }
    }
}
