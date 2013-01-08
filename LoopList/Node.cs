using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LoopList
{
    public class Node
    {
        private Node left, right, above, below;
        private FrameworkElement frameworkElement;
        private bool markedAbove, markedBelow, markedLeft, markedRight;

        internal void markAbove()
        {
            markedAbove = true;
        }

        internal void markBelow()
        {
            markedBelow = true;
        }

        internal void markLeft()
        {
            markedLeft = true;
        }

        internal void markRight()
        {
            markedRight = true;
        }

        internal void unmarkBelow()
        {
            markedBelow = false;
        }

        internal void unmarkAbove()
        {
            markedAbove = false;
        }

        internal void unmarkLeft()
        {
            markedLeft = false;
        }

        internal void unmarkRight()
        {
            markedRight = false;
        }

        internal bool isMarkedAbove()
        {
            return markedAbove;
        }

        internal bool isMarkedBelow()
        {
            return markedBelow;
        }

        internal bool isMarkedLeft()
        {
            return markedLeft;
        }

        internal bool isMarkedRight()
        {
            return markedRight;
        }

        internal Node(FrameworkElement frameworkElement)
        {
            this.frameworkElement = frameworkElement;
            markRight();
            markAbove();
            markLeft();
            markBelow();
            this.left = this;
            this.right = this;
            this.below = this;
            this.above = this;
        }

        internal void setLeft(Node left)
        {
            this.left = left;
        }

        internal void setRight(Node right)
        {
            this.right = right;
        }

        internal FrameworkElement getFrameworkElement()
        {
            return frameworkElement;
        }

        internal Node getRight()
        {
            return right;
        }

        internal Node getLeft()
        {
            return left;
        }

        internal Node getAbove()
        {
            return above;
        }

        internal Node getBelow()
        {
            return below;
        }

        internal void setBelow(Node below)
        {
            this.below = below;
        }

        internal void setAbove(Node above)
        {
            this.above = above;
        }
    }
}
