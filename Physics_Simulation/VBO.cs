using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Physics_Simulation
{
    public class VBO
    {
        #region private_members

        private int     _id;
        private float[] _data;

        #endregion

        #region public_members

        public VBO(int attribute_index, int type, float[] data, int dimensions)
        {
            this.data = data;

            Gl.glGenBuffers(1, out _id);

            bind();

            Gl.glBufferData(type, (IntPtr)(data.Length), data, Gl.GL_STATIC_DRAW);

            Gl.glVertexAttribPointer(attribute_index, dimensions, Gl.GL_FLOAT, Gl.GL_FALSE, 0, 0);

            unbind();
        }

        public int id
        {
            get { return _id; }
            private set { _id = value; }
        }

        public float[] data
        {
            get { return _data; }
            private set { _data = value;}
        }

        public void bind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, id);
        }

        public void unbind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public void delete()
        {
            Gl.glDeleteBuffers(1, ref _id);
        }

        #endregion
    }
}
