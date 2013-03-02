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
                       select p).FirstOrDefault();
           }
        }


        //todo clean db in specific intervall
        public void DeleteOldestPicture()
        {
            using (var con = new Model1Container())
            {
                var oldestPicture = (from p in con.PictureSet
                                     orderby p.Time descending
                                     select p).FirstOrDefault();
                con.PictureSet.Remove(oldestPicture);
            }
        }

        public void SavePicture(byte[] noPersonPicture)
        {
            var lastTime = SelectLastTakenPicture().Time;

            if ((DateTime.Now - lastTime) > new TimeSpan(0, 0, 5, 0)) //only take picture each 5 minutes //todo set time intervall
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
}
