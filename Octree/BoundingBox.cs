using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Octree
{
    public class BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public Vector3 GetClosestPoint(Vector3 point)
        {
            var closest = new Vector3();
            if (Min.X > point.X)
                closest.X = Min.X;
            else if (Max.X < point.X)
                closest.X = Max.X;
            else
                closest.X = point.X;

            if (Min.Y > point.Y)
                closest.Y = Min.Y;
            else if (Max.Y < point.Y)
                closest.Y = Max.Y;
            else
                closest.Y = point.Y;

            if (Min.Z > point.Z)
                closest.Z = Min.Z;
            else if (Max.Z < point.Z)
                closest.Z = Max.Z;
            else
                closest.Z = point.Z;

            return closest;
        }

        public override string ToString()
        {
            return $"Min: {Min.ToString()}, Max: {Max.ToString()}";
        }
    }
}
