using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Generate_DB_Data
{
    partial class CreateData
    {
        public BitmapImage ByteToImage(byte bytes)
        {
            BitmapImage btm;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                btm = new BitmapImage();
                btm.BeginInit();
                btm.StreamSource = ms;
                btm.CacheOption = BitmapCacheOption.OnLoad;
                btm.EndInit();
                btm.Freeze();
            }
            return btm;
        }

        public byte ImageToByte(BitmapImage imageSource)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageSource.StreamSource.CopyTo(ms); //todo doesnt work so
                return Convert.ToByte(ms.ToString());
            }
        }


    }
}
