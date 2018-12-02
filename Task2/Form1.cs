using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IndividualTask2
{
    public partial class Form1 : Form
    {
        Graphics g;

        Camera CreateCamera()
        {
            double pos_x = -10;
            double pos_y = -10;
            double pos_z = -30;

            double view_x = -2;
            double view_y = -8;
            double view_z = -3;

            double vert_angle = /*10 / 180.0 * Math.PI;*/ 0;

            Vector pos = new Vector(pos_x, pos_y, pos_z);
            Vector view = new Vector(view_x, view_y, view_z);
            view = view.Normalize();

            // вектор вертикали без учета угла наклона
            Vector vert;
            if (Math.Abs(view_x) < 1e-6 && Math.Abs(view_y) < 1e-6)
                vert = new Vector(-1, 0, 0);
            else
                vert = new Vector(0, 0, 1);
            double scalar_prod_vert_view = view * vert;
            vert = (vert - (view * scalar_prod_vert_view)).Normalize();

            // вектор горизонтали без учета угла наклона
            Vector hor = vert[view].Normalize();

            // учет угла наклона вертикали
            vert = (vert * Math.Cos(vert_angle) + hor * Math.Sin(vert_angle)).Normalize();
            hor = vert[view].Normalize();

            return new Camera(pos, view, -hor, -vert);
        }

        private Color light_color(Color clr, double light)
        {
            Color res = Color.FromArgb(clr.A, clr);
            if (light < 0)
                res = Color.Black;
            else if (light <= 1)
            {
                int red = (int)(ambient.R + (res.R - ambient.R) * light);
                int green = (int)(ambient.G + (res.G - ambient.G) * light);
                int blue = (int)(ambient.B + (res.B - ambient.B) * light);
                res = Color.FromArgb(red, green, blue);
            }
            else
            {
                int red = (int)(res.R + (255 - res.R) * (light - 1));
                if (red > 255)
                    red = 255;
                int green = (int)(res.G + (255 - res.G) * (light - 1));
                if (green > 255)
                    green = 255;
                int blue = (int)(res.B + (255 - res.B) * (light - 1));
                if (blue > 255)
                    blue = 255;
                res = Color.FromArgb(red, green, blue);
            }
            return res;
        }

        private List<XYZPoint> make_grid_corners(double dist)
        {
            var w = pictureBox1.Width / 2;
            var h = pictureBox1.Height / 2;
            Camera Camera = CreateCamera();

            Vector view = Camera.View * dist;
            XYZPoint m0 = new XYZPoint(
                Camera.Pos.X + view.X,
                Camera.Pos.Y + view.Y,
                Camera.Pos.Z + view.Z);

            Vector hor = Camera.Hor * w;
            Vector vert = Camera.Vert * h;
            XYZPoint p1 = new XYZPoint(
                m0.X - hor.X - vert.X,
                m0.Y - hor.Y - vert.Y,
                m0.Z - hor.Z - vert.Z);
            XYZPoint p2 = new XYZPoint(
                m0.X + hor.X - vert.X,
                m0.Y + hor.Y - vert.Y,
                m0.Z + hor.Z - vert.Z);
            XYZPoint p3 = new XYZPoint(
                m0.X - hor.X + vert.X,
                m0.Y - hor.Y + vert.Y,
                m0.Z - hor.Z + vert.Z);
            XYZPoint p4 = new XYZPoint(
                m0.X + hor.X + vert.X,
                m0.Y + hor.Y + vert.Y,
                m0.Z + hor.Z + vert.Z);

            List<XYZPoint> res = new List<XYZPoint>();
            res.Add(p1);
            res.Add(p2);
            res.Add(p3);
            res.Add(p4);
            return res;
        }

        List<Model> Models = new List<Model>();
        Dictionary<Model, Color> colors = new Dictionary<Model, Color>();
        Dictionary<Model, double> diffuse = new Dictionary<Model, double>();
        Dictionary<Model, double> reflect = new Dictionary<Model, double>();
        Dictionary<Model, double> trans = new Dictionary<Model, double>();
        Dictionary<Model, double> refract = new Dictionary<Model, double>();

        List<XYZPoint> lights = new List<XYZPoint>();
        Dictionary<XYZPoint, double> lights_power = new Dictionary<XYZPoint, double>();

        Color ambient = Color.Black;
        const double eps = 1e-6;
        const double amb_light = 0.3;

        private List<Sphere> find_spheres(List<Polyhedron> Models)
        {
            List<Sphere> spheres = new List<Sphere>();
            foreach (var obj in Models)
                spheres.Add(new Sphere(obj));
            return spheres;
        }

        private List<XYZPoint> find_rays()
        {
            var corners = make_grid_corners(pictureBox1.Width * Math.Sqrt(3) / 2); // было 100
            List<XYZPoint> points = new List<XYZPoint>();
            var w = pictureBox1.Width;
            var h = pictureBox1.Height;

            var step_h_x = (corners[2].X - corners[0].X) / h;
            var step_h_y = (corners[2].Y - corners[0].Y) / h;
            var step_h_z = (corners[2].Z - corners[0].Z) / h;

            var step_w_x = (corners[1].X - corners[0].X) / w;
            var step_w_y = (corners[1].Y - corners[0].Y) / w;
            var step_w_z = (corners[1].Z - corners[0].Z) / w;

            for (int i = 0; i < h; ++i)
            {
                var p = new XYZPoint(
                    corners[0].X + step_h_x * i,
                    corners[0].Y + step_h_y * i,
                    corners[0].Z + step_h_z * i);
                for (int j = 0; j < w; ++j)
                {
                    points.Add(new XYZPoint(
                        p.X + step_w_x * j,
                        p.Y + step_w_y * j,
                        p.Z + step_w_z * j));
                }
            }

            return points;
        }

        private Color add_colors(Color diff, Color refl, Color trans, bool is_refl, bool is_trans)
        {
            int r = diff.R, g = diff.G, b = diff.B;
            if (is_refl)
            {
                r += refl.R;
                g += refl.G;
                b += refl.B;
            }
            if (is_trans)
            {
                r += trans.R;
                g += trans.G;
                b += trans.B;
            }
            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;
            return Color.FromArgb(r, g, b);
        }

        private Color ray_step(XYZPoint start, XYZPoint p, double intense)
        {
            if (intense < 0.01)
                return Color.Black;

            double dist = double.MaxValue;
            Model obj = null;
            XYZPoint cross = new XYZPoint();
            foreach (var o in Models)
            {
                XYZPoint t = new XYZPoint();
                if (o.find_cross(start, p, ref t))
                {
                    double d1 = new Vector(start, t).Norm();
                    if (d1 < dist)
                    {
                        dist = d1;
                        obj = o;
                        cross = t;
                    }
                }
            }

            if (obj == null)
                return ambient;

            // диффузное освещение
            double ldiff = 0;
            XYZPoint tt = new XYZPoint();
            foreach (var l in lights)
            {
                bool flag = false;
                double trans_coeff = 1;
                foreach (var o in Models)
                {
                    if (o.find_cross(cross, l, ref tt) && new Vector(tt, cross).Norm() < new Vector(l, cross).Norm())
                    {
                        if (trans[o] > 0)
                            trans_coeff *= trans[o];
                        else
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                    continue;

                double kd = diffuse[obj];
                double l0 = lights_power[l];
                Vector N = obj.normal(cross);
                Vector L = new Vector(cross, l);
                if (N * L < 0)
                    N = -N;
                double cos = (N * L) / N.Norm() / L.Norm();
                ldiff += kd * cos * l0 * trans_coeff;
            }

            Color clr_diff = light_color(colors[obj], (ldiff + amb_light) * intense);

            // отражение
            Color clr_refl = Color.Black;
            if (reflect[obj] > 0)
            {
                Vector l = new Vector(start, p);
                l = l.Normalize();
                Vector n = obj.normal(cross);
                n = n.Normalize();
                if (n * l > 0)
                    n = -n;
                Vector r = l - 2 * n * (n * l);
                XYZPoint p_new = new XYZPoint(
                    cross.X + r.X,
                    cross.Y + r.Y,
                    cross.Z + r.Z);
                clr_refl = ray_step(cross, p_new, intense * reflect[obj]);
            }

            //преломление
            Color clr_trans = Color.Black;
            if (trans[obj] > 0)
            {
                Vector l = new Vector(start, p);
                l = l.Normalize();
                Vector n = obj.normal(cross);
                n = n.Normalize();
                if (n * l > 0)
                    n = -n;
                double coef = 1 / refract[obj];
                double cos = Math.Sqrt(1 - coef * coef * (1 - (n * l) * (n * l)));
                Vector t = coef * l - (cos + coef * (n * l)) * n;
                XYZPoint p_new = new XYZPoint(
                    cross.X + t.X,
                    cross.Y + t.Y,
                    cross.Z + t.Z);

                for (int i = 0; i < 10; ++i)
                {
                    if (!obj.find_cross(cross, p_new, ref tt))
                        return clr_diff;

                    l = new Vector(cross, p_new);
                    l = l.Normalize();
                    n = obj.normal(tt);
                    n.Normalize();
                    if (n * l > 0)
                        n = -n;
                    coef = refract[obj];
                    cos = 1 - coef * coef * (1 - (n * l) * (n * l));
                    if (cos < 0)
                    {
                        if (i == 9)
                        {
                            cross = tt;
                            p_new = new XYZPoint(
                                cross.X + l.X,
                                cross.Y + l.Y,
                                cross.Z + l.Z);
                            break;
                        }
                        Vector r = l - 2 * n * (n * l);
                        cross = tt;
                        p_new = new XYZPoint(
                            cross.X + r.X,
                            cross.Y + r.Y,
                            cross.Z + r.Z);
                        continue;
                    }
                    cos = Math.Sqrt(cos);
                    t = coef * l - (cos + coef * (n * l)) * n;
                    p_new = new XYZPoint(
                        tt.X + t.X,
                        tt.Y + t.Y,
                        tt.Z + t.Z);
                    break;
                }

                clr_trans = ray_step(tt, p_new, intense * trans[obj]);
            }

            return add_colors(clr_diff, clr_refl, clr_trans, reflect[obj] > 0, trans[obj] > 0);
        }

        private void Init()
        {
            Models.Clear();
            colors.Clear();
            diffuse.Clear();
            reflect.Clear();
            trans.Clear();
            refract.Clear();

            // Сферы
            Models.Add(new Sphere(new XYZPoint(-2, 1, -2.5), 1));
            colors.Add(Models.Last(), Color.White);
            diffuse.Add(Models.Last(), 0.8);
            reflect.Add(Models.Last(), 0.2);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1.5);

            Models.Add(new Sphere(new XYZPoint(2, 1, -2.5), 0.5));
            colors.Add(Models.Last(), Color.White);
            diffuse.Add(Models.Last(), 0.3);
            reflect.Add(Models.Last(), 0.7);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1.5);

            // Кубы
            Models.Add(new Cube(Polyhedron.CreateHexahedron(
                new XYZPoint(-3, -3, -5),
                new XYZPoint(-2, -3, -5),
                new XYZPoint(-3, -2, -5))));
            colors.Add(Models.Last(), Color.Black);
            diffuse.Add(Models.Last(), 0.8);
            reflect.Add(Models.Last(), 0.2);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1.5);

            Models.Add(new Cube(Polyhedron.CreateHexahedron(
                new XYZPoint(-1.5, -4, -5),
                new XYZPoint(0, -4, -5),
                new XYZPoint(-1.5, -2.5, -5))));
            colors.Add(Models.Last(), Color.Blue);
            diffuse.Add(Models.Last(), 0.1);
            reflect.Add(Models.Last(), 0);
            trans.Add(Models.Last(), 0.5);
            refract.Add(Models.Last(), 1);

            // Правая стена
            Models.Add(new Wall(
                new XYZPoint(-5, -5, -5),
                new XYZPoint(-5, -5, 5),
                new XYZPoint(-5, 5, 5),
                new XYZPoint(-5, 5, -5)));
            colors.Add(Models.Last(), Color.LawnGreen);
            diffuse.Add(Models.Last(), 1);
            reflect.Add(Models.Last(), 0);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1);

            // Левая стена
            Models.Add(new Wall(
                new XYZPoint(5, 5, -5),
                new XYZPoint(5, 5, 5),
                new XYZPoint(5, -5, 5),
                new XYZPoint(5, -5, -5)));
            colors.Add(Models.Last(), Color.Crimson);
            diffuse.Add(Models.Last(), 0);
            reflect.Add(Models.Last(), 1);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1);

            // Передняя стена
            Models.Add(new Wall(
                new XYZPoint(5, -5, -5),
                new XYZPoint(5, -5, 5),
                new XYZPoint(-5, -5, 5),
                new XYZPoint(-5, -5, -5)));
            colors.Add(Models.Last(), Color.Azure);
            diffuse.Add(Models.Last(), 0.5);
            reflect.Add(Models.Last(), 0.5);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1);


            // Пол
            Models.Add(new Wall(
               new XYZPoint(-5, -5, -5),
               new XYZPoint(5, -5, -5),
               new XYZPoint(5, 5, -5),
               new XYZPoint(-5, 5, -5)));
            colors.Add(Models.Last(), Color.White);
            diffuse.Add(Models.Last(), 0.8);
            reflect.Add(Models.Last(), 0.2);
            trans.Add(Models.Last(), 0);
            refract.Add(Models.Last(), 1);



            lights.Clear();
            lights_power.Clear();

            lights.Add(new XYZPoint(-4.9, -4.9, -4.9));
            lights_power.Add(lights.Last(), 1);

            lights.Add(new XYZPoint(0, -4.9, 0));
            lights_power.Add(lights.Last(), 1);
        }

        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();


        }
            


        private void DrawButton_Click(Object sender, EventArgs e)
        {
            Init();

            var points = find_rays();

            var Camera_pos = new XYZPoint(2,11,2);
            g.Clear(ambient);
            for (int i = 0; i < pictureBox1.Height; i += 1)
                for (int j = 0; j < pictureBox1.Width; j += 1)
                {
                    var clr = ray_step(Camera_pos, points[i * pictureBox1.Width + j], 1);
                    g.FillRectangle(new SolidBrush(clr), j, i, 1, 1);

                }
        }
    }
}
