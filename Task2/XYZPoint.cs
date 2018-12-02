using System;
using System.Drawing;

namespace IndividualTask2
{
    class XYZPoint : Primitive
    {
        public XYZPoint()
        {
        }

        public XYZPoint(double a, double b, double c)
        {
            x = a;
            y = b;
            z = c;
        }

        public XYZPoint(XYZPoint a)
        {
            x = a.x;
            y = a.y;
            z = a.z;
        }

        public void Copy(XYZPoint a)
        {
            x = a.x;
            y = a.y;
            z = a.z;
        }

        public Point To2d()
        {
            int xp = (int)Math.Round(Point0.X + pixelsPerUnit * y);
            int yp = (int)Math.Round(Point0.Y - pixelsPerUnit * z);

            return new Point(xp, yp);
        }

        protected double x, y, z;

        public double X
        {
            get
            {
                return x;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
        }

        public double Z
        {
            get
            {
                return z;
            }
        }

        public Primitive Clone()
        {
            return new XYZPoint(this);
        }

        public void Draw(Graphics g, Color c) { /* EMPTY */ }

        static Point Point0 = new Point(200, 170);

        public static void DrawPoint0(Graphics g, Color c)
        {
            SolidBrush b = new SolidBrush(c);
            g.FillEllipse(b, Point0.X - 2, Point0.Y - 2, 5, 5);
        }

        int pixelsPerUnit = 40;
    }
}
