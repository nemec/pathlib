using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathLib;
using Path = System.IO.Path;

namespace WindowsPathUnitTest
{
    [TestClass]
    public class WindowsPathTests
    {
        private static string TempFolder { get; set; }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            do
            {
                TempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            } while (Directory.Exists(TempFolder));
            Directory.CreateDirectory(TempFolder);
        }

        [TestMethod]
        public void IsJunction_WithJunction_ReturnsTrue()
        {
            var ret = TestUtils.CreateJunctionAndTarget(TempFolder);
            var junction = ret.Item2;
            
            var path = new WindowsPath(junction);
            
            Assert.IsTrue(path.IsJunction());
        }

        [TestMethod]
        public void SetCurrentDirectory_WithDirectory_SetsEnvironmentVariable()
        {
            const string newCwd = @"C:\";
            var path = new WindowsPath(newCwd);
            using (path.SetCurrentDirectory())
            {
                Assert.AreEqual(newCwd, Environment.CurrentDirectory);
            }
        }

        [TestMethod]
        public void SetCurrentDirectory_UponDispose_RestoresEnvironmentVariable()
        {
            var oldCwd = Environment.CurrentDirectory;
            var path = new WindowsPath(@"C:\");
            var tmp = path.SetCurrentDirectory();
            
            tmp.Dispose();

            Assert.AreEqual(oldCwd, Environment.CurrentDirectory);
        }

        [TestMethod]
        public void ExpandUser_WithHomeDir_ExpandsDir()
        {
            var path = new WindowsPath("~/tmp");
            var expected = new WindowsPath(
                Environment.GetEnvironmentVariable("USERPROFILE"), "tmp");

            var actual = path.ExpandUser();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExpandUser_WithCustomHomeDirString_ExpandsDir()
        {
            var homeDir = new WindowsPath(@"C:\users\test");
            var path = new WindowsPath("~/tmp");
            var expected = homeDir.Join("tmp");

            var actual = path.ExpandUser(homeDir);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JoinIPath_WithAnotherPath_ReturnsWindowsPath()
        {
            IPath path = new WindowsPath(@"C:\tmp");
            IPath other = new WindowsPath(@"C:\tmp");

            var final = path.Join(other);

            Assert.IsTrue(final is WindowsPath);
        }

        // TODO FileInfo with nonexistant file
        // TODO FileInfo with directory
        // TODO FileInfo with file

        // TODO DirectoryInfo with nonexistant directory
        // TODO DirectoryInfo with directory
        // TODO DirectoryInfo with file

        // TODO delete file
        // TODO delete directory
        // TODO delete recursive

        // TODO expand environment variables

        [ClassCleanup]
        public static void Cleanup()
        {
            Directory.Delete(TempFolder, true);
        }
    }
}
