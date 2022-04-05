using Xunit;
using System.Linq;

namespace PathLib.UnitTest
{
    public class PurePosixPathUnitTest
    {
        [Fact]
        public void CreatePath_WithEmptyPath_AllPartsAreNonNull()
        {
            var path = new PurePosixPath("");

            Assert.NotNull(path.Drive);
            Assert.NotNull(path.Root);
            Assert.NotNull(path.Dirname);
            Assert.NotNull(path.Basename);
            Assert.NotNull(path.Extension);
            Assert.Empty(path.Parts.Where(p => p is null));
        }

        [Fact]
        public void CreatePath_WithCurrentDir_CreatesRelativePathWithDirname()
        {
            // Arrange

            // Act
            var path = new PurePosixPath();

            // Assert
            Assert.Equal(".", path.Basename);
        }

        [Fact]
        public void CreatePath_WithFilename_StoresPathInFilename()
        {
            // Arrange

            // Act
            var path = new PurePosixPath("file.txt");

            // Assert
            Assert.Equal("file.txt", path.Filename);
        }

        [Fact]
        public void CreatePath_WithMultipleStringPaths_CombinesPaths()
        {
            // Arrange
            const string someInitialPath = "/home/dan";
            const string someSubPath = "music";

            // Act
            var path = new PurePosixPath(someInitialPath, someSubPath);

            // Assert
            Assert.Equal("/home/dan/music", path.ToPosix());
        }

        [Fact]
        public void CreatePath_WithMultipleAbsolutePaths_UsesLastPathAsAnchor()
        {
            // Arrange
            var paths = new[]{ "/home/dan", "/lib", "lib64" };
            const string expected = "/lib/lib64";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.Equal(expected, path.ToPosix());
        }

        [Fact]
        public void CreatePath_WithExtraSlashes_RemovesExtraSlashes()
        {
            // Arrange
            var paths = new[] { "foo//bar" };
            const string expected = "foo/bar";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.Equal(expected, path.ToPosix());
        }

        [Fact]
        public void CreatePath_WithExtraDots_RemovesExtraDots()
        {
            // Arrange
            var paths = new[] { "foo/./bar" };
            const string expected = "foo/bar";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.Equal(expected, path.ToPosix());
        }

        [Fact]
        public void CreatePath_WithExtraDoubleDots_KeepsDoubleDots()
        {
            // Arrange
            var paths = new[] { "foo/../bar" };
            const string expected = "foo/../bar";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.Equal(expected, path.ToPosix());
        }

        [Fact]
        public void CreatePath_WithLeadingDoubleSlash_KeepsDoubleSlashAsRoot()
        {
            // Arrange
            var paths = new[] { "//home/dan" };
            const string expected = "//";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.Equal(expected, path.Root);
        }

        [Fact]
        public void CreatePath_WithLeadingTripleSlash_CompressesLeadingSlashesToOne()
        {
            // Arrange
            var paths = new[] { "///home/dan" };
            const string expected = "/";

            // Act
            var path = new PurePosixPath(paths);

            // Assert
            Assert.Equal(expected, path.Root);
        }

        [Fact]
        public void PathEquality_WithSamePath_Equal()
        {
            // Arrange
            var first = new PurePosixPath("foo");
            var second = new PurePosixPath("foo");

            // Act
            var actual = first == second;

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void PathEquality_UsingPosixPaths_ComparesCaseSensitive()
        {
            // Arrange
            var first = new PurePosixPath("foo");
            var second = new PurePosixPath("FOO");

            // Act
            var actual = first == second;

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void PathCompare_ParentLessThanChild_ReturnsTrue()
        {
            // Arrange
            var parent = new PurePosixPath("/home");
            var child = new PurePosixPath("/home/dan");

            // Act
            var actual = parent < child;

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAbsolute_WithAbsolutePath_ReturnsTrue()
        {
            // Arrange
            var parent = new PurePosixPath("/home/tmp");

            // Act
            var actual = parent.IsAbsolute();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsAbsolute_WithRelativePath_ReturnsFalse()
        {
            // Arrange
            var parent = new PurePosixPath("home/tmp");

            // Act
            var actual = parent.IsAbsolute();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsAbsolute_WithRelativePath_ReturnsTrue()
        {
            // Arrange
            var parent = new PurePosixPath("music/songs");

            // Act
            var actual = parent.IsAbsolute();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void Addition_WithOtherPureNtPath_JoinsBoth()
        {
            var first = new PurePosixPath(@"/home/");
            var second = new PurePosixPath(@"dan");
            var expected = new PurePosixPath(@"/home/dan");

            var actual = first + second;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Addition_WithString_JoinsBoth()
        {
            var first = new PurePosixPath(@"/home/");
            const string second = @"dan";
            var expected = new PurePosixPath(@"/home/dan");

            var actual = first + second;

            Assert.Equal(expected, actual);
        }
    }
}
