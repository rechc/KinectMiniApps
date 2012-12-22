using System;
using System.Collections.Generic;
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
        private Image left, center, right;
        private int centerIndex;

        public LoopList()
        {
            InitializeComponent();
            
            left = new Image();
            left.Stretch = Stretch.Fill;
            left.RenderTransform = new TranslateTransform();

            center = new Image();
            center.Stretch = Stretch.Fill;
            center.RenderTransform = new TranslateTransform();

            right = new Image();
            right.Stretch = Stretch.Fill;
            right.RenderTransform = new TranslateTransform();

            rootGrid.Children.Add(left);
            rootGrid.Children.Add(center);
            rootGrid.Children.Add(right);

            Loaded += delegate
            {
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;
                ttLeft.X = -left.ActualWidth;
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                ttRight.X = right.ActualWidth;
            };
        }

        public void add(string path)
        {
            if (pathList.Count == 0)
            {
                center.Source = loadData(path);
                left.Source = center.Source;
                right.Source = center.Source;
            }
            left.Source = loadData(path);
            if (pathList.Count == 1)
            {
                
                right.Source = left.Source;
            }
            
            pathList.Add(path);
        }

        public void leftAnim()
        {
            TranslateTransform ttCenter = (TranslateTransform)center.RenderTransform;
            TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
            TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

            DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
            doubleAnimationCenter.From = ttCenter.X;
            doubleAnimationCenter.To = ttCenter.X - center.ActualWidth;
            doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            ttCenter.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

            DoubleAnimation doubleAnimationRight = new DoubleAnimation();
            doubleAnimationRight.From = ttRight.X;
            doubleAnimationRight.To = ttRight.X - right.ActualWidth;
            doubleAnimationRight.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationRight);

            ttLeft.BeginAnimation(TranslateTransform.XProperty, null);
            ttLeft.X = left.ActualWidth;

            Image tmp = left;
            left = center;
            center = right;
            right = tmp;
            centerIndex = nextIndex(centerIndex);
            int indexForRight = nextIndex(centerIndex);
            right.Source = loadData(pathList[indexForRight]);    
        }


        public void rightAnim()
        {
            TranslateTransform ttCenter = (TranslateTransform)center.RenderTransform;
            TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
            TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

            DoubleAnimation doubleAnimationCenter = new DoubleAnimation();
            doubleAnimationCenter.From = ttCenter.X;
            doubleAnimationCenter.To = ttCenter.X + center.ActualWidth;
            doubleAnimationCenter.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            ttCenter.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

            DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
            doubleAnimationLeft.From = ttLeft.X;
            doubleAnimationLeft.To = ttLeft.X + left.ActualWidth;
            doubleAnimationLeft.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);

            ttRight.BeginAnimation(TranslateTransform.XProperty, null);
            ttRight.X = -right.ActualWidth;

            Image tmp = right;
            right = center;
            center = left;
            left = tmp;
            centerIndex = previousIndex(centerIndex);
            int indexForLeft = previousIndex(centerIndex);
            left.Source = loadData(pathList[indexForLeft]);
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
