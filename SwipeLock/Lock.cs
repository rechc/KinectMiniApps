using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class Lock
    {
        private Ellipse _ellipse;
        private double _width;
        private bool _isFading = false;
        private Canvas _canvas;
        private double _position;

        public double Position // 0 means right, 1 means left
        {
            get
            {
                return _position;
            }
            set 
            {
                if (value > 1) value = 1;
                if (value < 0) value = 0;

                _position = value;
                UpdateEllipseTransition();    
            } 
        }

        public Lock(Ellipse ellipse, Canvas canvas, double width)
        {
            _ellipse = ellipse;
            _canvas = canvas;
            _width = width;
            Position = 0;
        }

        public void Reset()
        {
            Position = 0;
        }

        public void Hide()
        {
            _canvas.Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            _canvas.Visibility = Visibility.Visible;
        }

        public void MoveLeft(double step)
        {
            Position += step;
        }

        private void UpdateEllipseTransition()
        {
            TranslateTransform tt = (TranslateTransform)_ellipse.RenderTransform;
            double x = Position * _width;
            tt.X = -x;
        }

        private void FadeOut()
        {
            if (!_isFading)
            {
                _isFading = true;
                DoubleAnimation doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(1)),
                    To = 0,
                    From = 100
                };
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(doubleAnimation);
                Storyboard.SetTarget(doubleAnimation, _ellipse);
                //Storyboard.SetTargetName(doubleAnimation, ellipse.Name);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Canvas.RightProperty));
                storyboard.Begin();
                
            }
        }

    }
}
