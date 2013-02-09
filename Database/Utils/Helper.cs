using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Utils
{
    public static class Helper
    {
        public static int GetRandomInteger(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max + 1);
        }
    }
}
