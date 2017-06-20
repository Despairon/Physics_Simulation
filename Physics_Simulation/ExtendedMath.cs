using System;

namespace Physics_Simulation
{
    public struct Vector3
    {
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double x;
        public double y;
        public double z;

        // Vector product
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            Vector3 result = new Vector3();

            result.x = (left.y * right.z) - (left.z * right.y);
            result.y = (left.z * right.x) - (left.x * right.z);
            result.z = (left.x * right.y) - (left.y * right.x);

            return result;
        }

        public static Vector3 operator *(Vector3 left, double right)
        {
            left.x *= right;
            left.y *= right;
            left.z *= right;

            return left;
        }

        public static Vector3 operator *(double left, Vector3 right)
        {
            return (right * left);
        }

        public static Vector3 operator /(Vector3 left, double right)
        {
            if (right != 0)
            {
                left.x /= right;
                left.y /= right;
                left.z /= right;
            }

            return left;
        }

        public static Vector3 operator /(double left, Vector3 right)
        {
            return (right / left);
        }

        // Dot product
        public static double operator ^(Vector3 left, Vector3 right)
        {
            double result = (left.x * right.x) + (left.y * right.y) + (left.z * right.z);

            return result;
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            Vector3 result = new Vector3();

            result.x = left.x - right.x;
            result.y = left.y - right.y;
            result.z = left.z - right.z;

            return result;
        }

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            Vector3 result = new Vector3();

            result.x = left.x + right.x;
            result.y = left.y + right.y;
            result.z = left.z + right.z;

            return result;
        }

        public void translate(double x, double y, double z)
        {
            double[,] translation = new double[4, 4]
            {
                { 1, 0, 0, x },
                { 0, 1, 0, y },
                { 0, 0, 0, z },
                { 0, 0, 0, 1 }
            };

            Matrix4 translationMatrix = new Matrix4(translation);

            Vector4 resultVector = new Vector4(this.x, this.y, this.z, 1);

            resultVector *= translationMatrix;

            this = resultVector.vector3;
        }

        public void rotate(double ox, double oy, double oz)
        {
            double[,] rotationOX;
            double[,] rotationOY;
            double[,] rotationOZ;

            rotationOX = new double[4, 4]
            {
                { 1, 0,                      0,            0 },
                { 0, Math.Cos(ox),           Math.Sin(ox), 0 },
                { 0, Math.Sin(ox) * (-1.0f), Math.Cos(ox), 0 },
                { 0, 0,                      0,            1 }
            };

            rotationOY = new double[4, 4]
            {
                { Math.Cos(oy),           0, Math.Sin(oy), 0 },
                { 0,                      1, 0,            0 },
                { Math.Sin(oy) * (-1.0f), 0, Math.Cos(oy), 0 },
                { 0,                      0, 0,            1 }
            };

            rotationOZ = new double[4, 4]
            {
                { Math.Cos(oz),           Math.Sin(oz), 0, 0 },
                { Math.Sin(oz) * (-1.0f), Math.Cos(oz), 0, 0 },
                { 0,                      0,            1, 0 },
                { 0,                      0,            0, 1 }
            };

            Matrix4 rotationMatOX = new Matrix4(rotationOX);
            Matrix4 rotationMatOY = new Matrix4(rotationOY);
            Matrix4 rotationMatOZ = new Matrix4(rotationOZ);

            Vector4 resultVector = new Vector4(this.x, this.y, this.z, 1);

            resultVector *= rotationMatOX;
            resultVector *= rotationMatOY;
            resultVector *= rotationMatOZ;
            // TODO: complete implementation. Looks like we should use LOCAL COORDS in calculations here!!!
            this = resultVector.vector3;
        }

        public void scale(double x, double y, double z)
        {
            double[,] scaling = new double[4, 4]
            {
                { x, 0, 0, 0 },
                { 0, y, 0, 0 },
                { 0, 0, z, 0 },
                { 0, 0, 0, 1 }
            };

            Matrix4 scaleMatrix = new Matrix4(scaling);

            Vector4 resultVector = new Vector4(this.x, this.y, this.z, 1);

            resultVector *= scaleMatrix;

            this = resultVector.vector3;
        }

        public double getLength()
        {
            return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2) + Math.Pow(z,2));
        }

        public void normalize()
        {
            double length = getLength();
            if (length != 0)
            {
                this.x /= length;
                this.y /= length;
                this.z /= length;
            }
        }
    }

    public struct Vector4
    {
        public Vector4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public double x;
        public double y;
        public double z;
        public double w;

        public Vector3 vector3
        {
            get
            {
                return new Vector3(x,y,z);
            }
        }
    }

    public struct Matrix4
    {
        public Matrix4(double[,] values)
        {
            this.values = new double[4, 4];

            if (values.Length == this.values.Length)
                this.values = values;
            else return;
        }

        private double[,] values;

        public double this[int x_index, int y_index]
        {
            get
            {
                return values[x_index, y_index];
            }
            set
            {
                values[x_index, y_index] = value;
            }
        }

        public static Matrix4 operator *(Matrix4 left, Matrix4 right)
        {
            double[,] result = new double[4, 4];

            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    double sum = 0;
                    for (int z = 0; z < 4; z++)
                            sum += (left[x,z]*right[z,y]);
                    result[x,y] = sum;
                }

            return new Matrix4(result);
        }

        public static Vector4 operator *(Matrix4 left, Vector4 right)
        {
            Vector4 resultVector = new Vector4();

            resultVector.x = (left[0, 0] * right.x) + (left[0, 1] * right.y) + (left[0, 2] * right.z) + (left[0, 3] * right.w);
            resultVector.y = (left[1, 0] * right.x) + (left[1, 1] * right.y) + (left[1, 2] * right.z) + (left[1, 3] * right.w);
            resultVector.z = (left[2, 0] * right.x) + (left[2, 1] * right.y) + (left[2, 2] * right.z) + (left[2, 3] * right.w);
            resultVector.w = (left[3, 0] * right.x) + (left[3, 1] * right.y) + (left[3, 2] * right.z) + (left[3, 3] * right.w);

            return resultVector;
        }

        public static Vector4 operator *(Vector4 left, Matrix4 right)
        {
            return (right * left);
        }
    }

    public static class ExtendedMath
    {
        public static double degreesToRadians(double degrees)
        {
            return ( (degrees / 360.0f) * (Math.PI * 2.0f) );
        }

        public static double radiansToDegrees(double radians)
        {
            return ( (radians / Math.PI * 2.0f) * 360.0f );
        }

        public static Vector3 translated_vector(Vector3 vector, Vector3 direction, double distance)
        {
            vector.x = vector.x + ( (direction.x - vector.x) * distance);
            vector.y = vector.y + ( (direction.y - vector.y) * distance);
            vector.z = vector.z + ( (direction.z - vector.z) * distance);

            return vector;
        }

        public static Vector3 rotated_vector(Vector3 vector, Vector3 vectorStart, double xAxisAngle, double yAxisAngle)
        {
            vector.x = vectorStart.x + Math.Cos(xAxisAngle);
            vector.y = vectorStart.y + Math.Cos(yAxisAngle);
            vector.z = vectorStart.z + Math.Sin(xAxisAngle);

            return vector;
        }
    }
}
