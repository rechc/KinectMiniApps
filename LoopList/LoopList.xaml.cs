using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;


namespace LoopList
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class LoopList : UserControl
    {
        private Grid _left, _right, _above;
        private Node _currentNode;
        private int _animating;
        private int _lastX, _lastY;
        private double _autoDrag;
        private Duration _duration;

        public LoopList()
        {
            InitializeComponent();

            _duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));

            _left = new Grid();
            _left.RowDefinitions.Add(new RowDefinition());
            _left.RowDefinitions.Add(new RowDefinition());
            _left.RowDefinitions.Add(new RowDefinition());
            _left.RowDefinitions.Add(new RowDefinition());
            _left.RowDefinitions.Add(new RowDefinition());

            _left.ColumnDefinitions.Add(new ColumnDefinition());
            _left.ColumnDefinitions.Add(new ColumnDefinition());
            _left.ColumnDefinitions.Add(new ColumnDefinition());
            _left.ColumnDefinitions.Add(new ColumnDefinition());
            _left.ColumnDefinitions.Add(new ColumnDefinition());

            _left.RowDefinitions[0].Height = new GridLength(10, GridUnitType.Pixel);
            _left.ColumnDefinitions[0].Width = new GridLength(10, GridUnitType.Pixel);
            _left.RowDefinitions[1].Height = new GridLength(10, GridUnitType.Pixel);
            _left.ColumnDefinitions[1].Width = new GridLength(10, GridUnitType.Pixel);
            _left.RowDefinitions[3].Height = new GridLength(10, GridUnitType.Pixel);
            _left.ColumnDefinitions[3].Width = new GridLength(10, GridUnitType.Pixel);
            _left.RowDefinitions[4].Height = new GridLength(10, GridUnitType.Pixel);
            _left.ColumnDefinitions[4].Width = new GridLength(10, GridUnitType.Pixel);

            _left.RenderTransform = new TranslateTransform();

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
            PointCollection polygonPoints = new PointCollection
                {
                    new Point(20, 0),
                    new Point(20, 300),
                    new Point(0, 150),
                    new Point(20, 0)
                };
            hPolygon.Points = polygonPoints;
            hPolygon.Fill = new SolidColorBrush(Colors.DarkBlue);


            
            Viewbox leftDirViewbox = new Viewbox
                {
                    Margin = new Thickness(0, 0, 2, 0),
                    Stretch = Stretch.Fill,
                    Child = hPolygon
                };

            Viewbox rightDirViewbox = new Viewbox {Margin = new Thickness(2, 0, 0, 0)};
            hPolygon = (Polygon)CloneElement(hPolygon);
            hPolygon.RenderTransform = new RotateTransform(180, 10, 150);
            rightDirViewbox.Stretch = Stretch.Fill;
            rightDirViewbox.Child = hPolygon;

            Polygon vPolygon = new Polygon();
            polygonPoints = new PointCollection
                {
                    new Point(0, 20),
                    new Point(300, 20),
                    new Point(150, 0),
                    new Point(0, 20)
                };
            vPolygon.Points = polygonPoints;
            vPolygon.Fill = new SolidColorBrush(Colors.DarkBlue);

            Viewbox topDirViewbox = new Viewbox
                {
                    Margin = new Thickness(0, 0, 0, 2),
                    Child = vPolygon,
                    Stretch = Stretch.Fill
                };

            Viewbox bottomDirViewbox = new Viewbox {Margin = new Thickness(0, 2, 0, 0)};
            vPolygon = (Polygon)CloneElement(vPolygon);
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

            _left.Children.Add(leftBorderRect);
            _left.Children.Add(topBorderRect);
            _left.Children.Add(bottomBorderRect);
            _left.Children.Add(rightBorderRect);

            _left.Children.Add(leftDirViewbox);
            _left.Children.Add(topDirViewbox);
            _left.Children.Add(bottomDirViewbox);
            _left.Children.Add(rightDirViewbox);

            _right = (Grid)CloneElement(_left);
            _above = (Grid)CloneElement(_left);

            rootGrid.Children.Add(_left);
            rootGrid.Children.Add(_right);
            rootGrid.Children.Add(_above);

            Loaded += LoopList_Loaded;

        }


        private static UIElement CloneElement(UIElement orig)
        {

            if (orig == null)

                return (null);

            string s = XamlWriter.Save(orig);

            StringReader stringReader = new StringReader(s);

            XmlReader xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings());

            return (UIElement)XamlReader.Load(xmlReader);

        }

        void LoopList_Loaded(object sender, RoutedEventArgs e)
        {
            TranslateTransform ttAbove = (TranslateTransform)_above.RenderTransform;
            TranslateTransform ttLeft = (TranslateTransform)_left.RenderTransform;

            ttAbove.Y = -_right.ActualHeight*4;
            ttLeft.X = -_right.ActualWidth*4;
        }

        public void SetDuration(Duration duration)
        {
            _duration = duration;
        }

        public void SetAutoDragOffset(double autoDrag)
        {
            _autoDrag = autoDrag;
        }

        private void MarkDirections(Grid grid)
        {
            if (HNeighbourExists())
            {
                grid.Children[4].Visibility = Visibility.Visible;
                grid.Children[7].Visibility = Visibility.Visible;
            }
            else
            {
                grid.Children[4].Visibility = Visibility.Collapsed;
                grid.Children[7].Visibility = Visibility.Collapsed;
            }
            if (VNeighbourExists())
            {
                grid.Children[5].Visibility = Visibility.Visible;
                grid.Children[6].Visibility = Visibility.Visible;
            }
            else
            {
                grid.Children[5].Visibility = Visibility.Collapsed;
                grid.Children[6].Visibility = Visibility.Collapsed;
            }
        }

        public Node AddToLeft(Node anchor, FrameworkElement frameworkElement)
        {
            _currentNode = anchor;
            if (_currentNode == null)
            {
                _currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!_currentNode.isMarkedLeft())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = _currentNode.getLeft();
                _currentNode.setLeft(newNode);
                _currentNode.unmarkLeft();
                newNode.setRight(_currentNode);
                newNode.unmarkRight();
                newNode.setLeft(first);
                first.setRight(newNode);
                _currentNode = newNode;
            }
            SetChild(_right, _currentNode.getFrameworkElement());
            
            return _currentNode;
        }

        private void SetChild(Grid grid, FrameworkElement frameworkElement)
        {
            Grid.SetRow(frameworkElement, 2);
            Grid.SetColumn(frameworkElement, 2);

            if (grid.Children.Count == 9)
            {
                grid.Children.RemoveAt(8);
            }
            grid.Children.Add(frameworkElement);
            MarkDirections(grid);
        }

        public Node AddToRight(Node anchor, FrameworkElement frameworkElement)
        {
            _currentNode = anchor;
            if (_currentNode == null)
            {
                _currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!_currentNode.isMarkedRight())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = _currentNode.getRight();
                _currentNode.setRight(newNode);
                _currentNode.unmarkRight();
                newNode.setLeft(_currentNode);
                newNode.unmarkLeft();
                newNode.setRight(first);
                first.setLeft(newNode);
                _currentNode = newNode;
            }
            SetChild(_right, _currentNode.getFrameworkElement());

            return _currentNode;
        }

        public Node AddToAbove(Node anchor, FrameworkElement frameworkElement)
        {
            _currentNode = anchor;
            if (_currentNode == null)
            {
                _currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!_currentNode.isMarkedAbove())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = _currentNode.getAbove();
                _currentNode.setAbove(newNode);
                _currentNode.unmarkAbove();
                newNode.setBelow(_currentNode);
                newNode.unmarkBelow();
                newNode.setAbove(first);
                first.setBelow(newNode);
                _currentNode = newNode;
            }
            SetChild(_right, _currentNode.getFrameworkElement());
            return _currentNode;
        }

        public Node AddToBelow(Node anchor, FrameworkElement frameworkElement)
        {
            _currentNode = anchor;
            if (_currentNode == null)
            {
                _currentNode = new Node(frameworkElement);
            }
            else
            {
                if (!_currentNode.isMarkedBelow())
                {
                    throw new Exception("why you no add to marked anchor???");
                }
                Node newNode = new Node(frameworkElement);
                Node first = _currentNode.getBelow();
                _currentNode.setBelow(newNode);
                _currentNode.unmarkBelow();
                newNode.setAbove(_currentNode);
                newNode.unmarkAbove();
                newNode.setBelow(first);
                first.setAbove(newNode);
                _currentNode = newNode;
            }
            SetChild(_right, _currentNode.getFrameworkElement());
            return _currentNode;
        }

        private bool HNeighbourExists()
        {
            if (!_currentNode.isMarkedLeft() || !_currentNode.isMarkedRight())
            {
                return true;
            }
            return false;
        }

        private bool VNeighbourExists()
        {
            if (!_currentNode.isMarkedBelow() || !_currentNode.isMarkedAbove())
            {
                return true;
            }
            return false;
        }

        private void MarkCentered()
        {
            for (int i = 0; i < 4; i++)
            {
                _right.Children[i].Visibility = Visibility.Visible;
            }
        }

        private void UnmarkCentered()
        {
            for (int i = 0; i < 4; i++)
            {
                _right.Children[i].Visibility = Visibility.Collapsed;
            }
        }

        public bool HDrag(int xDistance)
        {
            if (_animating == 0)
            {
                TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)_left.RenderTransform;
                if (Math.Abs(ttRight.Y - 0) > 0.000000001) return true; // diagonales scrollen gibts nicht
                if (HNeighbourExists())
                {
                    ttLeft.X = (double)ttLeft.GetValue(TranslateTransform.XProperty);
                    ttRight.X = (double)ttRight.GetValue(TranslateTransform.XProperty);

                    ttLeft.BeginAnimation(TranslateTransform.XProperty, null);
                    ttRight.BeginAnimation(TranslateTransform.XProperty, null);

                    ttRight.X += xDistance;
                    ttLeft.X += xDistance;


                    if (ttRight.X >= _right.ActualWidth || ttRight.X <= -_right.ActualWidth)
                    {
                        Grid tmp = _right;
                        _right = _left;
                        _left = tmp;
                        ttRight = (TranslateTransform)_right.RenderTransform;
                        ttLeft = (TranslateTransform)_left.RenderTransform;
                        _lastX = 0;
                    }

                    if (Math.Abs(ttRight.X - 0) < 0.0000001)
                    {
                        MarkCentered();
                    }
                    else
                    {
                        UnmarkCentered();
                    }

                    if (ttRight.X >= 0 && _lastX < 0)
                    {
                        _currentNode = _currentNode.getLeft();
                        _lastX = 0;
                    }
                    if (ttRight.X <= 0 && _lastX > 0)
                    {
                        _currentNode = _currentNode.getRight();
                        _lastX = 0;
                    }
                    if (ttRight.X < 0 && _lastX == 0)
                    {
                        ttLeft.X = _right.ActualWidth + ttRight.X;
                        _currentNode = _currentNode.getRight();
                        SetChild(_left, _currentNode.getFrameworkElement());
                        _lastX = -1;
                    }
                    else
                    {
                        if (ttRight.X > 0 && _lastX == 0)
                        {
                            ttLeft.X = -_right.ActualWidth + ttRight.X;
                            _currentNode = _currentNode.getLeft();
                            SetChild(_left, _currentNode.getFrameworkElement());
                            _lastX = 1;
                        }
                    }
                }
                if (_autoDrag > 0 && _autoDrag < 1)
                {
                    if (ttRight.X >= _right.ActualWidth * _autoDrag)
                    {
                        AnimH(false);
                        return false;
                    }
                    if (ttRight.X <= -_right.ActualWidth * (_autoDrag))
                    {
                        AnimH(true);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
       
        public bool VDrag(int yDistance)
        {
            if (_animating == 0)
            {
                TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
                TranslateTransform ttAbove = (TranslateTransform)_above.RenderTransform;
                if (Math.Abs(ttRight.X - 0) > 0.00000001)
                {
                    return true;
                }
                if (VNeighbourExists())
                {
                    ttAbove.Y = (double)ttAbove.GetValue(TranslateTransform.YProperty);
                    ttRight.Y = (double)ttRight.GetValue(TranslateTransform.YProperty);

                    ttAbove.BeginAnimation(TranslateTransform.YProperty, null);
                    ttRight.BeginAnimation(TranslateTransform.YProperty, null);

                    ttRight.Y += yDistance;
                    ttAbove.Y += yDistance;

                    if (ttRight.Y >= _right.ActualHeight || ttRight.Y <= -_right.ActualHeight)
                    {
                        Grid tmp = _right;
                        _right = _above;
                        _above = tmp;
                        ttRight = (TranslateTransform)_right.RenderTransform;
                        ttAbove = (TranslateTransform)_above.RenderTransform;
                        _lastY = 0;
                    }

                    if (Math.Abs(ttRight.Y - 0) < 0.00000001)
                    {
                        MarkCentered();
                    }
                    else
                    {
                        UnmarkCentered();
                    }

                    if (ttRight.Y >= 0 && _lastY < 0)
                    {
                        _currentNode = _currentNode.getAbove();
                        _lastY = 0;
                    }
                    if (ttRight.Y <= 0 && _lastY > 0)
                    {
                        _currentNode = _currentNode.getBelow();
                        _lastY = 0;
                    }

                    if (ttRight.Y < 0 && _lastY == 0)
                    {
                        ttAbove.Y = _right.ActualHeight + ttRight.Y;
                        _currentNode = _currentNode.getBelow();
                        SetChild(_above, _currentNode.getFrameworkElement());
                        _lastY = -1;
                    }
                    else
                    {
                        if (ttRight.Y > 0 && _lastY == 0)
                        {
                            ttAbove.Y = -_right.ActualHeight + ttRight.Y;
                            _currentNode = _currentNode.getAbove();
                            SetChild(_above, _currentNode.getFrameworkElement());
                            _lastY = 1;
                        }
                    }
                }
                if (_autoDrag > 0 && _autoDrag < 1)
                {
                    if (ttRight.Y >= _right.ActualHeight * _autoDrag)
                    {
                        AnimV(false);
                        return false;
                    }
                    if (ttRight.Y <= -_right.ActualHeight * (_autoDrag))
                    {
                        AnimV(true);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        void AnimCompleted()
        {
            _animating--;
        }

        public void AnimH(bool leftDir)
        {
            if (_animating == 0 && HNeighbourExists())
            {
                TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)_left.RenderTransform;

                if (Math.Abs(ttRight.X - 0) < 0.00000001)
                {
                    if (leftDir)
                    {
                        HDrag(-1);
                    }
                    else
                    {
                        HDrag(1);
                    }
                }
                _animating = 2;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation {From = ttRight.X};
                if (leftDir)
                    doubleAnimationCenter.To = -_right.ActualWidth;
                else
                    doubleAnimationCenter.To = _right.ActualWidth;
                doubleAnimationCenter.Duration = _duration;
                doubleAnimationCenter.Completed += (s, _) => AnimCompleted();
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation
                    {
                        From = ttLeft.X,
                        To = 0,
                        Duration = _duration
                    };
                doubleAnimationLeft.Completed += (s, _) => AnimCompleted();
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);

                Grid tmp = _right;
                _right = _left;
                _left = tmp;
                _lastX = 0;
                _lastY = 0;
            }
        }

        public void AnimV(bool upDir)
        {
            if (_animating != 0 || !VNeighbourExists()) return;
            TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
            TranslateTransform ttAbove = (TranslateTransform)_above.RenderTransform;

            if (Math.Abs(ttRight.Y) < 0.0000000001)
            {
                if (upDir)
                {
                    VDrag(-1);
                }
                else
                {
                    VDrag(1);
                }
            }
            _animating = 2;

            DoubleAnimation doubleAnimationCenter = new DoubleAnimation {From = ttRight.Y};
            if (upDir)
                doubleAnimationCenter.To = -_right.ActualHeight;
            else
                doubleAnimationCenter.To = _right.ActualHeight;
            doubleAnimationCenter.Duration = _duration;
            doubleAnimationCenter.Completed += (s, _) => AnimCompleted();
            ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

            DoubleAnimation doubleAnimationLeft = new DoubleAnimation {From = ttAbove.Y, To = 0, Duration = _duration};
            doubleAnimationLeft.Completed += (s, _) => AnimCompleted();
            ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);

            Grid tmp = _right;
            _right = _above;
            _above = tmp;
            _lastY = 0;
        }

        public void AnimBack()
        {
            if (_animating != 0) return;
            if (_lastX != 0)
            {
                TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
                TranslateTransform ttLeft = (TranslateTransform)_left.RenderTransform;

                _animating = 2;

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation
                    {
                        From = ttRight.X,
                        To = 0,
                        Duration = _duration
                    };
                doubleAnimationCenter.Completed += (s, _) => AnimCompleted();
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation {From = ttLeft.X};
                if (_lastX < 0)
                    doubleAnimationLeft.To = _right.ActualWidth;
                if (_lastX > 0)
                    doubleAnimationLeft.To = -_right.ActualWidth;
                doubleAnimationLeft.Duration = _duration;
                doubleAnimationLeft.Completed += (s, _) => AnimCompleted();
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);
                if (_lastX < 0)
                {
                    _currentNode = _currentNode.getLeft();
                }
                if (_lastX > 0)
                {
                    _currentNode = _currentNode.getRight();
                }
                _lastX = 0;
                _lastY = 0;
            }
            else
            {
                if (_lastY != 0)
                {
                    TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
                    TranslateTransform ttAbove = (TranslateTransform)_above.RenderTransform;

                    _animating = 2;

                    DoubleAnimation doubleAnimationCenter = new DoubleAnimation
                        {
                            From = ttRight.Y,
                            To = 0,
                            Duration = _duration
                        };
                    doubleAnimationCenter.Completed += (s, _) => AnimCompleted();
                    ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                    DoubleAnimation doubleAnimationLeft = new DoubleAnimation {From = ttAbove.Y};
                    if (_lastY < 0)
                        doubleAnimationLeft.To = _right.ActualHeight;
                    if (_lastY > 0)
                        doubleAnimationLeft.To = -_right.ActualHeight;
                    doubleAnimationLeft.Duration = _duration;
                    doubleAnimationLeft.Completed += (s, _) => AnimCompleted();
                    ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);
                    if (_lastY < 0)
                    {
                        _currentNode = _currentNode.getAbove();
                    }
                    if (_lastY > 0)
                    {
                        _currentNode = _currentNode.getBelow();
                    }
                    _lastY = 0;
                    _lastX = 0;
                }

            }
        }
    }
}
