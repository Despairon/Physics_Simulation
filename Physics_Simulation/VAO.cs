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

        #endregion
    }
}
