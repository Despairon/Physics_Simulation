using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Physics_Simulation
{
    public class VAO
    {
        #region private_members

        private int       _id;
        private List<VBO> _vbo_list;
        private int[]     _face_indices_count = null;
        private IntPtr[]  _face_indices_offset = null;

        #endregion

        #region public_members

        // due to bad TAO Framework implementation, 
        // I must create dll functions on my own for VAOs 
        private delegate void GL_GenVertexArrays(int count, out int id);
        private delegate void GL_BindVertexArray(int id);

        private GL_GenVertexArrays    glGenVertexArrays    = (GL_GenVertexArrays)    Gl.GetDelegate("glGenVertexArrays",    typeof(GL_GenVertexArrays));
        private GL_BindVertexArray    glBindVertexArray    = (GL_BindVertexArray)    Gl.GetDelegate("glBindVertexArray",    typeof(GL_BindVertexArray));

        public VAO()
        {
            glGenVertexArrays(1, out _id);

            _vbo_list = new List<VBO>();
        }

        public int id
        {
            get         { return _id;  }
            private set { _id = value; }
        }

        public void add_VBO(VBO vbo)
        {
            int max_attribs;
            Gl.glGetIntegerv(Gl.GL_MAX_VERTEX_ATTRIBS, out max_attribs);

            if ((_vbo_list.Count < max_attribs) && (vbo != null))
                _vbo_list.Add(vbo);
        }

        public void add_VBO(int attribute_index, VBO.BufferType type, VBO.BufferDataType dataType, object data, int dataSize, VBO.BufferElementsPerVertex elements_per_vertex)
        {
            var vbo = new VBO(attribute_index, type, dataType, data, dataSize, elements_per_vertex);

            add_VBO(vbo);
        }

        public void bind()
        {
            glBindVertexArray(_id);
        }

        public void unbind()
        {
            glBindVertexArray(0);
        }

        public void enable(int index)
        {
            Gl.glEnableVertexAttribArray(index);
        }

        public void disable(int index)
        {
            Gl.glDisableVertexAttribArray(index);
        }

        public void dataPointer(int attribute_index, int isNormalized, IntPtr pointer)
        {
            var vbo = _vbo_list.Find(buffer => buffer.attribute_index == attribute_index);
            if (vbo != null)
            {
                vbo.bind();
                    Gl.glVertexAttribPointer(vbo.attribute_index, (int)vbo.elements_per_vertex, (int)vbo.bufferDataType, isNormalized, 0, pointer);
                vbo.unbind();
            }
        }

        public void prepareIndicesFaces(List<int[]> faces_indices)
        {
            _face_indices_count  = new int[faces_indices.Count];
            _face_indices_offset = new IntPtr[faces_indices.Count];

            int currentOffset = 0;

            int index_type_sz = 0;

            var ibo = _vbo_list.Find(buffer => buffer.type == VBO.BufferType.ELEMENT_ARRAY_BUFFER);

            switch (ibo.bufferDataType)
            {
                case VBO.BufferDataType.UBYTE:  index_type_sz = sizeof(byte);   break;
                case VBO.BufferDataType.UINT:   index_type_sz = sizeof(uint);   break;
                case VBO.BufferDataType.USHORT: index_type_sz = sizeof(ushort); break;

                default: break;
            }

            for (int i = 0; i < faces_indices.Count; i++)
            {
                _face_indices_count[i] = faces_indices[i].Length;
                _face_indices_offset[i] = new IntPtr(currentOffset);
                currentOffset += index_type_sz * _face_indices_count[i];
            }
        }

        public void draw(RenderObject.Primitives_type primitives_type)
        {
            bind();

                foreach (var vbo in _vbo_list)
                    enable(vbo.attribute_index);
            
                var ibo = _vbo_list.Find(buffer => buffer.type == VBO.BufferType.ELEMENT_ARRAY_BUFFER);

                ibo.bind();

                    if ( (_face_indices_count != null) && (_face_indices_offset != null) )
                        Gl.glMultiDrawElements((int)primitives_type, _face_indices_count, (int)ibo.bufferDataType, _face_indices_offset, _face_indices_count.Length);
                    else
                        Gl.glDrawElements((int)primitives_type, ibo.dataLength, (int)ibo.bufferDataType, IntPtr.Zero);

                ibo.unbind();

                foreach (var vbo in _vbo_list)
                    disable(vbo.attribute_index);

            unbind();
        }

        #endregion
    }
}
