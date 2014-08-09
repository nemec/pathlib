using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPathUnitTest
{
    public static class TestUtils
    {
        public static Tuple<string, string> CreateJunctionAndTarget(string baseDir)
        {
            var path = Path.Combine(baseDir, Guid.NewGuid().ToString());
            var junction = Path.Combine(baseDir, Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);
            JunctionPoint.Create(junction, path, true);

            return Tuple.Create(path, junction);
        }

        public static void DeleteJunctionAndTarget(string path)
        {
            var target = JunctionPoint.GetTarget(path);
            JunctionPoint.Delete(path);
            if (target != null)
            {
                File.Delete(target);
            }
        }

    }
}
