using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class PictureDao
    {
        private Picture _picture;

        private void Insert()
        {
            using (var con = new Model1Container())
            {
                con.PictureSet.Add(_picture);
                con.SaveChanges();
            }
        }

        public Picture SelectLastTakenPicture()
        {
           using(var con = new Model1Container())
           {
               return (from p in con.PictureSet
                       orderby p.Time ascending 
                       select p).First(); //todo check if database is not empty
           }
        }

        public void DeleteOldestPicture()
        {
            using (var con = new Model1Container())
            {
                var oldestPicture = (from p in con.PictureSet
                                     orderby p.Time descending
                                     select p).First(); //todo check if database is not empty
                con.PictureSet.Remove(oldestPicture);
            }
        }

        public void SavePicture(byte[] noPersonPicture)
        {

            _picture = new Picture()
                           {
                               Time = DateTime.Now,
                               NoPersonPicture = noPersonPicture
                           };
            Insert();
        }

    }
}
