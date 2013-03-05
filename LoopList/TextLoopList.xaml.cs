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
        private Viewbox _top = new Viewbox();
        private Viewbox _bottom = new Viewbox();
        private readonly List<string> _texts = new List<string>();
        private int _animating;
        private int _lastY;
        private int _index;
        private Duration _duration;

        public event EventHandler Scrolled;


        public TextLoopList()
        {
            InitializeComponent();


            _duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));

            TextBlock topBlock = new TextBlock();
            TextBlock bottomBlock = new TextBlock();


            topBlock.Foreground = new SolidColorBrush(Colors.White);

            bottomBlock.VerticalAlignment = VerticalAlignment.Center;

            _top.Child = topBlock;
            _bottom.Child = bottomBlock;

            _top.Margin = new Thickness(20);
            _bottom.Margin = new Thickness(20);

            SetFontFamily("Verdana");

            _top.RenderTransform = new TranslateTransform();
            _bottom.RenderTransform = new TranslateTransform();

            RootGrid.Children.Add(_top);
            RootGrid.Children.Add(_bottom);

            TranslateTransform ttBottom = (TranslateTransform)_bottom.RenderTransform;

            ttBottom.Y = RootGrid.ActualHeight;

        }

        public void SetDuration(Duration duration)
        {
            _duration = duration;
        }

        public void SetFontFamily(string ff)
        {
            ((TextBlock)_top.Child).FontFamily = new FontFamily(ff);
            ((TextBlock)_bottom.Child).FontFamily = new FontFamily(ff);
        }

        public void SetFontSize(int fontSize)
        {
            ((TextBlock)_top.Child).FontSize = fontSize;
            ((TextBlock)_bottom.Child).FontSize = fontSize;

            _top.Height = fontSize;
            _bottom.Height = fontSize;
        }

        public void SetFontColor(Color color)
        {
            SolidColorBrush colorBrush = new SolidColorBrush(color);
            ((TextBlock)_top.Child).Foreground = colorBrush;
            ((TextBlock)_bottom.Child).Foreground = colorBrush;
        }

        public void SetWordWrap(TextWrapping wrap)
        {         
            ((TextBlock)_top.Child).TextWrapping = wrap;
            ((TextBlock)_bottom.Child).TextWrapping = wrap;
        }


        private void FireScrolled(EventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            Scrolled(this, args);
        }



        public void Add(string text)
        {
            if (_texts.Count == 0)
                ((TextBlock)_top.Child).Text = text;
            _texts.Add(text);
        }


        public bool Anim(bool up)
        {
            
            if (_animating > 0 || _texts.Count <= 1) return false;
            TranslateTransform ttTop = (TranslateTransform)_top.RenderTransform;
            TranslateTransform ttBottom = (TranslateTransform)_bottom.RenderTransform;

            _animating = 4;

            Viewbox disappearing = null;
            Viewbox appearing = null;
            if (up)
            {

                appearing = _bottom;
                disappearing = _top;
                DoubleAnimation doubleAnimationTop = new DoubleAnimation
                    {
                        From = 0,
                        To = -RootGrid.ActualHeight/2,
                        Duration = _duration,
                        FillBehavior = FillBehavior.Stop
                    };
                doubleAnimationTop.Completed += (s, _) => AnimCompleted();
                ttTop.Y = RootGrid.ActualHeight;
                ttTop.BeginAnimation(TranslateTransform.YProperty, doubleAnimationTop);
                
                DoubleAnimation doubleAnimationBottom = new DoubleAnimation
                    {
                        From = RootGrid.ActualHeight,
                        To = 0,
                        Duration = _duration,
                        FillBehavior = FillBehavior.Stop
                    };
                doubleAnimationBottom.Completed += (s, _) => AnimCompleted();
                ttBottom.Y = 0;
                ttBottom.BeginAnimation(TranslateTransform.YProperty, doubleAnimationBottom);

                _index = NextIndex();

                ((TextBlock)_bottom.Child).Text = _texts[_index];
               

                _lastY = 1;
            }
            else
            {
  
                appearing = _bottom;
                disappearing = _top;
                DoubleAnimation doubleAnimationTop = new DoubleAnimation
                {
                    From = 0,
                    To = RootGrid.ActualHeight,
                    Duration = _duration,
                    FillBehavior = FillBehavior.Stop
                };
                doubleAnimationTop.Completed += (s, _) => AnimCompleted();
                ttTop.Y = RootGrid.ActualHeight;
                ttTop.BeginAnimation(TranslateTransform.YProperty, doubleAnimationTop);

                DoubleAnimation doubleAnimationBottom = new DoubleAnimation
                {
                    From = -RootGrid.ActualHeight/2,
                    To = 0,
                    Duration = _duration,
                    FillBehavior = FillBehavior.Stop
                };
                doubleAnimationBottom.Completed += (s, _) => AnimCompleted();
                ttBottom.Y = 0;
                ttBottom.BeginAnimation(TranslateTransform.YProperty, doubleAnimationBottom);

                _index = PreviousIndex();

                ((TextBlock)_bottom.Child).Text = _texts[_index];


                _lastY = -1;
            }
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = _duration.TimeSpan.Subtract(new TimeSpan((int)(_duration.TimeSpan.Ticks*0.5))),
                FillBehavior = FillBehavior.Stop
            };
            fadeOut.Completed += (s, _) => AnimCompleted();
            disappearing.Opacity = 0;
            disappearing.BeginAnimation(OpacityProperty, fadeOut);

            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = _duration.TimeSpan.Subtract(new TimeSpan((int)(_duration.TimeSpan.Ticks * 0.5))),
                FillBehavior = FillBehavior.Stop
            };
            fadeOut.Completed += (s, _) => AnimCompleted();
            appearing.Opacity = 1;
            appearing.BeginAnimation(OpacityProperty, fadeIn);
            return true;

        }

        private void AnimCompleted()
        {
            _animating--;
            if (_animating != 0) return;
            Viewbox tmp = _bottom;
            _bottom = _top;
            _top = tmp;
            if (_lastY > 0)
                FireScrolled(new LoopListTextArgs(Direction.Top));
            else
            {
                if (_lastY < 0)
                    FireScrolled(new LoopListTextArgs(Direction.Down));
            }
        }

        private int NextIndex()
        {
            int tmpIndex = _index + 1;
            if (tmpIndex == _texts.Count)
            {
                tmpIndex = 0;
            }
            return tmpIndex;
        }

        private int PreviousIndex()
        {
            int tmpIndex = _index - 1;
            if (tmpIndex == -1)
            {
                tmpIndex = _texts.Count - 1;
            }
            return tmpIndex;
        }

        public string[] GetNeighbourTexts()
        {
            string[] texts = new string[2];
            texts[0] = _texts[PreviousIndex()];
            texts[1] = _texts[NextIndex()];
            return texts;
        }
    }
}
