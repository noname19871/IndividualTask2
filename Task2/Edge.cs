using System.Drawing;

namespace IndividualTask2
{
    class Edge : Primitive
    {
        public Edge()
        {
            /*empty*/
        }

        public Edge(XYZPoint a, XYZPoint b)
        {
            this.a = new XYZPoint(a);
            this.b = new XYZPoint(b);
        }

        public XYZPoint First
        {
            get
            {
                return a;
            }
        }

        public XYZPoint Second
        {
            get
            {
                return b;
            }
        }

        public Primitive Clone()
        {
            return new Edge(a.Clone() as XYZPoint, b.Clone() as XYZPoint);
        }

        public void Draw(Graphics g, Color c)
        {
            Pen p = new Pen(c);
            g.DrawLine(p, a.To2d(), b.To2d());
            p.Dispose();
        }

        private XYZPoint a, b;
    }
}
