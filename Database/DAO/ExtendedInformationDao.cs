using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class ExtendedInformationDao
    {
        private readonly Model1Container _context;
        private ICollection<ExtendedInformation> _informations;

        public ExtendedInformationDao(Model1Container context)
        {
            _context = context;
        }

        public List<ExtendedInformation> SelectAllCountries()
        {
            return (from info in _context.ExtendedInformationSet
                    select info).ToList();
        }

        public void Save(ICollection<ExtendedInformation> informations)
        {
            _informations = informations;

            //SingleInfoDao decides for insert or update
            var singleInfoDao = new SingleInfoDao(_context);
            foreach (var info in _informations)
            {
                singleInfoDao.Save(info);
            }
        }
    }

    class SingleInfoDao
    {
        private readonly Model1Container _context;
        private ExtendedInformation _information;

        public SingleInfoDao(Model1Container context)
        {
            _context = context;
        }

        private bool IsNew()
        {
            return _information.ExtendetInformationId == 0;
        }

        public void Save(ExtendedInformation info)
        {
            _information = info;

            if (IsNew())
                Insert();
            else
                Update();
        }

        private void Update()
        {
            var infoEntity =
                _context.ExtendedInformationSet.Single(
                    o => o.ExtendetInformationId == _information.ExtendetInformationId);
            _context.Entry(infoEntity).CurrentValues.SetValues(_information);
        }

        private void Insert()
        {
            _context.ExtendedInformationSet.Add(_information);
        }
    }
}
