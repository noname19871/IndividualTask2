namespace IndividualTask2
{
    class Camera
    {
        private Vector pos;
        private Vector view;
        private Vector hor;
        private Vector vert;

        public Camera(Vector pos, Vector view, Vector hor, Vector vert)
        {
            this.pos = pos;
            this.view = view;
            this.hor = hor;
            this.vert = vert;
        }

        public void SetCameraByPoints(XYZPoint o, XYZPoint i, XYZPoint j, XYZPoint k)
        {
            pos = new Vector(o);
            view = new Vector(o, i);
            hor = new Vector(o, j);
            vert = new Vector(o, k);
        }

        public Vector Pos
        {
            get
            {
                return pos;
            }
        }

        public Vector View
        {
            get
            {
                return view;
            }
        }

        public Vector Hor
        {
            get
            {
                return hor;
            }
        }

        public Vector Vert
        {
            get
            {
                return vert;
            }
        }
    }
}
