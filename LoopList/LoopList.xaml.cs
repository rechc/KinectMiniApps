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
        private Border left, right, above;
        private Node currentNode;
        private int animating;
        private int lastX, lastY;
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

            above = new Border();
            above.BorderThickness = new Thickness(10, 5, 10, 5);
            above.RenderTransform = new TranslateTransform();
            above.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            rootGrid.Children.Add(left);
            rootGrid.Children.Add(right);
            rootGrid.Children.Add(above);
        }

        public void setAutoDragOffset(double autoDrag)
        {
            this.autoDrag = autoDrag;
        }

        public Node addToLeft(Node anchor, FrameworkElement frameworkElement)
        {
            this.currentNode = anchor;
            if (currentNode == null)
            {
                this.currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!currentNode.isMarkedLeft())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = currentNode.getLeft();
                currentNode.setLeft(newNode);
                currentNode.unmarkLeft();
                newNode.setRight(currentNode);
                newNode.unmarkRight();
                newNode.setLeft(first);
                first.setRight(newNode);
                currentNode = newNode;
            }
            right.Child = currentNode.getFrameworkElement();
            return currentNode;
        }

        public Node addToRight(Node anchor, FrameworkElement frameworkElement)
        {
            this.currentNode = anchor;
            if (currentNode == null)
            {
                this.currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!currentNode.isMarkedRight())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = currentNode.getRight();
                currentNode.setRight(newNode);
                currentNode.unmarkRight();
                newNode.setLeft(currentNode);
                newNode.unmarkLeft();
                newNode.setRight(first);
                first.setLeft(newNode);
                currentNode = newNode;
            }
            right.Child = currentNode.getFrameworkElement();
            return currentNode;
        }

        public Node addToAbove(Node anchor, FrameworkElement frameworkElement)
        {
            this.currentNode = anchor;
            if (currentNode == null)
            {
                this.currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!currentNode.isMarkedAbove())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = currentNode.getAbove();
                currentNode.setAbove(newNode);
                currentNode.unmarkAbove();
                newNode.setBelow(currentNode);
                newNode.unmarkBelow();
                newNode.setAbove(first);
                first.setBelow(newNode);
                currentNode = newNode;
            }
            right.Child = currentNode.getFrameworkElement();
            return currentNode;
        }

        public Node addToBelow(Node anchor, FrameworkElement frameworkElement)
        {
            this.currentNode = anchor;
            if (currentNode == null)
            {
                this.currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!currentNode.isMarkedBelow())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = currentNode.getBelow();
                currentNode.setBelow(newNode);
                currentNode.unmarkBelow();
                newNode.setAbove(currentNode);
                newNode.unmarkAbove();
                newNode.setBelow(first);
                first.setAbove(newNode);
                currentNode = newNode;
            }
            right.Child = currentNode.getFrameworkElement();
            return currentNode;
        }

        private bool hNeighbourExists()
        {
            if (!currentNode.isMarkedLeft() || !currentNode.isMarkedRight())
            {
                return true;
            }
            return false;
        }

        private bool vNeighbourExists()
        {
            if (!currentNode.isMarkedBelow() || !currentNode.isMarkedAbove())
            {
                return true;
            }
            return false;
        }

        public bool hDrag(int xDistance)
        {
            if (animating == 0)
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;
                if (ttRight.Y != 0) return true;
                if (hNeighbourExists())
                {
                    ttLeft.X = (double)ttLeft.GetValue(TranslateTransform.XProperty);
                    ttRight.X = (double)ttRight.GetValue(TranslateTransform.XProperty);

                    ttLeft.BeginAnimation(TranslateTransform.XProperty, null);
                    ttRight.BeginAnimation(TranslateTransform.XProperty, null);

                    ttRight.X += xDistance;
                    ttLeft.X += xDistance;

                    if (ttRight.X < 0 && lastX >= 0)
                    {

                        ttLeft.X = right.ActualWidth + ttRight.X;

                        if (lastX == 0)
                        {
                            currentNode = currentNode.getRight();
                        }
                        else
                        {
                            currentNode = currentNode.getRight();
                            currentNode = currentNode.getRight();
                        }
                        Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                        left.Child = currentNode.getFrameworkElement();
                        lastX = -1;
                        lastY = 0;
                    }
                    else
                    {
                        if (ttRight.X > 0 && lastX <= 0)
                        {
                            ttLeft.X = -right.ActualWidth + ttRight.X;
                            if (lastX == 0)
                                currentNode = currentNode.getLeft();
                            else
                            {
                                currentNode = currentNode.getLeft();
                                currentNode = currentNode.getLeft();
                            }
                            Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                            left.Child = currentNode.getFrameworkElement();
                           
                            lastX = 1;
                            lastY = 0;
                        }
                    }
                }
                if (autoDrag > 0 && autoDrag < 1)
                {
                    if (ttRight.X >= right.ActualWidth * autoDrag)
                    {
                        animH(false);
                        return false;
                    }
                    else if (ttRight.X <= -right.ActualWidth * (autoDrag))
                    {
                        animH(true);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
       
        public bool vDrag(int yDistance)
        {
            if (animating == 0)
            {
                
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttAbove = (TranslateTransform)above.RenderTransform;
                if (ttRight.X != 0)
                {

                    return true;
                }
                if (vNeighbourExists())
                {
                    ttAbove.Y = (double)ttAbove.GetValue(TranslateTransform.YProperty);
                    ttRight.Y = (double)ttRight.GetValue(TranslateTransform.YProperty);

                    ttAbove.BeginAnimation(TranslateTransform.YProperty, null);
                    ttRight.BeginAnimation(TranslateTransform.YProperty, null);

                    ttRight.Y += yDistance;
                    ttAbove.Y += yDistance;

                    if (ttRight.Y < 0 && lastY >= 0)
                    {

                        ttAbove.Y = right.ActualHeight + ttRight.Y;

                        if (lastY == 0)
                        {
                            currentNode = currentNode.getBelow();
                        }
                        else
                        {
                            currentNode = currentNode.getBelow();
                            currentNode = currentNode.getBelow();
                        }
                        Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                        above.Child = currentNode.getFrameworkElement();
    
                        lastY = -1;
                        lastX = 0;
                    }
                    else
                    {
                        if (ttRight.Y > 0 && lastY <= 0)
                        {
                            ttAbove.Y = -right.ActualHeight + ttRight.Y;
                            if (lastY == 0)
                                currentNode = currentNode.getAbove();
                            else
                            {
                                currentNode = currentNode.getAbove();
                                currentNode = currentNode.getAbove();
                            }
                            Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                            above.Child = currentNode.getFrameworkElement();
                   
                            lastY = 1;
                            lastX = 0;
                        }
                    }
                }
                if (autoDrag > 0 && autoDrag < 1)
                {
                    if (ttRight.Y >= right.ActualHeight * autoDrag)
                    {
                        animV(false);
                        return false;
                    }
                    else if (ttRight.Y <= -right.ActualHeight * (autoDrag))
                    {
                        animV(true);
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
            if (animating == 0 && hNeighbourExists())
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                if (ttRight.X == 0)
                {
                    if (leftDir)
                    {
                        hDrag(-1);
                    }
                    else
                    {
                        hDrag(1);
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
                lastY = 0;
            }
        }

        public void animV(bool upDir)
        {
            if (animating == 0 && vNeighbourExists())
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttAbove = (TranslateTransform)above.RenderTransform;

                if (ttRight.Y == 0)
                {
                    if (upDir)
                    {
                        vDrag(-1);
                    }
                    else
                    {
                        vDrag(1);
                    }
                }
                animating = 2;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
                doubleAnimationCenter.From = ttRight.Y;
                if (upDir)
                    doubleAnimationCenter.To = -right.ActualHeight;
                else
                    doubleAnimationCenter.To = right.ActualHeight;
                doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                doubleAnimationCenter.Completed += (s, _) => animCompleted();
                ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                doubleAnimationLeft.From = ttAbove.Y;
                doubleAnimationLeft.To = 0;
                doubleAnimationLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                doubleAnimationLeft.Completed += (s, _) => animCompleted();
                ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);

                Border tmp = right;
                right = above;
                above = tmp;
                lastY = 0;
            }
        }

        public void animBack()
        {
            if (animating == 0)
            {
                if (lastX != 0)
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
                    if (lastX < 0)
                    {
                        currentNode = currentNode.getLeft();
                        Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                    }
                    if (lastX > 0)
                    {
                        currentNode = currentNode.getRight();
                        Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                    }
                    lastX = 0;
                    lastY = 0;
                }
                else
                {
                    if (lastY != 0)
                    {
                        TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                        TranslateTransform ttAbove = (TranslateTransform)above.RenderTransform;

                        animating = 2;

                        DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
                        doubleAnimationCenter.From = ttRight.Y;
                        doubleAnimationCenter.To = 0;
                        doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                        doubleAnimationCenter.Completed += (s, _) => animCompleted();
                        ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                        DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                        doubleAnimationLeft.From = ttAbove.Y;
                        if (lastY < 0)
                            doubleAnimationLeft.To = right.ActualHeight;
                        if (lastY > 0)
                            doubleAnimationLeft.To = -right.ActualHeight;
                        doubleAnimationLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
                        doubleAnimationLeft.Completed += (s, _) => animCompleted();
                        ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);
                        if (lastY < 0)
                        {
                            currentNode = currentNode.getAbove();
                            Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                        }
                        if (lastY > 0)
                        {
                            currentNode = currentNode.getBelow();
                            Debug.WriteLine(((Button)((Grid)currentNode.getFrameworkElement()).Children[1]).Content);
                        }
                        lastY = 0;
                        lastX = 0;
                    }

                }
            }

        }
    }
}
