using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RectNavigation
{
    public delegate void AnimationCompletedDelegate(object sender, EventArgs e);

    class Animate
    {
        public static void Opacity(UIElement element, double from, double to, double seconds, AnimationCompletedDelegate callback)
        {
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                FillBehavior = FillBehavior.Stop
            };
            element.Opacity = to;
            if (callback != null)
            {
                fadeOutAnimation.Completed += (sender, _) => callback(sender, _);
            }
            element.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        }

        public static void Opacity(UIElement element, double from, double to, double seconds)
        {
            Opacity(element, from, to, seconds, null);
        }

        public static void Move(DependencyProperty dependencyProperty, UIElement element, double from, double to, double seconds, AnimationCompletedDelegate callback)
        {
            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(seconds))
            };
            if (callback != null)
            {
                moveAnimation.Completed += (sender, _) => callback(sender, _);
            }
            if (element.RenderTransform == null)
            {
                element.RenderTransform = new TranslateTransform();
            }
            element.RenderTransform.BeginAnimation(dependencyProperty, moveAnimation);
        }

        public static void Move(UIElement element, Point from, Point to, double seconds, AnimationCompletedDelegate callback)
        {
            Move(TranslateTransform.XProperty, element, from.X, to.X, seconds, null);
            Move(TranslateTransform.YProperty, element, from.Y, to.Y, seconds, callback);
        }

        public static void Move(UIElement element, double x, double y)
        {
            TranslateTransform translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;
            translateTransform.X = x;
            translateTransform.Y = y;
        }

        public static void Move(UIElement element, Point to)
        {
            Move(element, to.X, to.Y);
        }

        public static void MoveWithRotationAndFadeOut(FrameworkElement element, Point to, double rotationAngle, double seconds, AnimationCompletedDelegate callback)
        {
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform(rotationAngle, element.Width / 2, element.Height / 2));

            Duration duration = new Duration(TimeSpan.FromSeconds(seconds));
            DoubleAnimation animationX = new DoubleAnimation(to.X, duration);
            DoubleAnimation animationY = new DoubleAnimation(to.Y, duration);
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.4,
                Duration = duration,
                FillBehavior = FillBehavior.Stop
            };

            animationX.Completed += (sender, _) => callback(sender, _);

            TranslateTransform trans = new TranslateTransform();
            element.RenderTransform = transformGroup;

            trans.BeginAnimation(TranslateTransform.XProperty, animationX);
            trans.BeginAnimation(TranslateTransform.YProperty, animationY);

            element.Opacity = 0.4;
            element.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            transformGroup.Children.Add(trans);
        }
    }
}
