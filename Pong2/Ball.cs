using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class Ball
    {
        public Brush color { get; set; }
        public double x;
        public double y;
        public float r;
        public double dx;
        public double dy;
        private double fieldWidth;
        private double fieldHeight;
        private Random random;

        public Ball(Brush color, float r, double fieldWidth, double fieldHeight)
        {
            this.color = color;
            this.fieldWidth = fieldWidth;
            this.fieldHeight = fieldHeight;
            this.r = r;
            this.dx = 0;
            this.dy = 0;
            random = new Random();
        }

        public void Draw(DrawingContext dc)
        {
            dc.DrawRectangle(this.color, null, new Rect(x, y, r, r));
        }

        public void DetectCollision(Paddle pLeft, Paddle pRight)
        {
            if (y < 0 || y > fieldHeight - r) dy = -dy;
            if ((y >= pLeft.y - r) && (y <= pLeft.y + pLeft.h) && (x <= pLeft.x + pLeft.w)) dx = -dx;
            if ((y >= pRight.y - r) && (y <= pRight.y + pRight.h) && (x + r >= pRight.x)) dx = -dx;
        }

        public bool HitLeft()
        {
            return x < 0;
        }

        public bool HitRight()
        {
            return x > fieldWidth - r;
        }

        public void Move()
        {
            x += dx;
            y += dy;
        }

        public void Spawn()
        {
            x = fieldWidth / 2 - r / 2;
            y = fieldHeight / 2 - r / 2;
            dx = 5 * plusOrMinus();
            dy = 5 * random.NextDouble() * plusOrMinus();
        }

        private double plusOrMinus()
        {
            return random.NextDouble() <= 0.5 ? 1 : -1;
        }

    }
}
