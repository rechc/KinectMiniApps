using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class ExtendedInformationDao
    {
        private ICollection<ExtendedInformation> _informations;

        public List<ExtendedInformation> SelectAllCountries()
        {
            using (var con = new Model1Container())
            {
                return (from info in con.ExtendedInformationSet
                        select info).ToList();
            } 
        }

        public void Save(ICollection<ExtendedInformation> informations)
        {
            _informations = informations;

            //SingleInfoDao decides for insert or update
            var singleInfoDao = new SingleInfoDao();
            foreach (var info in _informations)
            {
                singleInfoDao.Save(info);
            }
        }
    }

    class SingleInfoDao
    {
        private ExtendedInformation _information;

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
            using (var con = new Model1Container())
            {
                var infoEntity =
                    con.ExtendedInformationSet.Single(
                        o => o.ExtendetInformationId == _information.ExtendetInformationId);
                        con.Entry(infoEntity).CurrentValues.SetValues(_information);
                con.SaveChanges();
            }
        }

        private void Insert()
        {
            using (var con = new Model1Container())
            {
                con.ExtendedInformationSet.Add(_information);
                con.SaveChanges();
            }
        }
    }
}
