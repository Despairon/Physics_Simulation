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
using System.IO;

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

        private List<Vector3>               _vertices;
        private List<Vector2>               _texcoords;
        private List<Vector3>               _normals;
        private List<int[]>                 _faces;    
        private VAO                         _vao;
        private Transform                   _transform;
        private string                      _name;
        private Primitives_type             _primitives_type;
        private ShaderManager.ShaderProgram _shader;

        private static List<RenderObject> preloadedObjects = new List<RenderObject>();

        #endregion

        #region public_members

        public enum Primitives_type
        {
            POINTS         = Gl.GL_POINTS,
            LINE_STRIP     = Gl.GL_LINE_STRIP,
            LINE_LOOP      = Gl.GL_LINE_LOOP,
            LINES          = Gl.GL_LINES,
            TRIANGLE_STRIP = Gl.GL_TRIANGLE_STRIP,
            TRIANGLE_FAN   = Gl.GL_TRIANGLE_FAN,
            TRIANGLES      = Gl.GL_TRIANGLES,
            POLYGON        = Gl.GL_POLYGON
        }

        public string name
        {
            get         { return _name; }
            private set { _name = value; }
        }

        public Primitives_type primitives_type
        {
            get         { return _primitives_type; }
            private set { _primitives_type = value; }
        }

        public int[] indices
        {
            get
            {
                List<int> inds = new List<int>();

                foreach (var face in _faces)
                    inds.AddRange(face);
                return inds.ToArray();
            }
        }

        public RenderObject(string name, List<Vector3> vertices, List<Vector2> texcoords, List<Vector3> normals, List<int[]> faces, Primitives_type primitives_type)
        {
            this.name = name;

            _vertices  = vertices;
            _texcoords = texcoords;
            _normals   = normals;

            _faces = faces;

            this.primitives_type = primitives_type;

            _transform = new Transform(0);

            _shader = ShaderManager.getShader("Default");

            _vao = new VAO();
            _vao.bind();
            
            // TODO: try to find a different approach to this
            int vbo_index = 0;

            if ((_vertices != null) && (_vertices.Count != 0))
            {
                List<float> vertices_data = new List<float>();

                foreach (var vertex in _vertices)
                    vertices_data.AddRange(vertex.toFloat());

                _vao.add_VBO(vbo_index, VBO.BufferType.ARRAY_BUFFER, VBO.BufferDataType.FLOAT, vertices_data.ToArray(), vertices_data.Count, VBO.BufferElementsPerVertex.THREE_ELEMENTS);
                _vao.dataPointer(vbo_index, 0, IntPtr.Zero);
                
                vbo_index++;
            }

            if ((_texcoords != null) && (_texcoords.Count != 0))
            {
                List<float> texcoord_data = new List<float>();

                foreach (var texcoord in _texcoords)
                    texcoord_data.AddRange(texcoord.toFloat());

                _vao.add_VBO(vbo_index, VBO.BufferType.ARRAY_BUFFER, VBO.BufferDataType.FLOAT, texcoord_data.ToArray(), texcoord_data.Count, VBO.BufferElementsPerVertex.TWO_ELEMENTS);
                _vao.dataPointer(vbo_index, 0, IntPtr.Zero);

                vbo_index++;
            }

            if ((_normals != null) && (_normals.Count != 0))
            {
                List<float> normal_data = new List<float>();

                foreach (var normal in _normals)
                    normal_data.AddRange(normal.toFloat());

                _vao.add_VBO(vbo_index, VBO.BufferType.ARRAY_BUFFER, VBO.BufferDataType.FLOAT, normal_data.ToArray(), normal_data.Count, VBO.BufferElementsPerVertex.THREE_ELEMENTS);
                _vao.dataPointer(vbo_index, 0, IntPtr.Zero);

                vbo_index++;
            }
            
            if (_faces != null)
                _vao.add_VBO(vbo_index, VBO.BufferType.ELEMENT_ARRAY_BUFFER, VBO.BufferDataType.UINT, indices, indices.Length, VBO.BufferElementsPerVertex.ONE_ELEMENT);

            _vao.prepareIndicesFaces(faces);

            _vao.unbind();
        }

        public RenderObject getClone()
        {
            return new RenderObject(name, _vertices, _texcoords, _normals, _faces, primitives_type);
        }

        public void applyShader(ShaderManager.ShaderProgram shader)
        {
            if (shader != null)
                _shader = shader;
        }

        public void draw()
        {
            // TODO: apply textures in future!

            _shader.use();

            Gl.glPushMatrix();

            Gl.glTranslated(_transform.translation.x, _transform.translation.y, _transform.translation.z);

            Gl.glRotated(_transform.rotation.ox, 1, 0, 0);
            Gl.glRotated(_transform.rotation.oy, 0, 1, 0);
            Gl.glRotated(_transform.rotation.oz, 0, 0, 1);

            Gl.glScaled(_transform.scale.x, _transform.scale.y, _transform.scale.z );

            int proj_uni = _shader.getUniform("projection");
            int view_uni = _shader.getUniform("view");

            float[] projection = Render.getProjectionMatrix();
            float[] view = Render.getModelViewMatrix();

            Gl.glUniformMatrix4fv(proj_uni, 1, 0, projection);
            Gl.glUniformMatrix4fv(view_uni, 1, 0, view);

            _vao.draw(primitives_type);

            Gl.glPopMatrix();

            _shader.unuse();
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
                string name = objFile.name;

                List<Vector3> vertices  = new List<Vector3>();
                List<Vector2> texcoords = new List<Vector2>();
                List<Vector3> normals   = new List<Vector3>();
                List<int[]>   faces     = new List<int[]>();

                Primitives_type type;

                if (objFile.meshes.Exists(mesh => (mesh.faces.Exists(face => face.vertex_indices.Length > 3))))
                    type = Primitives_type.TRIANGLE_FAN;
                else
                    type = Primitives_type.TRIANGLES;

                foreach (var mesh in objFile.meshes)
                    foreach (var face in mesh.faces)
                    {
                        List<int> f = new List<int>();
                        foreach (var vertex_index in face.vertex_indices)
                            f.Add(vertex_index - 1);
                        faces.Add(f.ToArray());

                        foreach (var texcoord_index in face.texcoord_indices)
                        {
                            var texcoord = objFile.texcoords[texcoord_index - 1];
                            texcoords.Add(texcoord);
                        }

                        foreach (var normal_index in face.normal_indices)
                        {
                            var normal = objFile.normals[normal_index - 1];
                            normals.Add(normal);
                        }
                    }

                var obj = new RenderObject(name, objFile.vertices, texcoords, normals, faces, type);

                return obj;
            }           
        }

        public static void preloadObjects()
        {
            if (Directory.Exists("Objects"))
            {
                foreach (var file in Directory.GetFiles("Objects", "*.obj"))
                    preloadedObjects.Add(loadObjectFromFile(file));
            }
        }

        public static RenderObject getPreloadedObject(string name)
        {
            return preloadedObjects.Find(obj => obj.name == name).getClone();
        }

        #endregion
    }
}
