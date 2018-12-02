using System.Drawing;

namespace IndividualTask2
{
    class Polyhedron : Primitive
    {
        private Polyhedron()
        {

        }

        public Polyhedron(Verge[] _verges, XYZPoint _center)
        {
            verges = new Verge[_verges.Length];
            for (int i = 0; i < verges.Length; ++i)
            {
                verges[i] = _verges[i].Clone() as Verge;
            }
            center = _center.Clone() as XYZPoint;
        }

        private Verge[] verges;

        private XYZPoint center;

        public Verge[] Verges
        {
            get
            {
                return verges;
            }
        }

        public XYZPoint Center
        {
            get
            {
                return center;
            }
        }

        private bool colorized = false;

        public bool Colorized
        {
            get
            {
                return colorized;
            }
        }



        public static Polyhedron CreateHexahedron(XYZPoint a, XYZPoint b, XYZPoint q)
        {
            XYZPoint[] cd = GetVertecesForSquare(a, b, q);
            XYZPoint c = cd[0];
            XYZPoint d = cd[1];
            Vector m = new Vector(a, b);
            Vector n = new Vector(a, c);
            double sideLen = m.Norm();
            Vector h = m[n].Normalize() * sideLen;
            XYZPoint a1 = new XYZPoint(a.X + h.X, a.Y + h.Y, a.Z + h.Z);
            XYZPoint b1 = new XYZPoint(b.X + h.X, b.Y + h.Y, b.Z + h.Z);
            XYZPoint c1 = new XYZPoint(c.X + h.X, c.Y + h.Y, c.Z + h.Z);
            XYZPoint d1 = new XYZPoint(d.X + h.X, d.Y + h.Y, d.Z + h.Z);

            Polyhedron Hexahedron = new Polyhedron();
            Hexahedron.verges = new Verge[6];
            Hexahedron.verges[0] = Verge.CreateSquare(a, b, c, d);
            Hexahedron.verges[1] = Verge.CreateSquare(a, b, b1, a1);
            Hexahedron.verges[2] = Verge.CreateSquare(b, c, c1, b1);
            Hexahedron.verges[3] = Verge.CreateSquare(c, d, d1, c1);
            Hexahedron.verges[4] = Verge.CreateSquare(a, d, d1, a1);
            Hexahedron.verges[5] = Verge.CreateSquare(a1, b1, c1, d1);

            double cx = (a.X + c1.X) / 2;
            double cy = (a.Y + c1.Y) / 2;
            double cz = (a.Z + c1.Z) / 2;
            Hexahedron.center = new XYZPoint(cx, cy, cz);

            return Hexahedron;
        }

        private static XYZPoint[] GetVertecesForSquare(XYZPoint a, XYZPoint b, XYZPoint q)
        {
            Vector m = new Vector(a, b);
            Vector n = new Vector(a, q);
            double sideLen = m.Norm();

            double coeff = (m * n) / (m * m);
            Vector h = (n - (coeff * m)).Normalize() * sideLen;
            Vector r = m + h;
            XYZPoint c = new XYZPoint(a.X + r.X, a.Y + r.Y, a.Z + r.Z);
            XYZPoint d = new XYZPoint(a.X + h.X, a.Y + h.Y, a.Z + h.Z);
            return new XYZPoint[2] { c, d };
        }

        public Primitive Clone()
        {
            Polyhedron res = new Polyhedron();
            res.verges = new Verge[verges.Length];
            for (int i = 0; i < verges.Length; ++i)
            {
                res.verges[i] = verges[i].Clone() as Verge;
            }
            res.center = center.Clone() as XYZPoint;
            return res;
        }

        public void Draw(Graphics g, Color c)
        {
            foreach (var x in verges)
            {
                x.Draw(g, c);
            }
        }
    }
}
