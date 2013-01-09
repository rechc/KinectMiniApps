using System.Windows;

namespace LoopList
{
    public class Node
    {
        private Node _left, _right, _above, _below;
        private readonly FrameworkElement _frameworkElement;
        private bool _markedAbove, _markedBelow, _markedLeft, _markedRight;

        internal void MarkAbove()
        {
            _markedAbove = true;
        }

        internal void MarkBelow()
        {
            _markedBelow = true;
        }

        internal void MarkLeft()
        {
            _markedLeft = true;
        }

        internal void MarkRight()
        {
            _markedRight = true;
        }

        internal void UnmarkBelow()
        {
            _markedBelow = false;
        }

        internal void UnmarkAbove()
        {
            _markedAbove = false;
        }

        internal void UnmarkLeft()
        {
            _markedLeft = false;
        }

        internal void UnmarkRight()
        {
            _markedRight = false;
        }

        internal bool IsMarkedAbove()
        {
            return _markedAbove;
        }

        internal bool IsMarkedBelow()
        {
            return _markedBelow;
        }

        internal bool IsMarkedLeft()
        {
            return _markedLeft;
        }

        internal bool IsMarkedRight()
        {
            return _markedRight;
        }

        internal Node(FrameworkElement frameworkElement)
        {
            _frameworkElement = frameworkElement;
            MarkRight();
            MarkAbove();
            MarkLeft();
            MarkBelow();
            _left = this;
            _right = this;
            _below = this;
            _above = this;
        }

        internal void SetLeft(Node left)
        {
            _left = left;
        }

        internal void SetRight(Node right)
        {
            _right = right;
        }

        internal FrameworkElement GetFrameworkElement()
        {
            return _frameworkElement;
        }

        internal Node GetRight()
        {
            return _right;
        }

        internal Node GetLeft()
        {
            return _left;
        }

        internal Node GetAbove()
        {
            return _above;
        }

        internal Node GetBelow()
        {
            return _below;
        }

        internal void SetBelow(Node below)
        {
            _below = below;
        }

        internal void SetAbove(Node above)
        {
            _above = above;
        }
    }
}
