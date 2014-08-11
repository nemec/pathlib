using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathLib;

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
            
            var path = new NtPath(junction);
            
            Assert.IsTrue(path.IsJunction());
        }

        [TestMethod]
        public void SetCurrentDirectory_WithDirectory_SetsEnvironmentVariable()
        {
            const string newCwd = @"C:\";
            var path = new NtPath(newCwd);
            using (path.SetCurrentDirectory())
            {
                Assert.AreEqual(newCwd, Environment.CurrentDirectory);
            }
        }

        [TestMethod]
        public void SetCurrentDirectory_UponDispose_RestoresEnvironmentVariable()
        {
            var oldCwd = Environment.CurrentDirectory;
            var path = new NtPath(@"C:\");
            var tmp = path.SetCurrentDirectory();
            
            tmp.Dispose();

            Assert.AreEqual(oldCwd, Environment.CurrentDirectory);
        }

        [TestMethod]
        public void ExpandUser_WithHomeDir_ExpandsDir()
        {
            var path = new NtPath("~/tmp");
            var expected = new NtPath(
                Environment.GetEnvironmentVariable("USERPROFILE") + @"\tmp");

            var actual = path.ExpandUser();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Join_WithAnotherPath_ReturnsNtPath()
        {
            var path = new NtPath(@"C:\tmp");
            var other = new NtPath(@"C:\tmp");

            object final = path.Join(other);

            // Prevent accidentally regressing code...
            #pragma warning disable 183
            Assert.IsTrue(final is NtPath);
            #pragma warning restore 183
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Directory.Delete(TempFolder, true);
        }
    }
}
