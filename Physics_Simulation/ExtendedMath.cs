using System;

namespace Physics_Simulation
{
    public struct Vector2
    {
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x;
        public double y;

        // Vector product
        public static double operator *(Vector2 left, Vector2 right)
        {
            double result = (left.x * right.y) - (left.y * right.x);

            return result;
        }

        // Dot product
        public static double operator ^(Vector2 left, Vector2 right)
        {
            double result = (left.x * right.x) + (left.y * right.y);

            return result;
        }

        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            Vector2 result = new Vector2();

            result.x = left.x - right.x;
            result.y = left.y - right.y;

            return result;
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            Vector2 result = new Vector2();

            result.x = left.x + right.x;
            result.y = left.y + right.y;

            return result;
        }
    }

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

        public static Vector3 rotated_vector(Vector3 vector, Vector3 vectorStart, double xAxisDegrees, double yAxisDegrees)
        {
            double alpha = degreesToRadians(xAxisDegrees);
            double beta  = degreesToRadians(yAxisDegrees);

            vector.x = vectorStart.x + Math.Cos(alpha);
            vector.y = vectorStart.y + Math.Cos(beta);
            vector.z = vectorStart.z + Math.Sin(alpha);

            return vector;
        }
    }
}
