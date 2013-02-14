using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generate_DB_Data
{
    partial class CreateData
    {
        public Image ByteArrayToImage(byte[] fileBytes)
        {
            using (MemoryStream memStream = new MemoryStream(fileBytes))
            {
                return Image.FromStream(memStream);
            }
        }

        public byte[] ImageToByteArray(Image imageIn)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                imageIn.Save(memStream, System.Drawing.Imaging.ImageFormat.Gif);
                return memStream.ToArray();
            }
        }

    }
}
