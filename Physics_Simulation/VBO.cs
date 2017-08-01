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

        private int                     _attribute_index;
        private int                     _id;
        private BufferType              _type;
        private BufferElementsPerVertex _elements_per_vertex;
        private BufferDataType          _bufferDataType;
        private object                  _data;
        private int                     _dataLength;

        #endregion

        #region public_members

        public enum BufferElementsPerVertex
        {
            NULL = 0,
            ONE_ELEMENT,
            TWO_ELEMENTS,
            THREE_ELEMENTS
        }

        public enum BufferDataType
        {
            UBYTE  = Gl.GL_UNSIGNED_BYTE,
            USHORT = Gl.GL_UNSIGNED_SHORT,
            UINT   = Gl.GL_UNSIGNED_INT,
            FLOAT  = Gl.GL_FLOAT
        }

        public enum BufferType
        {
            ARRAY_BUFFER         = Gl.GL_ARRAY_BUFFER,
            ELEMENT_ARRAY_BUFFER = Gl.GL_ELEMENT_ARRAY_BUFFER,
            PIXEL_PACK_BUFFER    = Gl.GL_PIXEL_PACK_BUFFER,
            PIXEL_UNPACK_BUFFER  = Gl.GL_PIXEL_UNPACK_BUFFER
        }

        public VBO(int attribute_index, BufferType type, BufferDataType bufferDataType, object data, int dataLength, BufferElementsPerVertex elements_per_vertex)
        {
            this.attribute_index = attribute_index;

            this.elements_per_vertex = elements_per_vertex;

            this.data = data;

            this.type = type;

            this.bufferDataType = bufferDataType;

            this.dataLength = dataLength;

            Gl.glGenBuffers(1, out _id);

            bind();

            int dataByteSize = 0;

            switch (bufferDataType)
            {
                case BufferDataType.UBYTE:  dataByteSize = sizeof(byte);   break;
                case BufferDataType.USHORT: dataByteSize = sizeof(ushort); break;
                case BufferDataType.UINT:   dataByteSize = sizeof(uint);   break;
                case BufferDataType.FLOAT:  dataByteSize = sizeof(float);  break;

                default: break;
            }

            Gl.glBufferData((int)type, (IntPtr)(dataByteSize * dataLength), data, Gl.GL_STATIC_DRAW);

            unbind();
        }

        public int attribute_index
        {
            get         { return _attribute_index;  }
            private set { _attribute_index = value; }
        }

        public int id
        {
            get         { return _id;  }
            private set { _id = value; }
        }

        public object data
        {
            get         { return _data;  }
            private set { _data = value; }
        }

        public int dataLength
        {
            get         { return _dataLength;  }
            private set { _dataLength = value; }
        }

        public BufferType type
        {
            get         { return _type;  }
            private set { _type = value; }
        }

        public BufferDataType bufferDataType
        {
            get         { return _bufferDataType;  }
            private set { _bufferDataType = value; }
        }

        public BufferElementsPerVertex elements_per_vertex
        {
            get         { return _elements_per_vertex;  }
            private set { _elements_per_vertex = value; }
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
