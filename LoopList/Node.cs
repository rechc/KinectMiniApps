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
        public bool MarkedAbove { get; set; }
        public bool MarkedBelow { get; set; }
        public bool MarkedLeft { get; set; }
        public bool MarkedRight { get; set; }


        internal Node(FrameworkElement frameworkElement)
        {
            FrameworkElement = frameworkElement;
            MarkedRight = true;
            MarkedAbove = true;
            MarkedLeft = true;
            MarkedBelow = true;
            Left = this;
            Right = this;
            Below = this;
            Above = this;
        }
    }
}
