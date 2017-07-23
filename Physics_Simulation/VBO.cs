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

        private int      _id;
        private VBO_Type _type;
        private float[]  _data;

        #endregion

        #region public_members

        public enum VBO_Elements_per_vertex
        {
            NULL = 0,
            ONE_ELEMENT,
            TWO_ELEMENTS,
            THREE_ELEMENTS
        }
        
        public enum VBO_Type
        {
            ARRAY_BUFFER         = Gl.GL_ARRAY_BUFFER,
            ELEMENT_ARRAY_BUFFER = Gl.GL_ELEMENT_ARRAY_BUFFER,
            PIXEL_PACK_BUFFER    = Gl.GL_PIXEL_PACK_BUFFER,
            PIXEL_UNPACK_BUFFER  = Gl.GL_PIXEL_UNPACK_BUFFER
        }

        public VBO(int attribute_index, VBO_Type type, float[] data, VBO_Elements_per_vertex elements_per_vertex)
        {
            this.data = data;

            this.type = type;

            Gl.glGenBuffers(1, out _id);

            bind();

            Gl.glBufferData((int)type, (IntPtr)(data.Length * sizeof(float)), data, Gl.GL_STATIC_DRAW);

            Gl.glVertexAttribPointer(attribute_index, (int)elements_per_vertex, Gl.GL_FLOAT, Gl.GL_FALSE, 0, IntPtr.Zero);

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

        public VBO_Type type
        {
            get         { return _type; }
            private set { _type = value; }
        }

        public void bind()
        {
            Gl.glBindBuffer((int)type, id);
        }

        public void unbind()
        {
            Gl.glBindBuffer((int)type, 0);
        }

        public void delete()
        {
            Gl.glDeleteBuffers(1, ref _id);
        }

        #endregion
    }
}
