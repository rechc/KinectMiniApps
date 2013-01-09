using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LoopList
{
    /// <summary>
    /// Interaktionslogik für TextLoopList.xaml
    /// </summary>
    public partial class TextLoopList
    {
        private Border _top = new Border();
        private Border _center = new Border();
        private Border _bottom = new Border();
        private readonly List<string> _texts = new List<string>();
        private int _animating;
        private int _lastY;
        private int _lastLastY;
        private int _index;
        private double _topYPos, _bottomYPos;
        private Duration _duration;

        public event EventHandler Scrolled;


        public TextLoopList()
        {
            InitializeComponent();


            _duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));

            TextBlock centerBlock = new TextBlock();
            TextBlock topBlock = new TextBlock();
            TextBlock bottomBlock = new TextBlock();

            centerBlock.HorizontalAlignment = HorizontalAlignment.Center;
            centerBlock.VerticalAlignment = VerticalAlignment.Center;
            centerBlock.Foreground = new SolidColorBrush(Colors.White);

            topBlock.HorizontalAlignment = HorizontalAlignment.Center;
            topBlock.VerticalAlignment = VerticalAlignment.Center;
            topBlock.Foreground = new SolidColorBrush(Colors.White);

            bottomBlock.HorizontalAlignment = HorizontalAlignment.Center;
            bottomBlock.Foreground = new SolidColorBrush(Colors.White);
            bottomBlock.VerticalAlignment = VerticalAlignment.Center;

            _top.Child = topBlock;
            _center.Child = centerBlock;
            _bottom.Child = bottomBlock;

            SetFontSize(10);
            SetFontFamily("Verdana");

            _top.RenderTransform = new TranslateTransform();
            _center.RenderTransform = new TranslateTransform();
            _bottom.RenderTransform = new TranslateTransform();


            RootGrid.Children.Add(_top);
            RootGrid.Children.Add(_center);
            RootGrid.Children.Add(_bottom);

            SizeChanged +=TextLoopList_SizeChanged;
        }

        public void SetDuration(Duration duration)
        {
            _duration = duration;
        }

        public void SetFontFamily(string ff)
        {
            ((TextBlock)_top.Child).FontFamily = new FontFamily(ff);
            ((TextBlock)_center.Child).FontFamily = new FontFamily(ff);
            ((TextBlock)_bottom.Child).FontFamily = new FontFamily(ff);
        }

        public void SetFontSize(int fontSize)
        {
            ((TextBlock)_top.Child).FontSize = fontSize;
            ((TextBlock)_center.Child).FontSize = fontSize;
            ((TextBlock)_bottom.Child).FontSize = fontSize;

            _top.Height = fontSize + 2;
            _center.Height = fontSize + 2;
            _bottom.Height = fontSize + 2;
        }


        private void FireScrolled(EventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            Scrolled(this, args);
        }

        private void TextLoopList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TranslateTransform ttTop = (TranslateTransform)_top.RenderTransform;
            TranslateTransform ttCenter = (TranslateTransform)_center.RenderTransform;
            TranslateTransform ttBottom = (TranslateTransform)_bottom.RenderTransform;

            ttTop.BeginAnimation(TranslateTransform.YProperty, null);
            ttCenter.BeginAnimation(TranslateTransform.YProperty, null);
            ttBottom.BeginAnimation(TranslateTransform.YProperty, null);

            _topYPos = -RootGrid.ActualHeight/3.5 + _top.ActualHeight/2.0;
            _bottomYPos = RootGrid.ActualHeight/3.5 - _bottom.ActualHeight/2.0;

            ttTop.Y = _topYPos;
            ttCenter.Y = 0;
            ttBottom.Y = _bottomYPos;
        }


        public void Add(string text)
        {
            switch (_texts.Count)
            {
                case 0:
                    ((TextBlock)_center.Child).Text = text;
                    break;
                case 1:
                    ((TextBlock)_top.Child).Text = text;
                    ((TextBlock)_bottom.Child).Text = text;
                    break;
                default:
                    ((TextBlock)_top.Child).Text = text;
                    break;
            }
            _texts.Add(text);
        }


        public bool Anim(bool up)
        {
            
            if (_animating > 0 || _texts.Count <= 1) return false;
            TranslateTransform ttTop = (TranslateTransform)_top.RenderTransform;
            TranslateTransform ttCenter = (TranslateTransform)_center.RenderTransform;
            TranslateTransform ttBottom = (TranslateTransform)_bottom.RenderTransform;

            _animating = 4;

            _lastLastY = _lastY;

            Border disappearing; 
            if (up)
            {
                disappearing = _top;
                DoubleAnimation doubleAnimationTop = new DoubleAnimation
                    {
                        From = ttTop.Y,
                        To = _topYPos*2,
                        Duration = _duration
                    };
                ttTop.BeginAnimation(TranslateTransform.YProperty, doubleAnimationTop);

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation
                    {
                        From = ttCenter.Y,
                        To = _topYPos,
                        Duration = _duration
                    };
                doubleAnimationCenter.Completed += (s, _) => AnimCompleted();
                ttCenter.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationBottom = new DoubleAnimation
                    {
                        From = ttBottom.Y,
                        To = 0,
                        Duration = _duration
                    };
                doubleAnimationBottom.Completed += (s, _) => AnimCompleted();
                ttBottom.BeginAnimation(TranslateTransform.YProperty, doubleAnimationBottom);
                _lastY = 1;
                Border tmp = _top;
                _top = _center;
                _center = _bottom;
                _bottom = tmp;
            }
            else
            {
                disappearing = _bottom;
                DoubleAnimation doubleAnimationTop = new DoubleAnimation
                {
                    From = ttTop.Y,
                    To = 0,
                    Duration = _duration
                };
                doubleAnimationTop.Completed += (s, _) => AnimCompleted();
                ttTop.BeginAnimation(TranslateTransform.YProperty, doubleAnimationTop);

                DoubleAnimation doubleAnimationCenter = new DoubleAnimation
                {
                    From = ttCenter.Y,
                    To = _bottomYPos,
                    Duration = _duration
                };
                doubleAnimationCenter.Completed += (s, _) => AnimCompleted();
                ttCenter.BeginAnimation(TranslateTransform.YProperty, doubleAnimationCenter);

                DoubleAnimation doubleAnimationBottom = new DoubleAnimation
                {
                    From = ttBottom.Y,
                    To = _bottomYPos*2,
                    Duration = _duration
                };
                ttBottom.BeginAnimation(TranslateTransform.YProperty, doubleAnimationBottom);
                
                _lastY = -1;
                Border tmp = _bottom;
                _bottom = _center;
                _center = _top;
                _top= tmp;
            }
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = _duration.TimeSpan.Subtract(new TimeSpan((int)(_duration.TimeSpan.Ticks*0.5)))
            };
            fadeOut.Completed += (s, _) => FadeOutAnimCompleted();
            
            disappearing.BeginAnimation(OpacityProperty, fadeOut);
            return true;
        }

        private void FadeOutAnimCompleted()
        {
            Border appearing;
            if (_lastY > 0)
            {
                appearing = _bottom;
                NextIndex();
                if (_lastLastY == 0)
                {
                    NextIndex();
                }
                else
                {
                    if (_lastLastY < 0)
                    {
                        NextIndex();
                        NextIndex();
                    }

                }

                ((TextBlock)_bottom.Child).Text = _texts[_index];

                TranslateTransform ttBottom = (TranslateTransform)_bottom.RenderTransform;
                DoubleAnimation doubleAnimationBottom = new DoubleAnimation
                {
                    From = _bottomYPos * 2,
                    To = _bottomYPos,
                    Duration = _duration
                };
                doubleAnimationBottom.Completed += (s, _) => AnimCompleted();
                ttBottom.BeginAnimation(TranslateTransform.YProperty, doubleAnimationBottom);

            }
            else
            {
                appearing = _top;
                PreviousIndex();
                if (_lastLastY == 0)
                {
                    PreviousIndex();
                }
                else
                {
                    if (_lastLastY > 0)
                    {
                        PreviousIndex();
                        PreviousIndex();
                    }
                }

                ((TextBlock)_top.Child).Text = _texts[_index];

                TranslateTransform ttBottom = (TranslateTransform)_top.RenderTransform;
                DoubleAnimation doubleAnimationBottom = new DoubleAnimation
                {
                    From = _topYPos * 2,
                    To = _topYPos,
                    Duration = _duration
                };
                doubleAnimationBottom.Completed += (s, _) => AnimCompleted();
                ttBottom.BeginAnimation(TranslateTransform.YProperty, doubleAnimationBottom);

            }
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = _duration
            };
            fadeIn.Completed += (s, _) => AnimCompleted();
            appearing.BeginAnimation(OpacityProperty, fadeIn);

        }

        private void AnimCompleted()
        {
            _animating--;
            if (_animating != 0) return;
            if (_lastY > 0)
                FireScrolled(new LoopListArgs(Direction.Top));
            else
            {
                if (_lastY < 0)
                    FireScrolled(new LoopListArgs(Direction.Down));
            }
        }

        private void NextIndex()
        {
            _index++;
            if (_index == _texts.Count)
            {
                _index = 0;
            }
        }

        private void PreviousIndex()
        {
            _index--;
            if (_index == -1)
            {
                _index = _texts.Count - 1;
            }
        }
    }
}
