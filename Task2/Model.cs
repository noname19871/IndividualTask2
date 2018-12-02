using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndividualTask2
{
    class Model
    {
        virtual public bool find_cross(XYZPoint Camera_pos, XYZPoint ray_pos, ref XYZPoint t) { return true; }
        virtual public Vector normal(XYZPoint p) { return new Vector(); }

        protected const double eps = 1e-6;
    }
}
