using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PathLib.UnitTest
{
    [TestClass]
    public class PureNtPathUnitTest
    {
        [TestMethod]
        public void CreatePath_WithEmptyPath_AllPartsAreNonNull()
        {
            var path = new PureNtPath("");

            Assert.IsNotNull(path.Drive);
            Assert.IsNotNull(path.Root);
            Assert.IsNotNull(path.Dirname);
            Assert.IsNotNull(path.Basename);
            Assert.IsNotNull(path.Extension);
            CollectionAssert.AllItemsAreNotNull(path.Parts.ToList());
        }
        [TestMethod]
        public void CreatePath_WithFilename_StoresFilename()
        {
            var path = new PureNtPath(".");

            Assert.AreEqual(".", path.Filename);
        }

        [TestMethod]
        public void CreatePath_JoiningTwoLocalRoots_DoesNotChangeDrive()
        {
            // Arrange
            var paths = new[] { "c:/Windows", "/Program Files" };
            const string expected = @"c:/Program Files";

            // Act
            var path = new PureNtPath(paths);

            // Assert
            Assert.AreEqual(expected, path.AsPosix());
        }

        [TestMethod]
        public void CreatePath_WhereSecondPathContainsDriveButNoRoot_ChangesDriveAndHasNoRoot()
        {
            // Arrange
            var expected = new PureNtPath(@"d:bar");
            var actual = new PureNtPath(@"c:\windows", @"d:bar");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereDirnameContainsReservedCharacter_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            var expected = new PureNtPath(@"C:\use<rs\illegal.txt");
            #pragma warning restore 168
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereBasenameContainsReservedCharacter_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            var expected = new PureNtPath(@"C:\users\illegal>char.txt");
            #pragma warning restore 168
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereExtensionContainsReservedCharacter_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            var expected = new PureNtPath(@"C:\users\illegal.tx<>t");
            #pragma warning restore 168
        }

        [TestMethod]
        public void PathEquality_UsingNtPaths_ComparesCaseInsensitive()
        {
            // Arrange
            var first = new PureNtPath("foo");
            var second = new PureNtPath("FOO");

            // Act
            var actual = first == second;

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void GetDrive_WithNoDrive_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureNtPath("/Program Files/");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.AreEqual(String.Empty, actual);
        }

        [TestMethod]
        public void GetDrive_WithDrive_ReturnsDriveName()
        {
            // Arrange
            var path = new PureNtPath("c:/Program Files/");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.AreEqual("c:", actual);
        }

        [TestMethod]
        public void GetDrive_WithUncShare_ReturnsShareName()
        {
            // Arrange
            var path = new PureNtPath("//some/share/foo.txt");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.AreEqual(@"\\some\share", actual);
        }

        [TestMethod]
        public void GetRoot_WithUncShare_ReturnsShareRoot()
        {
            // Arrange
            var path = new PureNtPath("//some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.AreEqual(@"\", actual);
        }

        [TestMethod]
        public void GetRoot_WithDriveAndRoot_ReturnsRoot()
        {
            // Arrange
            var path = new PureNtPath("c:/some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.AreEqual(@"\", actual);
        }

        [TestMethod]
        public void GetRoot_WithDriveAndNoRoot_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureNtPath("c:some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.AreEqual(@"", actual);
        }

        [TestMethod]
        public void GetAnchor_WithDriveAndNoRoot_ReturnsDrive()
        {
            // Arrange
            var path = new PureNtPath("c:some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"c:", actual);
        }

        [TestMethod]
        public void IsAbsolute_WithDriveAndNoRoot_ReturnsFalse()
        {
            // Arrange
            var path = new PureNtPath("c:some/share/foo.txt");

            // Act
            var actual = path.IsAbsolute();

            // Assert
            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        public void GetAnchor_WithDriveAndRoot_ReturnsDriveAndRoot()
        {
            // Arrange
            var path = new PureNtPath("c:/some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"c:\", actual);
        }

        [TestMethod]
        public void GetAnchor_WithNoDriveAndRoot_ReturnsRoot()
        {
            // Arrange
            var path = new PureNtPath("/some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"\", actual);
        }

        [TestMethod]
        public void GetAnchor_WithRelativePath_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureNtPath("some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"", actual);
        }

        [TestMethod]
        public void GetFilename_WithPathEndingInSlash_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureNtPath("some/path/");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.AreEqual(@"", actual);
        }

        [TestMethod]
        public void GetFilename_WithPathHavingFilename_ReturnsFilename()
        {
            // Arrange
            const string expected = "foo.txt";
            var path = new PureNtPath("some/path/foo.txt");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetFilename_WithOnlyFilename_ReturnsFilename()
        {
            // Arrange
            const string expected = "foo.txt";
            var path = new PureNtPath("foo.txt");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirname_WithDriveAndFilenameButNoBasename_ReturnsEmptyString()
        {
            // Arrange
            const string expected = "";
            var path = new PureNtPath("d:foo.txt");

            // Act
            var actual = path.Dirname;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Addition_WithOtherPureNtPath_JoinsBoth()
        {
            var first = new PureNtPath(@"c:\users");
            var second = new PureNtPath(@"nemec");
            var expected = new PureNtPath(@"C:\users\nemec");

            var actual = first + second;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Addition_WithString_JoinsBoth()
        {
            var first = new PureNtPath(@"c:\users");
            const string second = @"nemec";
            var expected = new PureNtPath(@"C:\users\nemec");

            var actual = first + second;

            Assert.AreEqual(expected, actual);
        }
    }
}
