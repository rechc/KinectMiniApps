using System.Windows;
using System.Windows.Controls;

namespace LoopList
{
    public class Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public Node Above { get; set; }
        public Node Below { get; set; }

        public FrameworkElement FrameworkElement { get; set; }


        internal Node(FrameworkElement frameworkElement)
        {
            FrameworkElement = frameworkElement;
            Left = this;
            Right = this;
            Below = this;
            Above = this;
        }

        public bool HasHNeighbour()
        {
            return Right != this || Left != this;
        }

        public bool HasVNeighbour()
        {
            return Above != this || Below != this;
        }
    }
}
