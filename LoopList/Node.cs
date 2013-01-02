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
        private Node left, right;
        private FrameworkElement frameworkElement;

        internal Node(FrameworkElement frameworkElement)
        {
            this.frameworkElement = frameworkElement;
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
    }
}
