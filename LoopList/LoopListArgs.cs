using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopList
{
    public class LoopListArgs: EventArgs
    {
        private readonly Direction _direction;
        internal LoopListArgs(Direction direction)
        {
            _direction = direction;
        }

        public Direction GetDirection()
        {
            return _direction;
        }
    }

    public enum Direction { Left, Right, Top, Down}
}
