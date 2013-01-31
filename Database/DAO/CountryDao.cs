using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class CountryDao
    {
        private readonly Model1Container _context;
        private Country _country;

        public CountryDao(Model1Container context)
        {
            _context = context;
        }

        private bool IsNew()
        {
            return _country.CountryId == 0;
        }

        private void Insert()
        {
            _context.CountrySet.Add(_country);
        }

        private void Update()
        {
            var offerEntity = _context.CountrySet.Single(o => o.CountryId == _country.CountryId);
            _context.Entry(offerEntity).CurrentValues.SetValues(_country);
        }

        public List<Country> SelectAllCountries()
        {
            return (from country in _context.CountrySet
                    select country).ToList();
        }

        public Country SelectById(int countryId)
        {
            var obj = (from country in _context.CountrySet
                       where countryId == country.CountryId
                       select country).FirstOrDefault();
            if(obj == null)
                throw new Exception("No entry found. Wrong Id");
            return obj;
        }

        public void Save(Country country)
        {
            _country = country;
            if (IsNew())
                Insert();
            else
                Update();
        }
    }
}
