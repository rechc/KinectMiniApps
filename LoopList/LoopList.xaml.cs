using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoopList
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class LoopList : UserControl
    {
        private List<String> pathList = new List<String>();
        private Border left, center, right;
        private int centerIndex;
        private int dragging;

        public LoopList()
        {
            InitializeComponent();

            left = new Border();
            left.BorderThickness = new Thickness(10);
            left.RenderTransform = new TranslateTransform();
            left.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            center = new Border();
            center.BorderThickness = new Thickness(10);
            center.RenderTransform = new TranslateTransform();
            center.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            
            right = new Border();
            right.BorderThickness = new Thickness(10);
            right.RenderTransform = new TranslateTransform();
            right.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            Image leftImg = new Image();
            leftImg.Stretch = Stretch.Fill;

            Image centerImg = new Image();
            centerImg.Stretch = Stretch.Fill;

            Image rightImg = new Image();
            rightImg.Stretch = Stretch.Fill;

            left.Child = leftImg;
            center.Child = centerImg;
            right.Child = rightImg;


            Loaded += delegate
            {
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;
                ttLeft.X = -left.ActualWidth + center.ActualWidth * 0.1;

                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                ttRight.X = right.ActualWidth - center.ActualWidth * 0.1;

                rootGrid.Margin = new Thickness(center.ActualWidth * 0.05, 0, center.ActualWidth * 0.05, 0);
            };


            rootGrid.Children.Add(left);
            rootGrid.Children.Add(center);
            rootGrid.Children.Add(right);
        }

        public void add(string path)
        {
            if (pathList.Count == 0)
            {
                ((Image)center.Child).Source = loadData(path);
                ((Image)left.Child).Source = ((Image)center.Child).Source;
                ((Image)right.Child).Source = ((Image)center.Child).Source;
                
            }
            ((Image)left.Child).Source = loadData(path);
            if (pathList.Count == 1)
            {

                ((Image)right.Child).Source = ((Image)left.Child).Source;
            }
            
            pathList.Add(path);
        }

        public bool drag(int xDistance)
        {
            if (dragging == 0)
            {
                TranslateTransform ttCenter = (TranslateTransform)center.RenderTransform;
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                ttCenter.X += xDistance;
                ttRight.X += xDistance;
                ttLeft.X += xDistance;

                if (ttCenter.X > center.ActualWidth * 0.4)
                {
                    rightAnim();
                    return false;
                }
                if ((ttCenter.X + center.ActualWidth) < center.ActualWidth*0.6)
                {
                    leftAnim();
                    return false;
                }
                return true;
            }
            return false;
        }

  

        public void leftAnim()
        {
            if (dragging == 0)
            {
                dragging = 2;
                Grid.SetZIndex(left, 0);
                Grid.SetZIndex(center, 1);
                Grid.SetZIndex(right, 1);

                TranslateTransform ttCenter = (TranslateTransform)center.RenderTransform;
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
                doubleAnimationCenter.From = ttCenter.X;
                doubleAnimationCenter.To = -center.ActualWidth;
                doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 150));
                doubleAnimationCenter.Completed += (s, _) => FadeAnimationCompleted(ttCenter, doubleAnimationCenter.To);
                ttCenter.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationRight = new DoubleAnimation();
                doubleAnimationRight.From = ttRight.X;
                doubleAnimationRight.To = 0;
                doubleAnimationRight.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 150));
                doubleAnimationRight.Completed += (s, _) => FadeAnimationCompleted(ttRight, doubleAnimationRight.To);
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationRight);

                ttLeft.X = left.ActualWidth;

                Border tmp = left;
                left = center;
                center = right;
                right = tmp;
                centerIndex = nextIndex(centerIndex);
                int indexForRight = nextIndex(centerIndex);
                ((Image)right.Child).Source = loadData(pathList[indexForRight]);
            }
        }

        void FadeAnimationCompleted(TranslateTransform element, double? to)
        {
            dragging--;
            element.BeginAnimation(TranslateTransform.XProperty, null);
            element.X = to.Value;
        }

        public void rightAnim()
        {
            if (dragging == 0)
            {
                dragging = 2;
                Grid.SetZIndex(right, 0);
                Grid.SetZIndex(center, 1);
                Grid.SetZIndex(left, 1);
                TranslateTransform ttCenter = (TranslateTransform)center.RenderTransform;
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
                doubleAnimationCenter.From = ttCenter.X;
                doubleAnimationCenter.To = center.ActualWidth;
                doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 150));
                doubleAnimationCenter.Completed += (s, _) => FadeAnimationCompleted(ttCenter, doubleAnimationCenter.To);
                ttCenter.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                doubleAnimationLeft.From = ttLeft.X;
                doubleAnimationLeft.To = 0;
                doubleAnimationLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 150));
                doubleAnimationLeft.Completed += (s, _) => FadeAnimationCompleted(ttLeft, doubleAnimationLeft.To);
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);

                ttRight.X = -right.ActualWidth;

                Border tmp = right;
                right = center;
                center = left;
                left = tmp;
                centerIndex = previousIndex(centerIndex);
                int indexForLeft = previousIndex(centerIndex);
                ((Image)left.Child).Source = loadData(pathList[indexForLeft]);
            }
        }

        private int nextIndex(int index)
        {
            index++;
            if (index == pathList.Count)
            {
                index = 0;
            }
            return index;
        }

        private int previousIndex(int index)
        {
            index--;
            if (index == -1)
            {
                index = pathList.Count - 1;
            }
            return index;
        }

        private BitmapImage loadData(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }
    }
}
