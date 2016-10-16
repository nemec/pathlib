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

        [Benchmark]
        public string NativePathCombine()
        {
            return Path.Combine(First, Second);
        }

        [Benchmark]
        public IPath PathLibPathCombine()
        {
            return FirstP.Join(Second);
        }
    }
}
