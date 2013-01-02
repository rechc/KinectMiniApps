using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace LoopList
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class LoopList : UserControl
    {
        private Border left, right;
        private Node first, node;
        private int animating;
        private int lastX;
        private double autoDrag;


        public LoopList()
        {
            InitializeComponent();
            left = new Border();
            left.BorderThickness = new Thickness(10, 5, 10, 5);
            left.RenderTransform = new TranslateTransform();
            left.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            right = new Border();
            right.BorderThickness = new Thickness(10, 5, 10, 5);
            right.RenderTransform = new TranslateTransform();
            right.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            rootGrid.Children.Add(left);
            rootGrid.Children.Add(right);

        }

        public void setAutoDragOffset(double autoDrag)
        {
            this.autoDrag = autoDrag;
        }

        private void initNode(FrameworkElement frameworkElement)
        {
            this.node = new Node(frameworkElement);
            this.first = node;
            node.setRight(node);
            node.setLeft(node);
        }

        public Node addToLeft(Node anchor, FrameworkElement frameworkElement)
        {
            this.node = anchor;
            if (node == null)
            {
                initNode(frameworkElement);
            }
            else
            {
                Node newNode = new Node(frameworkElement);
                node.setLeft(newNode);
                newNode.setLeft(first);
                newNode.setRight(node);
                first.setRight(newNode);
                node = newNode;
            }
            right.Child = node.getFrameworkElement();

            return node;
        }

        public Node addToRight(Node anchor, FrameworkElement frameworkElement)
        {
            this.node = anchor;
            if (node == null)
            {
                initNode(frameworkElement);
            }
            else
            {
                Node newNode = new Node(frameworkElement);
                node.setRight(newNode);
                newNode.setRight(first);
                newNode.setLeft(node);
                first.setLeft(newNode);
                node = newNode;
            }
            right.Child = node.getFrameworkElement();
            return node;
        }

        private bool moreThanOneNodeExists()
        {
            if (node.getLeft() != node.getRight())
            {
                return true;
            }
            return false;
        }
        public bool drag(int xDistance)
        {
            if (animating == 0 && moreThanOneNodeExists())
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                ttLeft.X = (double)ttLeft.GetValue(TranslateTransform.XProperty);
                ttRight.X = (double)ttRight.GetValue(TranslateTransform.XProperty);

                ttLeft.BeginAnimation(TranslateTransform.XProperty, null);
                ttRight.BeginAnimation(TranslateTransform.XProperty, null);
                
                ttRight.X += xDistance;
                ttLeft.X += xDistance;

                if ((ttRight.X <= -right.ActualWidth || ttRight.X >= right.ActualWidth))
                {
                    Border tmp = right;
                    right = left;
                    left = tmp;
                    lastX = 0;
                    ttRight = (TranslateTransform)right.RenderTransform;
                    ttLeft = (TranslateTransform)left.RenderTransform;
                }

                if (ttRight.X < 0 && lastX >= 0)
                {
                    
                    ttLeft.X = right.ActualWidth + ttRight.X;

                    if (lastX == 0)
                    {
                        node = node.getRight();
                    }
                    else
                    {
                        node = node.getRight();
                        node = node.getRight();
                    }

                    left.Child = node.getFrameworkElement();
                    Debug.WriteLine(ttRight.X);
                    lastX = -1;
                }
                else
                {
                    if (ttRight.X > 0 && lastX <= 0)
                    {
                        ttLeft.X = -right.ActualWidth + ttRight.X;
                        if (lastX == 0)
                            node = node.getLeft();
                        else
                        {
                            node = node.getLeft();
                            node = node.getLeft();
                        }
                        left.Child = node.getFrameworkElement();
                        Debug.WriteLine(ttRight.X);
                        lastX = 1;
                    }
                }
                if (autoDrag > 0 && autoDrag < 1)
                {
                    if (ttRight.X >= right.ActualWidth * autoDrag)
                    {
                        animH(false);
                        return false;
                    } else if (ttRight.X <= -right.ActualWidth*(autoDrag))
                    {
                        animH(true);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        void animCompleted()
        {
            animating--;
        }

        public void animH(bool leftDir)
        {
            if (animating == 0 && moreThanOneNodeExists())
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                if (ttRight.X == 0)
                {
                    if (leftDir)
                    {
                        drag(-1);
                    }
                    else
                    {
                        drag(1);
                    }
                }
                animating = 2;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
                doubleAnimationCenter.From = ttRight.X;
                if (leftDir)
                    doubleAnimationCenter.To = -right.ActualWidth;
                else
                    doubleAnimationCenter.To = right.ActualWidth;
                doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                doubleAnimationCenter.Completed += (s, _) => animCompleted();
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                doubleAnimationLeft.From = ttLeft.X;
                doubleAnimationLeft.To = 0;
                doubleAnimationLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                doubleAnimationLeft.Completed += (s, _) => animCompleted();
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);

                Border tmp = right;
                right = left;
                left = tmp;
                lastX = 0;
            }
        }

        public void animBack()
        {
            if (animating == 0 && lastX != 0)
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                animating = 2;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
                doubleAnimationCenter.From = ttRight.X;
                doubleAnimationCenter.To = 0;
                doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                doubleAnimationCenter.Completed += (s, _) => animCompleted();
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                doubleAnimationLeft.From = ttLeft.X;
                if (lastX < 0)
                    doubleAnimationLeft.To = right.ActualWidth;
                if (lastX > 0)
                    doubleAnimationLeft.To = -right.ActualWidth;
                doubleAnimationLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                doubleAnimationLeft.Completed += (s, _) => animCompleted();
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);

            }
        }
    }
}
