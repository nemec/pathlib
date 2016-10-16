using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathLib.Benchmark
{
    public class PathPerformance
    {
        const string First = "C:\\Users\\nemec";

        static IPath FirstP = Paths.Create(First);

        const string Second = "Downloads";

        //[Benchmark]
        public string Native_PathCombine()
        {
            return Path.Combine(First, Second);
        }

        //[Benchmark]
        public IPath PathLib_PathCombine()
        {
            return FirstP.Join(Second);
        }

        const string WalkDirectory = @"C:\Users\dan\prg\Paperless";
        static readonly IPath WalkDirectoryP = Paths.Create(WalkDirectory);

        [Benchmark]
        public int Native_WalkDir()
        {
            var count = 0;
            var q = new Queue<string>();
            q.Enqueue(WalkDirectory);
            while (q.Count > 0)
            {
                var dir = q.Dequeue();
                try
                {
                    foreach (var entry in Directory.GetDirectories(dir))
                    {
                        ++count;
                        q.Enqueue(entry);
                    }

                    foreach (var entry in Directory.GetFiles(dir))
                    {
                        ++count;
                    }
                }
                catch(UnauthorizedAccessException) { }
            }
            return count;
        }

        [Benchmark]
        public int PathLib_WalkDir()
        {
            var count = 0;
            foreach(var entry in WalkDirectoryP.WalkDir())
            {
                ++count;
                foreach(var file in entry.Files)
                {
                    ++count;
                }
            }

            return count;
        }
    }
}
