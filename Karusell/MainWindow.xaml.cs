using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Image> viewList = new List<Image>();
        private CircleList<BitmapImage> dataList = new CircleList<BitmapImage>();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += delegate
            {
                TransformGroup tg = (TransformGroup)LeftImage.RenderTransform;
                TranslateTransform tt = (TranslateTransform)tg.Children[0];
                tt.X = -LeftImage.ActualWidth;
                tg = (TransformGroup)RightImage.RenderTransform;
                tt = (TranslateTransform)tg.Children[0];
                tt.X = LeftImage.ActualWidth;
            };

            viewList.Add(LeftImage);
            viewList.Add(CenterImage);
            viewList.Add(RightImage);

            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images");
            foreach (string path in paths) {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bi.EndInit();
                dataList.add(bi);
            }

            for (int i = 0; i < viewList.Count; i++)
            {
                Image img = viewList[i];
                BitmapImage bi = dataList.getNext(i);

                img.Source = bi;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                Image Image = viewList[0];
                TransformGroup tg;
                TranslateTransform tt;
                for (int i = 1; i < 3; i++)
                {
                    viewList[i - 1] = viewList[i];
                    Image b = viewList[i];
                    tg = (TransformGroup)b.RenderTransform;
                    tt = (TranslateTransform)tg.Children[0];

                    DoubleAnimation doubleAnimation = new DoubleAnimation();
                    doubleAnimation.From = tt.X;
                    doubleAnimation.To = tt.X - Image.ActualWidth;
                    doubleAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
                    tt.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);

                }
                viewList[2] = Image;
                tg = (TransformGroup)Image.RenderTransform;
                tt = (TranslateTransform)tg.Children[0];
                tt.BeginAnimation(TranslateTransform.XProperty, null);
                tt.X = Image.ActualWidth;
                viewList[2].Source = dataList.getNext(2);
            }
            else if (e.Key == Key.Right)
            {
                Image Image = viewList[2];
                TransformGroup tg;
                TranslateTransform tt;
                for (int i = 2; i > 0; i--)
                {
                    viewList[i] = viewList[i - 1];
                    Image b = viewList[i];
                    tg = (TransformGroup)b.RenderTransform;
                    tt = (TranslateTransform)tg.Children[0];

                    DoubleAnimation doubleAnimation = new DoubleAnimation();
                    doubleAnimation.From = tt.X;
                    doubleAnimation.To = tt.X + Image.ActualWidth;
                    doubleAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
                    tt.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);

                }
                viewList[0] = Image;
                tg = (TransformGroup)Image.RenderTransform;
                tt = (TranslateTransform)tg.Children[0];
                tt.BeginAnimation(TranslateTransform.XProperty, null);
                tt.X = -Image.ActualWidth;
                viewList[0].Source = dataList.getPrevious();
            }

        }

    }
}
