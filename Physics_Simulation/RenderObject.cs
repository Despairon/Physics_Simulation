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

        List<int[]>                         _faces;
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

        public RenderObject(string name, VAO attributeList, List<int[]> faces, Primitives_type primitives_type)
        {
            this.name = name;

            _faces = faces;

            _vao = attributeList;

            this.primitives_type = primitives_type;

            _transform = new Transform(0);

            _shader = ShaderManager.getShader("Default");

            if (_vao != null)
            {
                _vao.bind();

                if (_faces != null)
                {
                    List<int> indices = new List<int>();

                    foreach (var face in faces)
                        indices.AddRange(face);

                    _vao.add_VBO(3, VBO.BufferType.ELEMENT_ARRAY_BUFFER, VBO.BufferDataType.UINT, indices.ToArray(), indices.ToArray().Length, VBO.BufferElementsPerVertex.ONE_ELEMENT);

                    _vao.prepareIndicesFaces(faces);
                }

                _vao.unbind();
            }
        }

        public RenderObject getClone()
        {
            return new RenderObject(_name, _vao, _faces, _primitives_type);
        }

        public void applyShader(ShaderManager.ShaderProgram shader)
        {
            if (shader != null)
                _shader = shader;
        }

        public void prepare()
        {
            _shader.use();
        }

        public void applyTransformations()
        {
            int transform_uni = _shader.getUniform("transform");

            var translate = ExtendedMath.translation_matrix(_transform.translation.x, _transform.translation.y, _transform.translation.z);
            var rotate    = ExtendedMath.rotation_matrix   (_transform.rotation.ox,   _transform.rotation.oy,   _transform.rotation.oz);
            var scale     = ExtendedMath.scale_matrix      (_transform.scale.x,       _transform.scale.y,       _transform.scale.z);

            var transform = (translate * rotate * scale).toFloat();

            Gl.glUniformMatrix4fv(transform_uni, 1, 1, transform);
        }

        public void draw()
        {
            _vao.draw(primitives_type);
        }

        public void complete()
        {
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

                // TODO: try to find a different approach...
                VAO vao = new VAO();

                vao.bind();

                if ((objFile.vertices != null) && (objFile.vertices.Count != 0))
                {
                    List<float> vertices_data = new List<float>();

                    foreach (var vertex in objFile.vertices)
                        vertices_data.AddRange(vertex.toFloat());

                    vao.add_VBO(0, VBO.BufferType.ARRAY_BUFFER, VBO.BufferDataType.FLOAT, vertices_data.ToArray(), vertices_data.Count, VBO.BufferElementsPerVertex.THREE_ELEMENTS);
                    vao.dataPointer(0, 0, IntPtr.Zero);
                }

                if ((texcoords != null) && (texcoords.Count != 0))
                {
                    List<float> texcoord_data = new List<float>();

                    foreach (var texcoord in texcoords)
                        texcoord_data.AddRange(texcoord.toFloat());

                    vao.add_VBO(1, VBO.BufferType.ARRAY_BUFFER, VBO.BufferDataType.FLOAT, texcoord_data.ToArray(), texcoord_data.Count, VBO.BufferElementsPerVertex.TWO_ELEMENTS);
                    vao.dataPointer(1, 0, IntPtr.Zero);
                }

                if ((normals != null) && (normals.Count != 0))
                {
                    List<float> normal_data = new List<float>();

                    foreach (var normal in normals)
                        normal_data.AddRange(normal.toFloat());

                    vao.add_VBO(2, VBO.BufferType.ARRAY_BUFFER, VBO.BufferDataType.FLOAT, normal_data.ToArray(), normal_data.Count, VBO.BufferElementsPerVertex.THREE_ELEMENTS);
                    vao.dataPointer(2, 0, IntPtr.Zero);
                }

                vao.unbind();

                var obj = new RenderObject(name, vao, faces, type);

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
