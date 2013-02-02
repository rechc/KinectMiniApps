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
    /// Dieses UserControl ermöglicht es alle Klassen vom Typ FrameworkElement karusellartig in vertikaler und horizontaler (nicht beides gleichzeitig) Richtung zu bewegen.
    /// Dabei sind maximal nur 3 Objekte im UserControl tatsächlich geladen. Diese Objekte sind Grids (_left, _right, _above), die um Richtungspfeile dekoriert sind.
    /// In die Mitte eines jeden Grids wird das aktuelle FrameworkElement, welches angezeigt werden soll, geladen.
    /// Bei nur einem hinzugefuegten FrameworkElement gibt es keine Bewegung. Bei 2 FrameworkElementen jeweils nur in horizontaler oder vertikaler Richtung.
    /// 
    /// Die interne Datenstruktur ist ein Graph, der aus verlinkten Nodes besteht. Der Graph kann beliebig sein, ein Node kann jedoch maximal nur 4 eingehende/ausgehende Links setzen.
    /// Wenn der Graph nicht zusammenhängend ist, können nur Nodes erreicht werden, die vom ersten jemals eingefuegten Node aus erreichbar sind.
    /// 
    /// Die Einfuegemethoden sind AddNewToLeft usw.. Diese erzeugen neue Nodes.
    /// Muss auf einen bereits existierenden Node verlinkt werden, so muss dies in den jeweiligen Nodes gesetzt werden (z.B node.Right = otherNode).
    /// 
    /// </summary>
    public partial class LoopList
    {
        private Grid _left, _right, _above;
        private Node _currentNode;
        private int _animating;
        private int _lastX, _lastY;
        private double _autoDrag;
        private Duration _duration;
        private bool _firstAdded;

        public event EventHandler Scrolled;

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
            _left.RowDefinitions[1].Height = new GridLength(15, GridUnitType.Pixel);
            _left.ColumnDefinitions[1].Width = new GridLength(15, GridUnitType.Pixel);
            _left.RowDefinitions[3].Height = new GridLength(15, GridUnitType.Pixel);
            _left.ColumnDefinitions[3].Width = new GridLength(15, GridUnitType.Pixel);
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
                    Child = hPolygon,
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
                    Stretch = Stretch.Fill,
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

            RootGrid.Children.Add(_left);
            RootGrid.Children.Add(_right);
            RootGrid.Children.Add(_above);

            Loaded += LoopList_Loaded;

        }

        private void FireScrolled(LoopListArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            Scrolled(this, args);
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

        private void MarkDirections(Node node)
        {
            Grid grid;
            if (node.FrameworkElement.Parent == _left)
            {
                grid = _left;
            } else if (node.FrameworkElement.Parent == _right)
            {
                grid = _right;
            } else if (node.FrameworkElement.Parent == _above)
            {
                grid = _above;
            }
            else
            {
                return;
            }
            DoubleAnimation doubleAnimationOpacity = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = _duration.TimeSpan.Subtract(new TimeSpan((int)(_duration.TimeSpan.Ticks * -2))),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };


            if (node.HasLeftNeighbour())
            {
                grid.Children[4].Visibility = Visibility.Visible;

                grid.Children[4].BeginAnimation(OpacityProperty, doubleAnimationOpacity);

            }
            else
            {
                grid.Children[4].Visibility = Visibility.Collapsed;

                grid.Children[4].BeginAnimation(OpacityProperty, null);
            }

            if (node.HasRightNeighbour())
            {
                grid.Children[7].Visibility = Visibility.Visible;

                grid.Children[7].BeginAnimation(OpacityProperty, doubleAnimationOpacity);

            }
            else
            {
                grid.Children[7].Visibility = Visibility.Collapsed;

                grid.Children[7].BeginAnimation(OpacityProperty, null);
            }

            if (node.HasAboveNeighbour())
            {
                grid.Children[5].Visibility = Visibility.Visible;

                grid.Children[5].BeginAnimation(OpacityProperty, doubleAnimationOpacity);
            }
            else
            {
                grid.Children[5].Visibility = Visibility.Collapsed;

                grid.Children[5].BeginAnimation(OpacityProperty, null);
            }

            if (node.HasBelowNeighbour())
            {
                grid.Children[6].Visibility = Visibility.Visible;

                grid.Children[6].BeginAnimation(OpacityProperty, doubleAnimationOpacity);
            }
            else
            {
                grid.Children[6].Visibility = Visibility.Collapsed;

                grid.Children[6].BeginAnimation(OpacityProperty, null);
            }
        }

        

        private void SetChild(Grid grid, FrameworkElement frameworkElement, Node node)
        {
            if (grid == _above) //es kann sein, dass ein einzufügendes element noch in _left unnötig geladen ist.
            {
                if (_left.Children.Count == 9)
                {
                    _left.Children.RemoveAt(8);
                }
            }
            else if (grid == _left)
            {
                if (_above.Children.Count == 9)
                {
                    _above.Children.RemoveAt(8);
                }
            }
            Grid.SetRow(frameworkElement, 2);
            Grid.SetColumn(frameworkElement, 2);

            if (grid.Children.Count == 9)
            {
                grid.Children.RemoveAt(8);
            }
            grid.Children.Add(frameworkElement);
            MarkDirections(node);
        }

        public Node AddNewToLeft(Node anchor, FrameworkElement frameworkElement)
        {
            if (anchor == null)
            {
                anchor = new Node(frameworkElement);
                anchor.NodeChangedEvent += anchor_NodeChangedEvent;
            }
            else
            {
                Node newNode = new Node(frameworkElement);
                newNode.NodeChangedEvent += anchor_NodeChangedEvent;
                Node first = anchor.Left;
                anchor.Left = newNode;
                newNode.Right = anchor;
                newNode.Left = first;
                first.Right = newNode;
                anchor = newNode;
            }
            if (!_firstAdded)
            {
                _firstAdded = true;
                _currentNode = anchor;
                SetChild(_right, _currentNode.FrameworkElement, _currentNode);
            }
            return anchor;
        }

        void anchor_NodeChangedEvent(object sender, EventArgs e)
        {
            MarkDirections((Node)sender);
        }

        public Node AddNewToRight(Node anchor, FrameworkElement frameworkElement)
        {
            if (anchor == null)
            {
                anchor = new Node(frameworkElement);
                anchor.NodeChangedEvent += anchor_NodeChangedEvent;
            }
            else
            {
                Node newNode = new Node(frameworkElement);
                newNode.NodeChangedEvent += anchor_NodeChangedEvent;
                Node first = anchor.Right;
                anchor.Right = newNode;
                newNode.Left = anchor;
                newNode.Right = first;
                first.Left = newNode;
                anchor = newNode;
            }
            if (!_firstAdded)
            {
                _firstAdded = true;
                _currentNode = anchor;
                SetChild(_right, anchor.FrameworkElement, _currentNode);
                
            }
            return anchor;
        }

        public Node AddNewToAbove(Node anchor, FrameworkElement frameworkElement)
        {
            if (anchor == null)
            {
                anchor = new Node(frameworkElement);
                anchor.NodeChangedEvent += anchor_NodeChangedEvent;
            }
            else
            {
                Node newNode = new Node(frameworkElement);
                newNode.NodeChangedEvent += anchor_NodeChangedEvent;
                Node first = anchor.Above;
                anchor.Above = newNode;
                newNode.Below = anchor;
                newNode.Above = first;
                first.Below = newNode;
                anchor = newNode;
            }
            if (!_firstAdded)
            {
                _firstAdded = true;
                _currentNode = anchor;
                SetChild(_right, _currentNode.FrameworkElement, _currentNode);
            }
            return anchor;
        }

        public Node AddNewToBelow(Node anchor, FrameworkElement frameworkElement)
        {
            if (anchor == null)
            {
                anchor = new Node(frameworkElement);
                anchor.NodeChangedEvent += anchor_NodeChangedEvent;
            }
            else
            {
                Node newNode = new Node(frameworkElement);
                newNode.NodeChangedEvent += anchor_NodeChangedEvent;
                Node first = anchor.Below;
                anchor.Below = newNode;
                newNode.Above = anchor;
                newNode.Below = first;
                first.Above = newNode;
                anchor = newNode;
            }
            if (!_firstAdded)
            {
                _firstAdded = true;
                _currentNode = anchor;
                SetChild(_right, _currentNode.FrameworkElement, _currentNode);
            }
            return anchor;
        }

        private void MarkCentered()
        {
            for (int i = 0; i < 4; i++)
            {
                _right.Children[i].Visibility = Visibility.Visible;
            }
            ((TranslateTransform) _right.RenderTransform).X = 0;
            ((TranslateTransform)_right.RenderTransform).Y = 0;
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
            if (_animating != 0) return false;
            
            TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
            TranslateTransform ttLeft = (TranslateTransform)_left.RenderTransform;
            
            if ((int)Math.Abs(ttRight.Y) != 0) return true; // diagonales scrollen gibts nicht
            
            ttLeft.X = (double)ttLeft.GetValue(TranslateTransform.XProperty);
            ttRight.X = (double)ttRight.GetValue(TranslateTransform.XProperty);

            ttLeft.BeginAnimation(TranslateTransform.XProperty, null);
            ttRight.BeginAnimation(TranslateTransform.XProperty, null);
            
            ttRight.X += xDistance;
            ttLeft.X += xDistance;
            
            if (xDistance < 0 && (_currentNode.HasRightNeighbour() || (int) ttRight.X >= 0) ||
                xDistance > 0 && (_currentNode.HasLeftNeighbour() || (int) ttRight.X <= 0))
            {

                if (ttRight.X >= _right.ActualWidth || ttRight.X <= -_right.ActualWidth)
                {
                    Grid tmp = _right;
                    _right = _left;
                    _left = tmp;
                    _currentNode = _lastX < 0 ? _currentNode.Right : _currentNode.Left;
                    FireScrolled(_lastX > 0
                                     ? new LoopListArgs(Direction.Right)
                                     : new LoopListArgs(Direction.Left));
                    ttRight = (TranslateTransform) _right.RenderTransform;
                    ttLeft = (TranslateTransform) _left.RenderTransform;
                }

                if ((int) Math.Abs(ttRight.X) == 0)
                {
                    MarkCentered();
                }
                else
                {
                    UnmarkCentered();
                    if (ttRight.X < 0)
                    {
                        ttLeft.X = _right.ActualWidth + ttRight.X;
                        SetChild(_left, _currentNode.Right.FrameworkElement, _currentNode.Right);
                        _lastX = -1;
                    }
                    else
                    {
                        if (ttRight.X > 0)
                        {
                            ttLeft.X = -_right.ActualWidth + ttRight.X;
                            SetChild(_left, _currentNode.Left.FrameworkElement, _currentNode.Left);
                            _lastX = 1;
                        }
                    }
                }
            }
            else
            {
                if (xDistance != 0)
                {
                    if (!_currentNode.HasRightNeighbour() && (int) ttRight.X < 0)
                    {
                        ttLeft.X -= ttRight.X;
                        ttRight.X = 0;
                    }
                    else
                    {
                        if (!_currentNode.HasLeftNeighbour() && (int) ttRight.X > 0)
                        {
                            ttLeft.X -= ttRight.X;
                            ttRight.X = 0;
                        }
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
                if (ttRight.X < -_right.ActualWidth * (_autoDrag))
                {
                    AnimH(true);
                    return false;
                }
            }
            return true;
        }
       
        public bool VDrag(int yDistance)
        {
            if (_animating != 0) return false;
            TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
            TranslateTransform ttAbove = (TranslateTransform)_above.RenderTransform;
            if ((int)Math.Abs(ttRight.X) != 0)
            {
                return true;
            }

            ttAbove.Y = (double)ttAbove.GetValue(TranslateTransform.YProperty);
            ttRight.Y = (double)ttRight.GetValue(TranslateTransform.YProperty);

            ttAbove.BeginAnimation(TranslateTransform.YProperty, null);
            ttRight.BeginAnimation(TranslateTransform.YProperty, null);

            ttRight.Y += yDistance;
            ttAbove.Y += yDistance;


            if (yDistance < 0 && (_currentNode.HasBelowNeighbour() || ttRight.Y >= 0) ||
                yDistance > 0 && (_currentNode.HasAboveNeighbour() || ttRight.Y <= 0))
            {

                if (ttRight.Y >= _right.ActualHeight || ttRight.Y <= -_right.ActualHeight)
                {
                    Grid tmp = _right;
                    _right = _above;
                    _above = tmp;
                    _currentNode = _lastY < 0 ? _currentNode.Below : _currentNode.Above;
                    FireScrolled(_lastY < 0
                                     ? new LoopListArgs(Direction.Top)
                                     : new LoopListArgs(Direction.Down));
                    ttRight = (TranslateTransform) _right.RenderTransform;
                    ttAbove = (TranslateTransform) _above.RenderTransform;

                }
                if ((int) Math.Abs(ttRight.Y) == 0)
                {
                    MarkCentered();
                }
                else
                {
                    UnmarkCentered();

                    if (ttRight.Y < 0)
                    {
                        ttAbove.Y = _right.ActualHeight + ttRight.Y;
                        SetChild(_above, _currentNode.Below.FrameworkElement, _currentNode.Below);
                        _lastY = -1;
                    }
                    else
                    {
                        if (ttRight.Y > 0)
                        {
                            ttAbove.Y = -_right.ActualHeight + ttRight.Y;
                            SetChild(_above, _currentNode.Above.FrameworkElement, _currentNode.Above);
                            _lastY = 1;
                        }
                    }
                }

            }
            else
            {
                if (yDistance != 0)
                {
                    if (!_currentNode.HasBelowNeighbour() && ttRight.Y < 0)
                    {
                        ttAbove.Y -= ttRight.Y;
                        ttRight.Y = 0;

                    }
                    else
                    {
                        if (!_currentNode.HasAboveNeighbour() && ttRight.Y > 0)
                        {
                            ttAbove.Y -= ttRight.Y;
                            ttRight.Y = 0;
                        }
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

        void AnimCompletedX()
        {
            _animating--;
            if (_animating == 0)
            {
                LoopListArgs lla = _lastX > 0
                                        ? new LoopListArgs(Direction.Right)
                                        : new LoopListArgs(Direction.Left);
                _lastX = 0;
                FireScrolled(lla);
            }
        }

        void AnimCompletedY()
        {
            _animating--;
            if (_animating == 0)
            {
                LoopListArgs lla = _lastY > 0
                                        ? new LoopListArgs(Direction.Down)
                                        : new LoopListArgs(Direction.Top);
                _lastY = 0;
                FireScrolled(lla);
            }
        }

        void AnimCompletedBack()
        {
            _animating--;
        }

        public void AnimH(bool leftDir)
        {
            if (_animating != 0 || !(leftDir && _currentNode.HasRightNeighbour() || !leftDir && _currentNode.HasLeftNeighbour())) return;
            TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
            TranslateTransform ttLeft = (TranslateTransform)_left.RenderTransform;
            
            if ((int)Math.Abs(ttRight.Y) != 0)
            {
                return;
            }
            if ((int)Math.Abs(ttRight.X) == 0)
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
            doubleAnimationCenter.Completed += (s, _) => AnimCompletedX();
            ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);

            DoubleAnimation doubleAnimationLeft = new DoubleAnimation
                {
                    From = ttLeft.X,
                    To = 0,
                    Duration = _duration
                };
            doubleAnimationLeft.Completed += (s, _) => AnimCompletedX();
            ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);



            Grid tmp = _right;
            _right = _left;
            _left = tmp;

            _currentNode = _lastX < 0 ? _currentNode.Right : _currentNode.Left;
        }

        public void AnimV(bool upDir)
        {
            if (_animating != 0 || !(upDir && _currentNode.HasBelowNeighbour() || !upDir && _currentNode.HasAboveNeighbour())) return;
            TranslateTransform ttRight = (TranslateTransform)_right.RenderTransform;
            TranslateTransform ttAbove = (TranslateTransform)_above.RenderTransform;

            if ((int)Math.Abs(ttRight.X) != 0)
            {
                return;
            }

            if ((int)Math.Abs(ttRight.Y) == 0)
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
            doubleAnimationCenter.Completed += (s, _) => AnimCompletedY();
            ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

            DoubleAnimation doubleAnimationLeft = new DoubleAnimation {From = ttAbove.Y, To = 0, Duration = _duration};
            doubleAnimationLeft.Completed += (s, _) => AnimCompletedY();
            ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);



            Grid tmp = _right;
            _right = _above;
            _above = tmp;

            _currentNode = _lastY < 0 ? _currentNode.Below : _currentNode.Above;
        }

        public bool IsAnimating()
        {
            return _animating > 0;
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
                doubleAnimationCenter.Completed += (s, _) => AnimCompletedBack();
                

                DoubleAnimation doubleAnimationLeft = new DoubleAnimation {From = ttLeft.X};
                if (_lastX < 0)
                    doubleAnimationLeft.To = _right.ActualWidth;
                if (_lastX > 0)
                    doubleAnimationLeft.To = -_right.ActualWidth;
                doubleAnimationLeft.Duration = _duration;
                doubleAnimationLeft.Completed += (s, _) => AnimCompletedBack();
                
               
                ttRight.BeginAnimation(TranslateTransform.XProperty, doubleAnimationCenter);
                ttLeft.BeginAnimation(TranslateTransform.XProperty, doubleAnimationLeft);
                _lastX = 0;
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
                    doubleAnimationCenter.Completed += (s, _) => AnimCompletedBack();
                    

                    DoubleAnimation doubleAnimationLeft = new DoubleAnimation {From = ttAbove.Y};
                    if (_lastY < 0)
                        doubleAnimationLeft.To = _right.ActualHeight;
                    if (_lastY > 0)
                        doubleAnimationLeft.To = -_right.ActualHeight;
                    doubleAnimationLeft.Duration = _duration;
                    doubleAnimationLeft.Completed += (s, _) => AnimCompletedBack();
                    
                   
                    ttRight.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);
                    ttAbove.BeginAnimation(TranslateTransform.YProperty, doubleAnimationLeft);
                    _lastY = 0;
                }
            }
        }
    }
}
