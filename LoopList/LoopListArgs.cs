using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopList
{
    public class LoopListTextArgs : EventArgs
    {
        private readonly Direction _direction;
        internal LoopListTextArgs(Direction direction)
        {
            _direction = direction;
        }

        public Direction GetDirection()
        {
            return _direction;
        }
    }

    public class LoopListArgs: LoopListTextArgs
    {
        private readonly int _id;

        internal LoopListArgs(Direction direction, int id): base(direction)
        {
            _id = id;
        }

        public int GetId()
        {
            return _id;
        }
    }

    public enum Direction { Left, Right, Top, Down}
}
