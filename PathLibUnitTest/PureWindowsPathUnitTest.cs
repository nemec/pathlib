using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PathLib.UnitTest
{
    [TestClass]
    public class PureWindowsPathUnitTest
    {
        [TestMethod]
        public void CreatePath_WithEmptyPath_AllPartsAreNonNull()
        {
            var path = new PureWindowsPath("");

            Assert.IsNotNull(path.Drive);
            Assert.IsNotNull(path.Root);
            Assert.IsNotNull(path.Dirname);
            Assert.IsNotNull(path.Basename);
            Assert.IsNotNull(path.Extension);
            CollectionAssert.AllItemsAreNotNull(path.Parts.ToList());
        }

        [TestMethod]
        public void CreatePath_WithCurrentDirectory_StoresAsDirname()
        {
            var path = new PureWindowsPath(".");

            Assert.AreEqual(".", path.Dirname);
        }

        [TestMethod]
        public void CreatePath_WithFilename_StoresFilename()
        {
            var path = new PureWindowsPath("file.txt");

            Assert.AreEqual("file.txt", path.Filename);
        }

        [TestMethod]
        public void CreatePath_JoiningTwoLocalRoots_DoesNotChangeDrive()
        {
            // Arrange
            var paths = new[] { "c:/Windows", "/Program Files" };
            const string expected = @"c:/Program Files";

            // Act
            var path = new PureWindowsPath(paths);

            // Assert
            Assert.AreEqual(expected, path.ToPosix());
        }

        [TestMethod]
        public void CreatePath_WhereSecondPathContainsDriveButNoRoot_ChangesDriveAndHasNoRoot()
        {
            // Arrange
            var expected = new PureWindowsPath(@"d:bar");
            var actual = new PureWindowsPath(@"c:\windows", @"d:bar");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereDirnameContainsReservedCharacter_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            // ReSharper disable once ObjectCreationAsStatement
            new PureWindowsPath(@"C:\use<rs\illegal.txt");
            #pragma warning restore 168
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereDirnameContainsColons_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            // ReSharper disable once ObjectCreationAsStatement
            new PureWindowsPath(@"C:\:::\illegal.txt");
            #pragma warning restore 168
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereBasenameContainsReservedCharacter_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            // ReSharper disable once ObjectCreationAsStatement
            new PureWindowsPath(@"C:\users\illegal>char.txt");
            #pragma warning restore 168
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void CreatePath_WhereExtensionContainsReservedCharacter_ThrowsException()
        {
            // Arrange
            #pragma warning disable 168
            // ReSharper disable once ObjectCreationAsStatement
            new PureWindowsPath(@"C:\users\illegal.tx<>t");
            #pragma warning restore 168
        }

        [TestMethod]
        public void PathEquality_UsingWindowsPaths_ComparesCaseInsensitive()
        {
            // Arrange
            var first = new PureWindowsPath("foo");
            var second = new PureWindowsPath("FOO");

            // Act
            var actual = first == second;

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void GetDrive_WithNoDrive_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureWindowsPath("/Program Files/");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.AreEqual(String.Empty, actual);
        }

        [TestMethod]
        public void GetDrive_WithDrive_ReturnsDriveName()
        {
            // Arrange
            var path = new PureWindowsPath("c:/Program Files/");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.AreEqual("c:", actual);
        }

        [TestMethod]
        public void GetDrive_WithUncShare_ReturnsShareName()
        {
            // Arrange
            var path = new PureWindowsPath("//some/share/foo.txt");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.AreEqual(@"\\some\share", actual);
        }

        [TestMethod]
        public void GetRoot_WithUncShare_ReturnsShareRoot()
        {
            // Arrange
            var path = new PureWindowsPath("//some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.AreEqual(@"\", actual);
        }

        [TestMethod]
        public void GetRoot_WithDriveAndRoot_ReturnsRoot()
        {
            // Arrange
            var path = new PureWindowsPath("c:/some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.AreEqual(@"\", actual);
        }

        [TestMethod]
        public void GetRoot_WithDriveAndNoRoot_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureWindowsPath("c:some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.AreEqual(@"", actual);
        }

        [TestMethod]
        public void GetAnchor_WithDriveAndNoRoot_ReturnsDrive()
        {
            // Arrange
            var path = new PureWindowsPath("c:some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"c:", actual);
        }

        [TestMethod]
        public void IsAbsolute_WithDriveAndNoRoot_ReturnsFalse()
        {
            // Arrange
            var path = new PureWindowsPath("c:some/share/foo.txt");

            // Act
            var actual = path.IsAbsolute();

            // Assert
            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        public void GetAnchor_WithDriveAndRoot_ReturnsDriveAndRoot()
        {
            // Arrange
            var path = new PureWindowsPath("c:/some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"c:\", actual);
        }

        [TestMethod]
        public void GetAnchor_WithNoDriveAndRoot_ReturnsRoot()
        {
            // Arrange
            var path = new PureWindowsPath("/some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"\", actual);
        }

        [TestMethod]
        public void GetAnchor_WithRelativePath_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureWindowsPath("some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.AreEqual(@"", actual);
        }

        [TestMethod]
        public void GetFilename_WithPathEndingInSlash_ReturnsLastPathComponent()
        {
            // POSIX spec drops the trailing slash

            // Arrange
            var path = new PureWindowsPath("some/path/");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.AreEqual(@"path", actual);
        }

        [TestMethod]
        public void GetFilename_WithPathHavingFilename_ReturnsFilename()
        {
            // Arrange
            const string expected = "foo.txt";
            var path = new PureWindowsPath("some/path/foo.txt");

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
            var path = new PureWindowsPath("foo.txt");

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
            var path = new PureWindowsPath("d:foo.txt");

            // Act
            var actual = path.Dirname;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Addition_WithOtherPureWindowsPath_JoinsBoth()
        {
            var first = new PureWindowsPath(@"c:\users");
            var second = new PureWindowsPath(@"nemec");
            var expected = new PureWindowsPath(@"C:\users\nemec");

            var actual = first + second;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Addition_WithString_JoinsBoth()
        {
            var first = new PureWindowsPath(@"c:\users");
            const string second = @"nemec";
            var expected = new PureWindowsPath(@"C:\users\nemec");

            var actual = first + second;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithRootAndDrive_CalculatesRelativePath()
        {
            var expected = new PureWindowsPath(@"users\nemec");
            var root = new PureWindowsPath(@"C:\");
            var abs = new PureWindowsPath(@"C:\users\nemec");

            var actual = abs.RelativeTo(root);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithCaseInsensitivePathAndDifferentCasing_CalculatesRelativePath()
        {
            var expected = new PureWindowsPath(@"nemec\downloads");
            var root = new PureWindowsPath(@"C:\users");
            var abs = new PureWindowsPath(@"C:\USERS\nemec\downloads");

            var actual = abs.RelativeTo(root);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithCaseSensitivePathAndDifferentCasing_ThrowsException()
        {
            var root = new PurePosixPath(@"/home");
            var abs = new PurePosixPath(@"/HOME/nemec");

            abs.RelativeTo(root);
        }

        [TestMethod]
        public void TypeConverter_FromString_CreatesPath()
        {
            const string str = @"c:\users\nemec";
            var converter = TypeDescriptor.GetConverter(typeof (PureWindowsPath));
            var path = (PureWindowsPath)converter.ConvertFrom(str);

            Assert.IsNotNull(path);
            Assert.AreEqual(str, path.ToString());
        }



        [XmlRoot]
        public class XmlDeserialize
        {
            [XmlElement]
            public PureWindowsPath Folder { get; set; }
        }

        [TestMethod]
        public void XmlDeserialize_WithPathAsStringElement_DeserializesIntoType()
        {
            const string pathXml = @"<XmlDeserialize><Folder>c:\users\nemec</Folder></XmlDeserialize>";
            var obj = (XmlDeserialize)new XmlSerializer(typeof(XmlDeserialize))
                .Deserialize(new StringReader(pathXml));
            var expected = new PureWindowsPath(@"c:\users\nemec");

            Assert.AreEqual(expected, obj.Folder);
        }
    }
}
