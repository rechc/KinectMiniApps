using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MiniGame
{
    class GridObjects
    {
        public Grid image;

        public int column;

        public int row;

        public GridObjects(Grid image, int column, int row)
        {
            this.image = image;
            this.column = column;
            this.row = row;
        }
    }
}
