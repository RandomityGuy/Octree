using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Octree
{
    public enum OctreeNodeType
    {
        Point,
        Leaf,
        Node
    }

    public interface IOctreeNode
    {
        OctreeNodeType GetNodeType();
    }

    /// <summary>
    /// Wrapper class used to store points in the octree
    /// </summary>
    public class OctreePoint : IOctreeNode
    {
        /// <summary>
        /// The point stored in the octree point
        /// </summary>
        public Vector3 Point;

        public OctreePoint(Vector3 point)
        {
            Point = point;
        }

        /// <summary>
        /// Gets the type of the octree node this is
        /// </summary>
        /// <returns>OctreeNodeType.Point</returns>
        public OctreeNodeType GetNodeType()
        {
            return OctreeNodeType.Point;
        }
    }

    /// <summary>
    /// The nodes of an octree
    /// </summary>
    public class OctreeNode : IOctreeNode
    {
        internal OctreeNode[] Nodes;

        internal bool IsLeaf;

        internal bool IsEmpty
        {
            get
            {
                if (this.IsLeaf)
                    return this.Count == 0;
                else
                {
                    foreach (var node in Nodes)
                    {
                        if (!node.IsEmpty)
                            return false;
                    }
                    return true;
                }
            }
        }

        public Vector3 Center;

        internal BoundingBox Box
        {
            get
            {
                if (box == null)
                    GenerateBounds();
                return box;
            }
            set
            {
                box = value;
            }
        }

        BoundingBox box;

        /// <summary>
        /// If this is a octree, it gives the list of points in the leaf else null
        /// </summary>
        public List<OctreePoint> Points;

        /// <summary>
        /// Returns the amount of points stored in the node and its children
        /// </summary>
        public int Count
        {
            get
            {
                if (this.IsLeaf) return this.Points.Count;
                return this.Nodes.Sum(a => a.Count);
            }
        }

        internal OctreeNode()
        {
            IsLeaf = true;
            Points = new List<OctreePoint>();
        }

        internal bool Find(Vector3 pt)
        {
            if (IsLeaf)
            {
                return Points.Select(a => a.Point).Contains(pt);
            }
            else
            {
                var msk = 0;
                msk |= (pt.X - Center.X) < 0 ? 0b001 : 0;
                msk |= (pt.Y - Center.Y) < 0 ? 0b010 : 0;
                msk |= (pt.Z - Center.Z) < 0 ? 0b100 : 0;

                return Nodes[msk].Find(pt);
            }
        }

        internal bool Remove(Vector3 pt)
        {
            if (IsLeaf)
            {
                var ptexists = Points.Find(a => a.Point == pt);
                if (ptexists == null) return false;
                return Points.Remove(ptexists);
            }
            else
            {
                var msk = 0;
                msk |= (pt.X - Center.X) < 0 ? 0b001 : 0;
                msk |= (pt.Y - Center.Y) < 0 ? 0b010 : 0;
                msk |= (pt.Z - Center.Z) < 0 ? 0b100 : 0;

                var ret = Nodes[msk].Remove(pt);
                this.Merge();
                return ret;
            }
        }

        internal void Insert(Vector3 pt)
        {
            if (IsLeaf)
            {
                Points.Add(new OctreePoint(pt));
                SubDivide(createBounds: false);
            }
            else
            {
                var msk = 0;
                msk |= (pt.X - Center.X) < 0 ? 0b001 : 0;
                msk |= (pt.Y - Center.Y) < 0 ? 0b010 : 0;
                msk |= (pt.Z - Center.Z) < 0 ? 0b100 : 0;

                Nodes[msk].Insert(pt);
            }
        }

        internal void GenerateBounds()
        {
            if (IsLeaf)
            {
                var min = Vector3.Zero;
                var max = Vector3.Zero;

                foreach (var p in Points.Select(a => a.Point))
                {
                    if (p.X < min.X) min.X = p.X;
                    if (p.Y < min.Y) min.Y = p.Y;
                    if (p.Z < min.Z) min.Z = p.Z;

                    if (p.X > max.X) max.X = p.X;
                    if (p.Y > max.Y) max.Y = p.Y;
                    if (p.Z > max.Z) max.Z = p.Z;
                }
                Box = new BoundingBox(min, max);
            }
            else
            {
                var minx = this.Nodes.Min(x => x.Box.Min.X);
                var miny = this.Nodes.Min(x => x.Box.Min.Y);
                var minz = this.Nodes.Min(x => x.Box.Min.Z);
                var maxx = this.Nodes.Max(x => x.Box.Max.X);
                var maxy = this.Nodes.Max(x => x.Box.Max.Y);
                var maxz = this.Nodes.Max(x => x.Box.Max.Z);

                Box = new BoundingBox(new Vector3(minx, miny, minz), new Vector3(maxx, maxy, maxz));
            }
        }

        internal void SubDivide(int BinPoints = 8, bool createBounds = true)
        {
            Center = (Box.Min + Box.Max) / 2;

            // Check if we have to divide
            if (Points.Count > BinPoints)
            {
                IsLeaf = false;

                // We will regenerate bounds if specified
                if (createBounds)
                {
                    var min = Vector3.Zero;
                    var max = Vector3.Zero;

                    foreach (var p in Points.Select(a => a.Point))
                    {
                        if (p.X < min.X) min.X = p.X;
                        if (p.Y < min.Y) min.Y = p.Y;
                        if (p.Z < min.Z) min.Z = p.Z;

                        if (p.X > max.X) max.X = p.X;
                        if (p.Y > max.Y) max.Y = p.Y;
                        if (p.Z > max.Z) max.Z = p.Z;
                    }
                    Box = new BoundingBox(min, max);
                }

                var Size = Box.Max - Box.Min;

                // Prepare the child octree nodes
                Nodes = new OctreeNode[8];
                for (var i = 0; i < 8; i++)
                    Nodes[i] = new OctreeNode();

                // We generate the bounding boxes for each of them
                Nodes[0].Box = new BoundingBox(Center, Box.Max);
                Nodes[1].Box = new BoundingBox(Center - new Vector3(Size.X / 2, 0, 0), Box.Max - new Vector3(Size.X / 2, 0, 0));
                Nodes[2].Box = new BoundingBox(Center - new Vector3(0, Size.Y / 2, 0), Box.Max - new Vector3(0, Size.Y / 2, 0));
                Nodes[3].Box = new BoundingBox(Center - new Vector3(Size.X / 2, Size.Y / 2, 0), Box.Max - new Vector3(Size.X / 2, Size.Y / 2, 0));
                Nodes[4].Box = new BoundingBox(Center - new Vector3(0, 0, Size.Z / 2), Box.Max - new Vector3(0, 0, Size.Z / 2));
                Nodes[5].Box = new BoundingBox(Center - new Vector3(Size.X / 2, 0, Size.Z / 2), Box.Max - new Vector3(Size.X / 2, 0, Size.Z / 2));
                Nodes[6].Box = new BoundingBox(Center - new Vector3(0, Size.Y / 2, Size.Z / 2), Box.Max - new Vector3(0, Size.Y / 2, Size.Z / 2));
                Nodes[7].Box = new BoundingBox(Box.Min, Center);

                // Now we sort the points into the nodes
                foreach (var pt in Points.Select(a => a.Point))
                {
                    var msk = 0;
                    msk |= (pt.X - Center.X) < 0 ? 0b001 : 0;
                    msk |= (pt.Y - Center.Y) < 0 ? 0b010 : 0;
                    msk |= (pt.Z - Center.Z) < 0 ? 0b100 : 0;

                    if (!Nodes[msk].Points.Select(a => a.Point).Contains(pt))
                    {
                        Nodes[msk].Points.Add(new OctreePoint(pt));
                    }

                }

                Points = null;

                // Divide the child nodes
                foreach (var node in Nodes)
                {
                    node.SubDivide(BinPoints, false);
                }
            }
            else
            {
                IsLeaf = true;
            }
        }

        internal void Merge(int BinPoints = 8)
        {
            if (IsLeaf)
                return;
            else
            {
                
                if (IsEmpty)
                {
                    this.IsLeaf = true;
                    this.Nodes = null;
                    this.Points = new List<OctreePoint>();
                }
            }
        }

        public override string ToString()
        {
            return $"Octree: Count: {Count}, Bounding: {Box.ToString()}";
        }

        /// <summary>
        /// Gets the type of the node this is
        /// </summary>
        /// <returns></returns>
        public OctreeNodeType GetNodeType()
        {
            if (IsLeaf)
                return OctreeNodeType.Leaf;
            else
                return OctreeNodeType.Node;
        }
    }

    /// <summary>
    /// Octree implementation, with point insertion, deletion and KNN
    /// </summary>
    public class Octree
    {
        internal BoundingBox Box;

        internal OctreeNode Root;

        public Octree(List<Vector3> points, int BinPoints = 8)
        {
            var pos = points;

            var min = Vector3.Zero;
            var max = Vector3.Zero;

            // Generate the bounding box
            foreach (var p in pos)
            {
                if (p.X < min.X) min.X = p.X;
                if (p.Y < min.Y) min.Y = p.Y;
                if (p.Z < min.Z) min.Z = p.Z;

                if (p.X > max.X) max.X = p.X;
                if (p.Y > max.Y) max.Y = p.Y;
                if (p.Z > max.Z) max.Z = p.Z;
            }
            Box = new BoundingBox(min, max);

            Root = new OctreeNode();
            Root.Box = Box;

            // We use the insert method because its much faster doing this way
            foreach (var pt in points)
                Root.Insert(pt);

        }


        /// <summary>
        /// Check if the octree contains the given point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool Find(Vector3 pt)
        {
            return Root.Find(pt);
        }

        /// <summary>
        /// Remove the point from the octree, returns true if removal succeeded else false
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool Remove(Vector3 pt)
        {
            return Root.Remove(pt);
        }

        /// <summary>
        /// Inserts a point into the octree
        /// </summary>
        /// <param name="pt"></param>
        public void Insert(Vector3 pt)
        {
            Root.Insert(pt);
        }

        /// <summary>
        /// Does a k nearest nearest neighbour search on the octree for the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <param name="number">Maximum number of results to return</param>
        /// <returns></returns>
        public List<Vector3> KNN(Vector3 point, int number)
        {
            var queue = new PriorityQueue<IOctreeNode>();
            queue.Enqueue(Root, ((Root.Box.GetClosestPoint(point) - point).LengthSquared()));

            var L = new List<Vector3>();

            while (L.Count < number && queue.Count > 0)
            {
                var node = queue.Dequeue();

                switch (node.GetNodeType())
                {
                    case OctreeNodeType.Leaf:
                        var leaf = node as OctreeNode;
                        foreach (var pt in leaf.Points)
                        {
                            queue.Enqueue(pt, ((pt.Point - point).LengthSquared()));
                        }
                        break;

                    case OctreeNodeType.Point:
                        L.Add((node as OctreePoint).Point);
                        break;

                    case OctreeNodeType.Node:
                        var n = node as OctreeNode;
                        foreach (var subnode in n.Nodes)
                            queue.Enqueue(subnode, ((subnode.Box.GetClosestPoint(point) - point).LengthSquared()));
                        break;
                }
            }
            return L;
        }

        /// <summary>
        /// Does a radius search on the octree for the given point
        /// </summary>
        /// <param name="point">The given point</param>
        /// <param name="number">Maximum number of results to return</param>
        /// <param name="maximumDistance">The radius</param>
        /// <returns></returns>
        public List<Vector3> RadiusSearch(Vector3 point, int number, float maximumDistance)
        {
            var L = new List<Vector3>();

            var queue = new PriorityQueue<IOctreeNode>();

            var maxDistSq = maximumDistance * maximumDistance;

            var closestPoint = Root.Box.GetClosestPoint(point);
            var distSq = (closestPoint - point).LengthSquared();

            if (distSq > maximumDistance)
                return L;

            queue.Enqueue(Root,distSq);

            while (L.Count < number && queue.Count > 0)
            {
                var node = queue.Dequeue();

                switch (node.GetNodeType())
                {
                    case OctreeNodeType.Leaf:
                        var leaf = node as OctreeNode;
                        foreach (var pt in leaf.Points)
                        {
                            distSq = ((pt.Point - point).LengthSquared());
                            if (distSq < maxDistSq)
                                queue.Enqueue(pt, distSq);
                        }
                        break;

                    case OctreeNodeType.Point:
                        L.Add((node as OctreePoint).Point);
                        break;

                    case OctreeNodeType.Node:
                        var n = node as OctreeNode;
                        foreach (var subnode in n.Nodes)
                        {
                            closestPoint = subnode.Box.GetClosestPoint(point);
                            distSq = (closestPoint - point).LengthSquared();
                            if (distSq < maxDistSq)
                                queue.Enqueue(subnode,distSq);
                        }
                        break;
                }
            }
            return L;
        }

    }
}
