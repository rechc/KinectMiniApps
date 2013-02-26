using System;
using System.Windows;

namespace LoopList
{
    public class Node
    {
        internal event EventHandler NodeChangedEvent;

        public FrameworkElement FrameworkElement { get; internal set; }
        public int Id { get; internal set; }


        private Node _left;
        private Node _right;
        private Node _above;
        private Node _below;

        public Node Left
        {
            get { return _left; }
            set
            { 
                _left = value;
                FireNodeChanged();
            }
        }

        public Node Right
        {
            get { return _right; }
            set
            {
                _right = value;
                FireNodeChanged();
            }
        }

        public Node Above
        {
            get { return _above; }
            set
            {
                _above = value;
                FireNodeChanged();
            }
        }

        public Node Below
        {
            get { return _below; }
            set
            {
                _below = value;
                FireNodeChanged();
            }
        }

        
        internal Node(int id, FrameworkElement frameworkElement)
        {
            Id = id;
            FrameworkElement = frameworkElement;
            Left = this;
            Right = this;
            Below = this;
            Above = this;
        }

        private void FireNodeChanged()
        {
            if (NodeChangedEvent != null)
                NodeChangedEvent(this, EventArgs.Empty);
        }

        public bool HasRightNeighbour()
        {
            return Right != this;
        }

        public bool HasLeftNeighbour()
        {
            return Left != this;
        }

        public bool HasAboveNeighbour()
        {
            return Above != this;
        }

        public bool HasBelowNeighbour()
        {
            return Below != this;
        }
    }
}
