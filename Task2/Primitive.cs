using System.Drawing;

namespace IndividualTask2
{
    interface Primitive
    {
        Primitive Clone();

        void Draw(Graphics g, Color c);
    }
}
