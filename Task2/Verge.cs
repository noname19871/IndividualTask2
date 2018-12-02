using System.Drawing;

namespace IndividualTask2
{
    class Verge : Primitive
    {
        private Verge() { }

        public Verge(XYZPoint[] verteces)
        {
            edges = new Edge[verteces.Length];
            for (int i = 1; i < verteces.Length; i++)
            {
                edges[i - 1] = new Edge(verteces[i - 1], verteces[i]);
            }
            edges[verteces.Length - 1] = new Edge(verteces[verteces.Length - 1], verteces[0]);
        }

        private Edge[] edges;

        public Edge[] Edges
        {
            get
            {
                return edges;
            }
        }

        public byte Color;

        public static Verge CreateSquare(XYZPoint a, XYZPoint b, XYZPoint c, XYZPoint d)
        {
            Verge Square = new Verge();
            Square.edges = new Edge[4];

            Square.edges[0] = new Edge(a, b);
            Square.edges[1] = new Edge(b, c);
            Square.edges[2] = new Edge(c, d);
            Square.edges[3] = new Edge(d, a);

            return Square;
        }

        public XYZPoint[] GetPoints()
        {
            XYZPoint[] res = new XYZPoint[edges.Length];
            for (int i = 0; i < res.Length; ++i)
            {
                res[i] = edges[i].First;
            }
            return res;
        }

        public Primitive Clone()
        {
            Verge res = new Verge();
            res.edges = new Edge[edges.Length];
            for (int i = 0; i < edges.Length; ++i)
            {
                res.edges[i] = edges[i].Clone() as Edge;
            }
            res.Color = Color;
            return res;
        }

        public void Draw(Graphics g, Color c)
        {
            foreach (var x in Edges)
            {
                x.Draw(g, c);
            }
        }
    }
}
