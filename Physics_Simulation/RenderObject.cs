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

    public class RenderObject : IDrawable
    {
        #region private_members

        private List<Vector3>  _vertices;
        private Color          _color;
        // private List<Triangle> _indices; // TODO: will need in shaders

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

        public RenderObject(List<Vector3> vertices, Color color)
        {
            this.vertices = new List<Vector3>();

            if (vertices != null)
                this.vertices = vertices;

            this.color = color;
        }

        public RenderObject(List<Triangle> triangles, Color color)
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
        }

        public void draw()
        {
            Render.setColor(color);

            // TODO: maybe apply textures in future!

            Gl.glPushMatrix();

            // TODO: apply matrix transformations here!

            Gl.glBegin(Gl.GL_TRIANGLES);
    
            foreach (var vertex in vertices)
                Gl.glVertex3f((float)vertex.x, (float)vertex.y, (float)vertex.z);

            Gl.glEnd();

            Gl.glPopMatrix();
        }

        #endregion
    }
}
