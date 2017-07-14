using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System.Drawing;

namespace Physics_Simulation
{
    public struct Triangle
    {
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
    }

    public struct Transform
    {
        public struct Translation
        {
            public double x; public double y; public double z;
        }
        public struct Rotation
        {
            public double ox; public double oy; public double oz;
        }
        public struct Scale
        {
            public double x; public double y; public double z;
        }

        public Translation translation;
        public Rotation    rotation;
        public Scale       scale;

        public Transform (double default_value)
        {
            translation = new Translation();
            rotation    = new Rotation();
            scale       = new Scale();

            translation.x = default_value; translation.y = default_value; translation.z = default_value;
            rotation.ox   = default_value; rotation.oy   = default_value; rotation.oz   = default_value;
            scale.x = 1; scale.y = 1; scale.z = 1;
        }
    }

    public class RenderObject : IDrawable, ITransformable
    {
        #region private_members

        private List<Vector3>  _vertices;
        private Color          _color;
        private Transform      _transform;
        private string         _name;
        // private List<Triangle> _indices; // TODO: will be needed in shaders

        #endregion

        #region public_members

        public List<Vector3> vertices
        {
            get
            {
                return _vertices;
            }
            private set
            {
                _vertices = value;
            }
        }

        public Color color
        {
            get         { return _color;  }
            private set { _color = value; }
        }

        public string name
        {
            get         { return _name; }
            private set { _name = value; }
        }

        public RenderObject(string name, List<Vector3> vertices, Color color)
        {
            this.vertices = new List<Vector3>();

            if (vertices != null)
                this.vertices = vertices;

            this.color = color;

            _transform = new Transform(0);

            this.name = name;
        }

        public RenderObject(string name, List<Triangle> triangles, Color color)
        {
            vertices = new List<Vector3>();

            if (triangles != null)
                foreach (var triangle in triangles)
                {
                    vertices.Add(triangle.a);
                    vertices.Add(triangle.b);
                    vertices.Add(triangle.c);
                }

            this.color = color;

            _transform = new Transform(0);

            this.name = name;
        }

        public void draw()
        {
            Render.setColor(color);

            // TODO: maybe apply textures in future!

            Gl.glPushMatrix();

            Gl.glTranslated(_transform.translation.x, _transform.translation.y, _transform.translation.z);

            Gl.glRotated(_transform.rotation.ox, 1, 0, 0);
            Gl.glRotated(_transform.rotation.oy, 0, 1, 0);
            Gl.glRotated(_transform.rotation.oz, 0, 0, 1);

            Gl.glScaled(_transform.scale.x, _transform.scale.y, _transform.scale.z );

            Gl.glBegin(Gl.GL_TRIANGLES);
    
            foreach (var vertex in vertices)
                Gl.glVertex3f((float)vertex.x, (float)vertex.y, (float)vertex.z);

            Gl.glEnd();

            Gl.glPopMatrix();
        }

        public void translate(double x, double y, double z)
        {
            _transform.translation.x = x; _transform.translation.y = y; _transform.translation.z = z;
        }

        public void rotate(double x_angle, double y_angle, double z_angle)
        {
            x_angle = ExtendedMath.radiansToDegrees(x_angle);
            y_angle = ExtendedMath.radiansToDegrees(y_angle);
            z_angle = ExtendedMath.radiansToDegrees(z_angle);
            _transform.rotation.ox = x_angle; _transform.rotation.oy = y_angle; _transform.rotation.oz = z_angle;
        }

        public void scale(double x, double y, double z)
        {
            _transform.scale.x = x; _transform.scale.y = y; _transform.scale.z = z;
        }

        public static RenderObject loadObjectFromFile(string filename)
        {
            var objFile = ObjFileReader.read(filename);

            if (objFile == null)
                return null;
            else
            {
                return new RenderObject("", (List<Vector3>)null, Color.AliceBlue); // TODO: ...
            }
            
        }

        #endregion
    }
}
