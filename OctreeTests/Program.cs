using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Octree;
using System.Timers;

namespace OctreeTests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Generate a list of points for benchmarking
            var l = new List<Vector3>();

            var r = new Random();

            for (var i = 0; i < 80000; i++)
            {
                l.Add(new Vector3(r.Next(0, 100), r.Next(0, 100), r.Next(0, 100)));
            }

            // Now create the octree
            Console.WriteLine("Generating Octree");
            var timestart = DateTime.Now;
            var octree = new Octree.Octree(l);
            var timeend = DateTime.Now;
            var difference = timeend.Subtract(timestart).TotalMilliseconds;
            Console.WriteLine($"Took {difference}ms");

            // Now we do a KNN search for k = 1
            Console.WriteLine("KNN Test: k = 1");
            timestart = DateTime.Now;
            for (var i = 0; i < 80000; i++)
            {
                var nn = octree.KNN(new Vector3(r.Next(0, 100), r.Next(0, 100), r.Next(0, 100)), 1);
            }
            timeend = DateTime.Now;
            difference = timeend.Subtract(timestart).TotalMilliseconds;
            Console.WriteLine($"Took {difference}ms");

            // For k = 5
            Console.WriteLine("KNN Test: k = 5");
            timestart = DateTime.Now;
            for (var i = 0; i < 80000; i++)
            {
                var nn = octree.KNN(new Vector3(r.Next(0, 100), r.Next(0, 100), r.Next(0, 100)), 5);
            }
            timeend = DateTime.Now;
            difference = timeend.Subtract(timestart).TotalMilliseconds;
            Console.WriteLine($"Took {difference}ms");

            Console.WriteLine("Deletion Test:");
            timestart = DateTime.Now;
            foreach (var pt in l)
            {
                octree.Remove(pt);
            }
            timeend = DateTime.Now;
            difference = timeend.Subtract(timestart).TotalMilliseconds;
            Console.WriteLine($"Took {difference}ms");

            Console.WriteLine("Insertion Test:");
            timestart = DateTime.Now;
            foreach (var pt in l)
            {
                octree.Insert(pt);
            }
            timeend = DateTime.Now;
            difference = timeend.Subtract(timestart).TotalMilliseconds;
            Console.WriteLine($"Took {difference}ms");

        }
    }
}
