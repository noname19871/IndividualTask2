using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndividualTask2
{
    class Vector
    {
        public Vector() { }

        public Vector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector(XYZPoint a)
        {
            this.x = a.X;
            this.y = a.Y;
            this.z = a.Z;
        }

        public Vector(XYZPoint a, XYZPoint b)
        {
            this.x = b.X - a.X;
            this.y = b.Y - a.Y;
            this.z = b.Z - a.Z;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector operator -(Vector a)
        {
            return new Vector(-a.x, -a.y, -a.z);
        }

        public static Vector operator *(double a, Vector b)
        {
            return new Vector(a * b.x, a * b.y, a * b.z);
        }

        public static Vector operator *(Vector a, double b)
        {
            return new Vector(a.x * b, a.y * b, a.z * b);
        }

        public static double operator *(Vector a, Vector b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public double Norm()
        {
            return Math.Sqrt(this * this);
        }

        public Vector Normalize()
        {
            return this * (1.0 / this.Norm());
        }

        public Vector this[Vector a]
        {
            get
            {
                return new Vector(this.y * a.z - a.y * this.z, this.z * a.x - a.z * this.x, this.x * a.y - a.x * this.y);
            }
        }

        private double x, y, z;

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
    }
}
