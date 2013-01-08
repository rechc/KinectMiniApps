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
        private Grid left, right, above;
        private Node currentNode;
        private int animating;
        private int lastX, lastY;
        private double autoDrag;
        private Duration duration;

        public LoopList()
        {
            InitializeComponent();

            duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));

            left = new Grid();
            left.RowDefinitions.Add(new RowDefinition());
            left.RowDefinitions.Add(new RowDefinition());
            left.RowDefinitions.Add(new RowDefinition());
            left.RowDefinitions.Add(new RowDefinition());
            left.RowDefinitions.Add(new RowDefinition());

            left.ColumnDefinitions.Add(new ColumnDefinition());
            left.ColumnDefinitions.Add(new ColumnDefinition());
            left.ColumnDefinitions.Add(new ColumnDefinition());
            left.ColumnDefinitions.Add(new ColumnDefinition());
            left.ColumnDefinitions.Add(new ColumnDefinition());

            left.RowDefinitions[0].Height = new GridLength(10, GridUnitType.Pixel);
            left.ColumnDefinitions[0].Width = new GridLength(10, GridUnitType.Pixel);
            left.RowDefinitions[1].Height = new GridLength(10, GridUnitType.Pixel);
            left.ColumnDefinitions[1].Width = new GridLength(10, GridUnitType.Pixel);
            left.RowDefinitions[3].Height = new GridLength(10, GridUnitType.Pixel);
            left.ColumnDefinitions[3].Width = new GridLength(10, GridUnitType.Pixel);
            left.RowDefinitions[4].Height = new GridLength(10, GridUnitType.Pixel);
            left.ColumnDefinitions[4].Width = new GridLength(10, GridUnitType.Pixel);

            left.RenderTransform = new TranslateTransform();

            Rectangle leftBorderRect = new Rectangle();
            Rectangle rightBorderRect = new Rectangle();
            Rectangle topBorderRect = new Rectangle();
            Rectangle bottomBorderRect = new Rectangle();

            leftBorderRect.Fill = new SolidColorBrush(Colors.Red);
            rightBorderRect.Fill = new SolidColorBrush(Colors.Red);
            topBorderRect.Fill = new SolidColorBrush(Colors.Red);
            bottomBorderRect.Fill = new SolidColorBrush(Colors.Red);

            leftBorderRect.Visibility = Visibility.Collapsed;
            rightBorderRect.Visibility = Visibility.Collapsed;
            topBorderRect.Visibility = Visibility.Collapsed;
            bottomBorderRect.Visibility = Visibility.Collapsed;
            
            Polygon hPolygon = new Polygon();
            PointCollection polygonPoints = new PointCollection();
            polygonPoints.Add(new Point(20, 0));
            polygonPoints.Add(new Point(20, 300));
            polygonPoints.Add(new Point(0, 150));
            polygonPoints.Add(new Point(20, 0));
            hPolygon.Points = polygonPoints;
            hPolygon.Fill = new SolidColorBrush(Colors.DarkBlue);


            
            Viewbox leftDirViewbox = new Viewbox();
            leftDirViewbox.Margin = new Thickness(0, 0, 2, 0);
            leftDirViewbox.Stretch = Stretch.Fill;
            leftDirViewbox.Child = hPolygon;

            Viewbox rightDirViewbox = new Viewbox();
            rightDirViewbox.Margin = new Thickness(2, 0, 0, 0);
            hPolygon = (Polygon)cloneElement(hPolygon);
            hPolygon.RenderTransform = new RotateTransform(180, 10, 150);
            rightDirViewbox.Stretch = Stretch.Fill;
            rightDirViewbox.Child = hPolygon;

            Polygon vPolygon = new Polygon();
            polygonPoints = new PointCollection();
            polygonPoints.Add(new Point(0, 20));
            polygonPoints.Add(new Point(300, 20));
            polygonPoints.Add(new Point(150, 0));
            polygonPoints.Add(new Point(0, 20));
            vPolygon.Points = polygonPoints;
            vPolygon.Fill = new SolidColorBrush(Colors.DarkBlue);

            Viewbox topDirViewbox = new Viewbox();
            topDirViewbox.Margin = new Thickness(0, 0, 0, 2);
            topDirViewbox.Child = vPolygon;
            topDirViewbox.Stretch = Stretch.Fill;

            Viewbox bottomDirViewbox = new Viewbox();
            bottomDirViewbox.Margin = new Thickness(0, 2, 0, 0);
            vPolygon = (Polygon)cloneElement(vPolygon);
            vPolygon.RenderTransform = new RotateTransform(180, 150, 10);
            bottomDirViewbox.Stretch = Stretch.Fill;
            bottomDirViewbox.Child = vPolygon;

            Grid.SetRowSpan(leftBorderRect, 5);

            Grid.SetColumnSpan(topBorderRect, 3);
            Grid.SetColumn(topBorderRect, 1);

            Grid.SetColumnSpan(bottomBorderRect, 3);
            Grid.SetColumn(bottomBorderRect, 1);
            Grid.SetRow(bottomBorderRect, 4);

            Grid.SetRowSpan(rightBorderRect, 5);
            Grid.SetColumn(rightBorderRect, 4);

            Grid.SetColumn(leftDirViewbox, 1);
            Grid.SetRow(leftDirViewbox, 2);

            Grid.SetRow(topDirViewbox, 1);
            Grid.SetColumn(topDirViewbox, 2);

            Grid.SetRow(bottomDirViewbox, 3);
            Grid.SetColumn(bottomDirViewbox, 2);

            Grid.SetRow(rightDirViewbox, 2);
            Grid.SetColumn(rightDirViewbox, 3);

            left.Children.Add(leftBorderRect);
            left.Children.Add(topBorderRect);
            left.Children.Add(bottomBorderRect);
            left.Children.Add(rightBorderRect);

            left.Children.Add(leftDirViewbox);
            left.Children.Add(topDirViewbox);
            left.Children.Add(bottomDirViewbox);
            left.Children.Add(rightDirViewbox);

            right = (Grid)cloneElement(left);
            above = (Grid)cloneElement(left);

            rootGrid.Children.Add(left);
            rootGrid.Children.Add(right);
            rootGrid.Children.Add(above);

            

            Loaded += LoopList_Loaded;

        }

        private BitmapImage loadImage(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }

        private UIElement cloneElement(UIElement orig)
        {

            if (orig == null)

                return (null);

            string s = XamlWriter.Save(orig);

            StringReader stringReader = new StringReader(s);

            XmlReader xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());

            return (UIElement)XamlReader.Load(xmlReader);

        }

        void LoopList_Loaded(object sender, RoutedEventArgs e)
        {
            TranslateTransform ttAbove = (TranslateTransform)above.RenderTransform;
            TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;

            ttAbove.Y = -right.ActualHeight*4;
            ttLeft.X = -right.ActualWidth*4;
        }

        public void setDuration(Duration duration)
        {
            this.duration = duration;
        }

        public void setAutoDragOffset(double autoDrag)
        {
            this.autoDrag = autoDrag;
        }

        private void markDirections(Grid grid)
        {
            if (hNeighbourExists())
            {
                grid.Children[4].Visibility = System.Windows.Visibility.Visible;
                grid.Children[7].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                grid.Children[4].Visibility = System.Windows.Visibility.Collapsed;
                grid.Children[7].Visibility = System.Windows.Visibility.Collapsed;
            }
            if (vNeighbourExists())
            {
                grid.Children[5].Visibility = System.Windows.Visibility.Visible;
                grid.Children[6].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                grid.Children[5].Visibility = System.Windows.Visibility.Collapsed;
                grid.Children[6].Visibility = System.Windows.Visibility.Collapsed;
            }
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
            setChild(right, currentNode.getFrameworkElement());
            
            return currentNode;
        }

        private void setChild(Grid grid, FrameworkElement frameworkElement)
        {
            Grid.SetRow(frameworkElement, 2);
            Grid.SetColumn(frameworkElement, 2);

            if (grid.Children.Count == 9)
            {
                grid.Children.RemoveAt(8);
            }
            grid.Children.Add(frameworkElement);
            markDirections(grid);
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
            setChild(right, currentNode.getFrameworkElement());

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
            setChild(right, currentNode.getFrameworkElement());
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
            setChild(right, currentNode.getFrameworkElement());
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

        private void markCentered()
        {

            right.Children[0].Visibility = Visibility.Visible;
            right.Children[1].Visibility = Visibility.Visible;
            right.Children[2].Visibility = Visibility.Visible;
            right.Children[3].Visibility = Visibility.Visible;
        }

        private void unmarkCentered()
        {
            right.Children[0].Visibility = Visibility.Collapsed;
            right.Children[1].Visibility = Visibility.Collapsed;
            right.Children[2].Visibility = Visibility.Collapsed;
            right.Children[3].Visibility = Visibility.Collapsed;
        }

        public bool hDrag(int xDistance)
        {
            if (animating == 0)
            {
                TranslateTransform ttRight = (TranslateTransform)right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)left.RenderTransform;
                if (ttRight.Y != 0) return true; // diagonales scrollen gibts nicht
                if (hNeighbourExists())
                {
                    ttLeft.X = (double)ttLeft.GetValue(TranslateTransform.XProperty);
                    ttRight.X = (double)ttRight.GetValue(TranslateTransform.XProperty);

                    ttLeft.BeginAnimation(TranslateTransform.XProperty, null);
                    ttRight.BeginAnimation(TranslateTransform.XProperty, null);

                    ttRight.X += xDistance;
                    ttLeft.X += xDistance;


                    if (ttRight.X >= right.ActualWidth || ttRight.X <= -right.ActualWidth)
                    {
                        Grid tmp = right;
                        right = left;
                        left = tmp;
                        ttRight = (TranslateTransform)right.RenderTransform;
                        ttLeft = (TranslateTransform)left.RenderTransform;
                        lastX = 0;
                    }

                    if (ttRight.X == 0)
                    {
                        markCentered();
                    }
                    else
                    {
                        unmarkCentered();
                    }

                    if (ttRight.X >= 0 && lastX < 0)
                    {
                        currentNode = currentNode.getLeft();
                        lastX = 0;
                    }
                    if (ttRight.X <= 0 && lastX > 0)
                    {
                        currentNode = currentNode.getRight();
                        lastX = 0;
                    }
                    if (ttRight.X < 0 && lastX == 0)
                    {
                        ttLeft.X = right.ActualWidth + ttRight.X;
                        currentNode = currentNode.getRight();
                        setChild(left, currentNode.getFrameworkElement());
                        lastX = -1;
                    }
                    else
                    {
                        if (ttRight.X > 0 && lastX == 0)
                        {
                            ttLeft.X = -right.ActualWidth + ttRight.X;
                            currentNode = currentNode.getLeft();
                            setChild(left, currentNode.getFrameworkElement());
                            lastX = 1;
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

                    if (ttRight.Y >= right.ActualHeight || ttRight.Y <= -right.ActualHeight)
                    {
                        Grid tmp = right;
                        right = above;
                        above = tmp;
                        ttRight = (TranslateTransform)right.RenderTransform;
                        ttAbove = (TranslateTransform)above.RenderTransform;
                        lastY = 0;
                    }

                    if (ttRight.Y == 0)
                    {
                        markCentered();
                    }
                    else
                    {
                        unmarkCentered();
                    }

                    if (ttRight.Y >= 0 && lastY < 0)
                    {
                        currentNode = currentNode.getAbove();
                        lastY = 0;
                    }
                    if (ttRight.Y <= 0 && lastY > 0)
                    {
                        currentNode = currentNode.getBelow();
                        lastY = 0;
                    }

                    if (ttRight.Y < 0 && lastY == 0)
                    {
                        ttAbove.Y = right.ActualHeight + ttRight.Y;
                        currentNode = currentNode.getBelow();
                        setChild(above, currentNode.getFrameworkElement());
                        lastY = -1;
                    }
                    else
                    {
                        if (ttRight.Y > 0 && lastY == 0)
                        {
                            ttAbove.Y = -right.ActualHeight + ttRight.Y;
                            currentNode = currentNode.getAbove();
                            setChild(above, currentNode.getFrameworkElement());
                            lastY = 1;
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
                doubleAnimationCenter.Duration = duration;
                doubleAnimationCenter.Completed += (s, _) => animCompleted();
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                doubleAnimationLeft.From = ttLeft.X;
                doubleAnimationLeft.To = 0;
                doubleAnimationLeft.Duration = duration;
                doubleAnimationLeft.Completed += (s, _) => animCompleted();
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);

                Grid tmp = right;
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
                doubleAnimationCenter.Duration = duration;
                doubleAnimationCenter.Completed += (s, _) => animCompleted();
                ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                doubleAnimationLeft.From = ttAbove.Y;
                doubleAnimationLeft.To = 0;
                doubleAnimationLeft.Duration = duration;
                doubleAnimationLeft.Completed += (s, _) => animCompleted();
                ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);

                Grid tmp = right;
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
                    doubleAnimationCenter.Duration = duration;
                    doubleAnimationCenter.Completed += (s, _) => animCompleted();
                    ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                    DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                    doubleAnimationLeft.From = ttLeft.X;
                    if (lastX < 0)
                        doubleAnimationLeft.To = right.ActualWidth;
                    if (lastX > 0)
                        doubleAnimationLeft.To = -right.ActualWidth;
                    doubleAnimationLeft.Duration = duration;
                    doubleAnimationLeft.Completed += (s, _) => animCompleted();
                    ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);
                    if (lastX < 0)
                    {
                        currentNode = currentNode.getLeft();
                    }
                    if (lastX > 0)
                    {
                        currentNode = currentNode.getRight();
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
                        doubleAnimationCenter.Duration = duration;
                        doubleAnimationCenter.Completed += (s, _) => animCompleted();
                        ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                        DoubleAnimation doubleAnimationLeft = new DoubleAnimation();
                        doubleAnimationLeft.From = ttAbove.Y;
                        if (lastY < 0)
                            doubleAnimationLeft.To = right.ActualHeight;
                        if (lastY > 0)
                            doubleAnimationLeft.To = -right.ActualHeight;
                        doubleAnimationLeft.Duration = duration;
                        doubleAnimationLeft.Completed += (s, _) => animCompleted();
                        ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);
                        if (lastY < 0)
                        {
                            currentNode = currentNode.getAbove();
                        }
                        if (lastY > 0)
                        {
                            currentNode = currentNode.getBelow();
                        }
                        lastY = 0;
                        lastX = 0;
                    }

                }
            }
        }
    }
}
