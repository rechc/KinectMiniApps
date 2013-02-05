using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generate_DB_Data
{
    class Program
    {
        static void Main(string[] args)
        {
            int entryCount = 0;
            if(args.Count() == 1)
                entryCount = int.Parse(args.First());
            new CreateData(entryCount);
            Console.WriteLine("-- Press any key to exit --");
            Console.ReadKey();
        }
    }
}
