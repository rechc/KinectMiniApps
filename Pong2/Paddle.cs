using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Paddle handles its own position.
    /// </summary>
    public class Paddle
    {
        public Brush Color { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public float w { get; set; }
        public float h { get; set; }

        public Paddle(Brush color, double x, double y, float w, float h)
        {
            Color = color;
            this.x = x;
            this.y = y;
            this.h = h;
            this.w = w;
        }

        public void Draw(DrawingContext dc)
        {
            dc.DrawRectangle(this.Color, null, new Rect(x, y, w, h));
        }

        public void MoveTo(double y)
        {
            this.y = y;
        }
    }
}
