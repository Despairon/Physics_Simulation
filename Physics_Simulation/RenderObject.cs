﻿using System;
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

        private List<Vector3>   _vertices;
        private List<Vector2>   _texcoords;
        private List<Vector3>   _normals;
        private VAO             _vao;
        private Color           _color;
        private Transform       _transform;
        private string          _name;
        private Primitives_type _type;

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

        public Primitives_type type
        {
            get         { return _type; }
            private set { _type = value; }
        }

        public RenderObject(string name, List<Vector3> vertices, List<Vector2> texcoords, List<Vector3> normals, Color color, Primitives_type type)
        {
            this.name = name;

            _vertices  = vertices;
            _texcoords = texcoords;
            _normals   = normals;

            this.color = color;

            this.type = type;

            _transform = new Transform(0);

            _vao = new VAO();
            _vao.bind();

            
            int vbo_index = 0;

            if (_vertices != null)
            {
                List<float> vertices_data = new List<float>();

                foreach (var vertex in _vertices)
                    vertices_data.AddRange(vertex.toFloat());

                VBO vertex_VBO = new VBO(vbo_index, VBO.VBO_Type.ARRAY_BUFFER, vertices_data.ToArray(), VBO.VBO_Elements_per_vertex.THREE_ELEMENTS);

                _vao.add_VBO(vertex_VBO);

                vbo_index++;
            }

            if (_texcoords != null)
            {
                List<float> texcoord_data = new List<float>();

                foreach (var texcoord in _texcoords)
                    texcoord_data.AddRange(texcoord.toFloat());

                VBO texcoords_VBO = new VBO(vbo_index, VBO.VBO_Type.ARRAY_BUFFER, texcoord_data.ToArray(), VBO.VBO_Elements_per_vertex.TWO_ELEMENTS);

                _vao.add_VBO(texcoords_VBO);

                vbo_index++;
            }

            if (_normals != null)
            {
                List<float> normal_data = new List<float>();

                foreach (var normal in _normals)
                    normal_data.AddRange(normal.toFloat());

                VBO normals_VBO = new VBO(vbo_index, VBO.VBO_Type.ARRAY_BUFFER, normal_data.ToArray(), VBO.VBO_Elements_per_vertex.THREE_ELEMENTS);

                _vao.add_VBO(normals_VBO);

                vbo_index++;
            }

            _vao.unbind();
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

            _vao.bind();
                _vao.enable(0);

                    Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, _vertices.Count);

                _vao.disable(0);
            _vao.unbind();

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
                string name = objFile.name;

                List<Vector3> vertices  = new List<Vector3>();
                List<Vector2> texcoords = new List<Vector2>();
                List<Vector3> normals   = new List<Vector3>();

                Primitives_type type = Primitives_type.TRIANGLES;

                if (objFile.meshes.Exists(mesh => (mesh.faces.Exists(face => face.vertex_indices.Length > 3))))
                    type = Primitives_type.POLYGON;
                else if (objFile.meshes.Exists(mesh => (mesh.faces.Exists(face => face.vertex_indices.Length == 3))))
                    type = Primitives_type.TRIANGLES;
                else
                    type = Primitives_type.POINTS;

                foreach (var mesh in objFile.meshes)
                    foreach (var face in mesh.faces)
                    {
                        foreach (var vertex_index in face.vertex_indices)
                        {
                            var vertex = objFile.vertices[vertex_index - 1];
                            vertices.Add(vertex);
                        }

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

                Random rnd = new Random(DateTime.Now.Millisecond);
                int r = (int)(rnd.NextDouble() * 0xFF);
                int g = (int)(rnd.NextDouble() * 0xFF);
                int b = (int)(rnd.NextDouble() * 0xFF);

                Color c = Color.FromArgb(r, g, b);

                var obj = new RenderObject(name, vertices, texcoords, normals, c, type);

                return obj;
            }
            
        }

        #endregion
    }
}
