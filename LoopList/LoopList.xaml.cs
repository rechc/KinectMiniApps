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
        private List<FrameworkElement> controlsList = new List<FrameworkElement>();
        private Border left, right;
        private int index;
        private int dragging;
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

        public void add(FrameworkElement control)
        {
            right.Child = control;
            controlsList.Add(control);
            nextIndex();
        }

        public bool drag(int xDistance)
        {
            if (dragging == 0 && controlsList.Count > 1)
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
                    Debug.WriteLine(ttRight.X);
                    ttLeft.X = right.ActualWidth + ttRight.X;

                    if (lastX == 0)
                        nextIndex();
                    else
                    {
                        nextIndex();
                        nextIndex();
                    }
                    left.Child = null;
                    left.Child = controlsList[index];
                    lastX = -1;
                }
                else
                {
                    if (ttRight.X > 0 && lastX <= 0)
                    {
                        ttLeft.X = -right.ActualWidth + ttRight.X;
                        if (lastX == 0)
                            previousIndex();
                        else
                        {
                            previousIndex();
                            previousIndex();
                        }
                        left.Child = controlsList[index];
                        lastX = 1;
                    }
                }
                if (autoDrag > 0 && autoDrag < 1)
                {
                    if (ttRight.X >= right.ActualWidth * autoDrag)
                    {
                        anim(false);
                        return false;
                    } else if (ttRight.X <= -right.ActualWidth*(autoDrag))
                    {
                        anim(true);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        void animCompleted()
        {
            dragging--;
        }

        public void anim(bool leftDir)
        {
            if (dragging == 0 && controlsList.Count > 1)
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
                dragging = 2;

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

        private void nextIndex()
        {
            index++;
            if (index == controlsList.Count)
            {
                index = 0;
            }
        }

        private void previousIndex()
        {
            index--;
            if (index == -1)
            {
                index = controlsList.Count - 1;
            }
        }


    }
}
