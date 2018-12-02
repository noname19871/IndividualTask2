using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IndividualTask2
{

    class Sphere : Model
    {
        XYZPoint center;
        double radius;

        public XYZPoint C
        {
            get
            {
                return center;
            }
        }

        public double R
        {
            get
            {
                return radius;
            }
        }

        public Sphere(XYZPoint c, double r)
        {
            this.center = c;
            this.radius = r;
        }

        public Sphere(Polyhedron ph)
        {
            var p = ph.Verges[0].Edges[0].First;
            var dist = new Vector(ph.Center, p).Norm();
            foreach (var f in ph.Verges)
                foreach (var e in f.Edges)
                {
                    var pp = e.First;
                    var ddist = new Vector(ph.Center, pp).Norm();
                    if (ddist > dist)
                    {
                        dist = ddist;
                        p = pp;
                    }
                }
            
            this.center = ph.Center;
            this.radius = dist;
        }

        public override bool find_cross(XYZPoint Camera_pos, XYZPoint ray_pos, ref XYZPoint t)
        {
            Vector d = new Vector(
                ray_pos.X - Camera_pos.X,
                ray_pos.Y - Camera_pos.Y,
                ray_pos.Z - Camera_pos.Z);
            Vector c = new Vector(
                Camera_pos.X - this.C.X,
                Camera_pos.Y - this.C.Y,
                Camera_pos.Z - this.C.Z);

            double k1 = d * d,
                   k2 = 2 * (c * d),
                   k3 = (c * c) - this.R * this.R;
            double D = k2 * k2 - 4 * k1 * k3;
            if (D < 0)
                return false;

            double x1 = (-k2 + Math.Sqrt(D)) / (2 * k1);
            double x2 = (-k2 - Math.Sqrt(D)) / (2 * k1);
            double x = 0;
            if (x1 < eps && x2 < eps)
                return false;
            else if (x1 < eps)
                x = x2;
            else if (x2 < eps)
                x = x1;
            else
                x = x1 < x2 ? x1 : x2;

            t = new XYZPoint(
                Camera_pos.X + d.X * x,
                Camera_pos.Y + d.Y * x,
                Camera_pos.Z + d.Z * x);
            return true;
        }

        public override Vector normal(XYZPoint p)
        {
            return new Vector(this.C, p);
        }
    }

    class Wall : Model
    {
        XYZPoint[] points;
        double square;

        public Wall(XYZPoint p1, XYZPoint p2, XYZPoint p3, XYZPoint p4)
        {
            Vector v1 = new Vector(p1, p2);
            Vector v2 = new Vector(p1, p3);
            Vector n = v1[v2];
            double d = -(n.X * p1.X + n.Y * p1.Y + n.Z * p1.Z);
            points = new XYZPoint[4];
            points[0] = p1;
            points[1] = p2;
            points[2] = p3;
            points[3] = p4;
            square = triangle_square(p1, p2, p3) + triangle_square(p1, p3, p4);
        }

        private double triangle_square(XYZPoint A, XYZPoint B, XYZPoint C)
        {
            double a = new Vector(B, C).Norm();
            double b = new Vector(A, C).Norm();
            double c = new Vector(A, B).Norm();
            double p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        public override bool find_cross(XYZPoint Camera_pos, XYZPoint ray_pos, ref XYZPoint t)
        {
            Vector n = this.normal(null);
            double d = -(n.X * points[0].X + n.Y * points[0].Y + n.Z * points[0].Z);

            Vector v = new Vector(Camera_pos, ray_pos);
            Vector u = new Vector(Camera_pos);

            double denum = n * v;
            if (Math.Abs(denum) < eps)
                return false;
            double num = n * u + d;
            double tp = -num / denum;
            if (tp < eps)
                return false;
            t = new XYZPoint(
                v.X * tp + u.X,
                v.Y * tp + u.Y,
                v.Z * tp + u.Z);
            double square = 0;
            for (int i = 0; i < 4; ++i)
                square += triangle_square(points[i], points[(i + 1) % 4], t);
            if (Math.Abs(this.square - square) > eps)
                return false;

            return true;
        }

        public override Vector normal(XYZPoint p)
        {
            Vector v1 = new Vector(points[0], points[1]);
            Vector v2 = new Vector(points[0], points[2]);
            return v1[v2];
        }
    }

    class Cube : Model
    {
        Polyhedron ph;
        Sphere outside_sphere;
        Dictionary<Verge, double> squares = new Dictionary<Verge, double>();

        public Cube(Polyhedron p)
        {
            ph = p.Clone() as Polyhedron;
            outside_sphere = new Sphere(ph);
            squares.Clear();
            foreach (var face in ph.Verges)
            {
                double s = 0;
                int cnt = face.Edges.Count();
                for (int i = 1; i < cnt - 1; ++i)
                    s += triangle_square(face.Edges[0].First, face.Edges[i].First, face.Edges[i + 1].First);
                squares.Add(face, s);
            }
        }

        private double triangle_square(XYZPoint A, XYZPoint B, XYZPoint C)
        {
            double a = new Vector(B, C).Norm();
            double b = new Vector(A, C).Norm();
            double c = new Vector(A, B).Norm();
            double p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        public override bool find_cross(XYZPoint Camera_pos, XYZPoint ray_pos, ref XYZPoint t)
        {
            if (!outside_sphere.find_cross(Camera_pos, ray_pos, ref t))
                return false;

            double dist = double.MaxValue;
            XYZPoint res = new XYZPoint();
            bool flag = false;

            foreach (var face in ph.Verges)
            {
                var p1 = face.Edges[0].First;
                var p2 = face.Edges[1].First;
                var p3 = face.Edges[2].First;
                Vector v1 = new Vector(p1, p2);
                Vector v2 = new Vector(p1, p3);
                Vector n = v1[v2];
                double d = -(n.X * p1.X + n.Y * p1.Y + n.Z * p1.Z);

                Vector v = new Vector(Camera_pos, ray_pos);
                Vector u = new Vector(Camera_pos);

                double denum = n * v;
                if (Math.Abs(denum) < eps)
                    continue;
                double num = n * u + d;
                double tp = -num / denum;
                if (tp < eps)
                    continue;
                t = new XYZPoint(
                    v.X * tp + u.X,
                    v.Y * tp + u.Y,
                    v.Z * tp + u.Z);

                double square = 0;
                int cnt = face.Edges.Count();
                for (int i = 0; i < cnt; ++i)
                    square += triangle_square(face.Edges[i].First, face.Edges[(i + 1) % cnt].First, t);
                if (Math.Abs(squares[face] - square) > eps)
                    continue;
                var dist_t = new Vector(Camera_pos, t).Norm();
                if (dist_t < dist)
                {
                    res = t.Clone() as XYZPoint;
                    dist = dist_t;
                    flag = true;
                }
            }

            t = res.Clone() as XYZPoint;
            return flag;
        }

        public override Vector normal(XYZPoint p)
        {
            foreach (var face in ph.Verges)
            {
                double square = 0;
                int cnt = face.Edges.Count();
                for (int i = 0; i < cnt; ++i)
                    square += triangle_square(face.Edges[i].First, face.Edges[(i + 1) % cnt].First, p);
                if (Math.Abs(squares[face] - square) > eps)
                    continue;

                var p1 = face.Edges[0].First;
                var p2 = face.Edges[1].First;
                var p3 = face.Edges[2].First;
                Vector v1 = new Vector(p1, p2);
                Vector v2 = new Vector(p1, p3);
                return v1[v2];
            }
            return new Vector();
        }
    }
}
