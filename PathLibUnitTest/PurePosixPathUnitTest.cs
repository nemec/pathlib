using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace PathLib.UnitTest
{
    [TestClass]
    public class PurePosixPathUnitTest
    {
        [TestMethod]
        public void CreatePath_WithEmptyPath_AllPartsAreNonNull()
        {
            var path = new PurePosixPath("");

            Assert.IsNotNull(path.Drive);
            Assert.IsNotNull(path.Root);
            Assert.IsNotNull(path.Dirname);
            Assert.IsNotNull(path.Basename);
            Assert.IsNotNull(path.Extension);
            CollectionAssert.AllItemsAreNotNull(path.Parts.ToList());
        }

        [TestMethod]
        public void CreatePath_WithFilename_CreatesRelativePathWithFilename()
        {
            // Arrange

            // Act
            var path = new PurePosixPath();

            // Assert
            Assert.AreEqual(".", path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WithMultipleStringPaths_CombinesPaths()
        {
            // Arrange
            const string someInitialPath = "/home/dan";
            const string someSubPath = "music";

            // Act
            var path = new PurePosixPath(someInitialPath, someSubPath);

            // Assert
            Assert.AreEqual("/home/dan/music", path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WithMultipleAbsolutePaths_UsesLastPathAsAnchor()
        {
            // Arrange
            var paths = new[]{ "/home/dan", "/lib", "lib64" };
            const string expected = "/lib/lib64";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.AreEqual(expected, path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WithExtraSlashes_RemovesExtraSlashes()
        {
            // Arrange
            var paths = new[] { "foo//bar" };
            const string expected = "foo/bar";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.AreEqual(expected, path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WithExtraDots_RemovesExtraDots()
        {
            // Arrange
            var paths = new[] { "foo/./bar" };
            const string expected = "foo/bar";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.AreEqual(expected, path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WithExtraDoubleDots_KeepsDoubleDots()
        {
            // Arrange
            var paths = new[] { "foo/../bar" };
            const string expected = "foo/../bar";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.AreEqual(expected, path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WithLeadingDoubleSlash_KeepsDoubleSlashAsRoot()
        {
            // Arrange
            var paths = new[] { "//home/dan" };
            const string expected = "//";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void CreatePath_WithLeadingTripleSlash_CompressesLeadingSlashesToOne()
        {
            // Arrange
            var paths = new[] { "///home/dan" };
            const string expected = "/";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void PathEquality_WithSamePath_AreEqual()
        {
            // Arrange
            var first = new PurePosixPath("foo");
            var second = new PurePosixPath("foo");

            // Act
            var actual = first == second;

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void PathEquality_UsingPosixPaths_ComparesCaseSensitive()
        {
            // Arrange
            var first = new PurePosixPath("foo");
            var second = new PurePosixPath("FOO");

            // Act
            var actual = first == second;

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void PathCompare_ParentLessThanChild_ReturnsTrue()
        {
            // Arrange
            var parent = new PurePosixPath("/home");
            var child = new PurePosixPath("/home/dan");

            // Act
            var actual = parent < child;

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsAbsolute_WithAbsolutePath_ReturnsTrue()
        {
            // Arrange
            var parent = new PurePosixPath("/home/tmp");

            // Act
            var actual = parent.IsAbsolute();

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsAbsolute_WithRelativePath_ReturnsTrue()
        {
            // Arrange
            var parent = new PurePosixPath("music/songs");

            // Act
            var actual = parent.IsAbsolute();

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Addition_WithOtherPureNtPath_JoinsBoth()
        {
            var first = new PurePosixPath(@"/home/");
            var second = new PurePosixPath(@"dan");
            var expected = new PurePosixPath(@"/home/dan");

            var actual = first + second;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Addition_WithString_JoinsBoth()
        {
            var first = new PurePosixPath(@"/home/");
            const string second = @"dan";
            var expected = new PurePosixPath(@"/home/dan");

            var actual = first + second;

            Assert.AreEqual(expected, actual);
        }
    }
}
