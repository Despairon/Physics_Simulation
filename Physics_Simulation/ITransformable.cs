using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Simulation
{
    public interface ITransformable
    {
        void translate(double x, double y, double z);
        void rotate(double x_angle, double y_angle, double z_angle);
        void scale(double x, double y, double z);
    }
}
