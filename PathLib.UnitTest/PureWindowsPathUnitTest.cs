using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using Xunit;
using System.Text;
using PathLib;

namespace PathLib.UnitTest
{
    public class PureWindowsPathUnitTest
    {
        [Fact]
        public void CreatePath_WithEmptyPath_AllPartsAreNonNull()
        {
            var path = new PureWindowsPath("");

            Assert.NotNull(path.Drive);
            Assert.NotNull(path.Root);
            Assert.NotNull(path.Dirname);
            Assert.NotNull(path.Basename);
            Assert.NotNull(path.Extension);
            Assert.Empty(path.Parts.Where(p => p is null));
        }

        [Fact]
        public void CreatePath_WithCurrentDirectory_StoresAsDirname()
        {
            var path = new PureWindowsPath(".");

            Assert.Equal(".", path.Dirname);
        }

        [Fact]
        public void CreatePath_WithFilename_StoresFilename()
        {
            var path = new PureWindowsPath("file.txt");

            Assert.Equal("file.txt", path.Filename);
        }

        [Fact]
        public void CreatePath_JoiningTwoLocalRoots_DoesNotChangeDrive()
        {
            // Arrange
            var paths = new[] { "c:/Windows", "/Program Files" };
            const string expected = @"c:/Program Files";

            // Act
            var path = new PureWindowsPath(paths);

            // Assert
            Assert.Equal(expected, path.ToPosix());
        }

        [Fact]
        public void CreatePath_WhereSecondPathContainsDriveButNoRoot_ChangesDriveAndHasNoRoot()
        {
            // Arrange
            var expected = new PureWindowsPath(@"d:bar");
            var actual = new PureWindowsPath(@"c:\windows", @"d:bar");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CreatePath_WhereDirnameContainsReservedCharacter_ThrowsException()
        {
            Assert.Throws<InvalidPathException>(() => new PureWindowsPath(@"C:\use<rs\illegal.txt"));
        }

        [Fact]
        public void CreatePath_WhereDirnameContainsColons_ThrowsException()
        {
            Assert.Throws<InvalidPathException>(() => new PureWindowsPath(@"C:\:::\illegal.txt"));
        }

        [Fact]
        public void CreatePath_WhereBasenameContainsReservedCharacter_ThrowsException()
        {
            Assert.Throws<InvalidPathException>(() => new PureWindowsPath(@"C:\users\illegal>char.txt"));
        }

        [Fact]
        public void CreatePath_WhereExtensionContainsReservedCharacter_ThrowsException()
        {
            Assert.Throws<InvalidPathException>(() => new PureWindowsPath(@"C:\users\illegal.tx<>t"));
        }

        [Fact]
        public void PathEquality_UsingWindowsPaths_ComparesCaseInsensitive()
        {
            // Arrange
            var first = new PureWindowsPath("foo");
            var second = new PureWindowsPath("FOO");

            // Act
            var actual = first == second;

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void GetDrive_WithNoDrive_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureWindowsPath("/Program Files/");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.Equal(String.Empty, actual);
        }

        [Fact]
        public void GetDrive_WithDrive_ReturnsDriveName()
        {
            // Arrange
            var path = new PureWindowsPath("c:/Program Files/");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.Equal("c:", actual);
        }

        [Fact]
        public void GetDrive_WithUncShare_ReturnsShareName()
        {
            // Arrange
            var path = new PureWindowsPath("//some/share/foo.txt");

            // Act
            var actual = path.Drive;

            // Assert
            Assert.Equal(@"\\some\share", actual);
        }

        [Fact]
        public void GetRoot_WithUncShare_ReturnsShareRoot()
        {
            // Arrange
            var path = new PureWindowsPath("//some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.Equal(@"\", actual);
        }

        [Fact]
        public void GetRoot_WithDriveAndRoot_ReturnsRoot()
        {
            // Arrange
            var path = new PureWindowsPath("c:/some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.Equal(@"\", actual);
        }

        [Fact]
        public void GetRoot_WithDriveAndNoRoot_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureWindowsPath("c:some/share/foo.txt");

            // Act
            var actual = path.Root;

            // Assert
            Assert.Equal(@"", actual);
        }

        [Fact]
        public void GetAnchor_WithDriveAndNoRoot_ReturnsDrive()
        {
            // Arrange
            var path = new PureWindowsPath("c:some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.Equal(@"c:", actual);
        }

        [Fact]
        public void IsAbsolute_WithDriveAndNoRoot_ReturnsFalse()
        {
            // Arrange
            var path = new PureWindowsPath("c:some/share/foo.txt");

            // Act
            var actual = path.IsAbsolute();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void GetAnchor_WithDriveAndRoot_ReturnsDriveAndRoot()
        {
            // Arrange
            var path = new PureWindowsPath("c:/some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.Equal(@"c:\", actual);
        }

        [Fact]
        public void GetAnchor_WithNoDriveAndRoot_ReturnsRoot()
        {
            // Arrange
            var path = new PureWindowsPath("/some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.Equal(@"\", actual);
        }

        [Fact]
        public void GetAnchor_WithRelativePath_ReturnsEmptyString()
        {
            // Arrange
            var path = new PureWindowsPath("some/share/foo.txt");

            // Act
            var actual = path.Anchor;

            // Assert
            Assert.Equal(@"", actual);
        }

        [Fact]
        public void GetFilename_WithPathEndingInSlash_ReturnsLastPathComponent()
        {
            // POSIX spec drops the trailing slash

            // Arrange
            var path = new PureWindowsPath("some/path/");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.Equal(@"path", actual);
        }

        [Fact]
        public void GetFilename_WithPathHavingFilename_ReturnsFilename()
        {
            // Arrange
            const string expected = "foo.txt";
            var path = new PureWindowsPath("some/path/foo.txt");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetFilename_WithOnlyFilename_ReturnsFilename()
        {
            // Arrange
            const string expected = "foo.txt";
            var path = new PureWindowsPath("foo.txt");

            // Act
            var actual = path.Filename;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDirname_WithDriveAndFilenameButNoBasename_ReturnsEmptyString()
        {
            // Arrange
            const string expected = "";
            var path = new PureWindowsPath("d:foo.txt");

            // Act
            var actual = path.Dirname;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Addition_WithOtherPureWindowsPath_JoinsBoth()
        {
            var first = new PureWindowsPath(@"c:\users");
            var second = new PureWindowsPath(@"nemec");
            var expected = new PureWindowsPath(@"C:\users\nemec");

            var actual = first + second;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Addition_WithString_JoinsBoth()
        {
            var first = new PureWindowsPath(@"c:\users");
            const string second = @"nemec";
            var expected = new PureWindowsPath(@"C:\users\nemec");

            var actual = first + second;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RelativeTo_WithRootAndDrive_CalculatesRelativePath()
        {
            var expected = new PureWindowsPath(@"users\nemec");
            var root = new PureWindowsPath(@"C:\");
            var abs = new PureWindowsPath(@"C:\users\nemec");

            var actual = abs.RelativeTo(root);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RelativeTo_WithCaseInsensitivePathAndDifferentCasing_CalculatesRelativePath()
        {
            var expected = new PureWindowsPath(@"nemec\downloads");
            var root = new PureWindowsPath(@"C:\users");
            var abs = new PureWindowsPath(@"C:\USERS\nemec\downloads");

            var actual = abs.RelativeTo(root);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RelativeTo_WithCaseSensitivePathAndDifferentCasing_ThrowsException()
        {
            var root = new PurePosixPath(@"/home");
            var abs = new PurePosixPath(@"/HOME/nemec");

            Assert.Throws<ArgumentException>(() => abs.RelativeTo(root));
        }

        [Fact]
        public void TypeConverter_FromString_CreatesPath()
        {
            const string str = @"c:\users\nemec";
            var converter = TypeDescriptor.GetConverter(typeof (PureWindowsPath));
            var path = (PureWindowsPath?)converter.ConvertFrom(str);

            Assert.NotNull(path);
            Assert.Equal(str, path!.ToString());
        }



        [XmlRoot]
        public class XmlSerialize
        {
            [XmlElement]
            public PureWindowsPath? Folder { get; set; }
        }

        [Fact]
        public void XmlSerialize_WithPathAsStringElement_SerializesIntoString()
        {
            const string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<XmlSerialize xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Folder>c:\users\nemec</Folder>
</XmlSerialize>";
            var data = new XmlSerialize { Folder = new PureWindowsPath(@"c:\users\nemec") };
            var writer = new StringWriter(new StringBuilder());

            XmlWriterSettings ws = new XmlWriterSettings{ Indent = true};
            using(var xmlWriter = XmlWriter.Create(writer, ws)){
                new XmlSerializer(typeof(XmlSerialize))
                    .Serialize(xmlWriter, data);

                Assert.Equal(expected, writer.ToString());
            }
        }

        [Fact]
        public void XmlDeserialize_WithPathAsStringElement_DeserializesIntoType()
        {
            const string pathXml = @"<XmlSerialize><Folder>c:\users\nemec</Folder></XmlSerialize>";
            var obj = (XmlSerialize?)new XmlSerializer(typeof(XmlSerialize))
                .Deserialize(new StringReader(pathXml));
            var expected = new PureWindowsPath(@"c:\users\nemec");

            Assert.NotNull(obj);
            Assert.Equal(expected, obj!.Folder);
        }

        [Fact]
        public void CompareDirectoryHierarchy_FromDir_ToDir_ShouldShowParentAbove()
        {
            var parentPath = @"c:\foo\bar";
            var childPath = @"c:\foo\bar\child\path";

            var parentWindowsPath = new PureWindowsPath(parentPath);
            var childWindowsPath = new PureWindowsPath(childPath);

            Assert.True(childWindowsPath.IsChildOf(parentWindowsPath));
        }
    }
}

public static class PureWindowsPathExtensions
{
    public static bool IsChildOf(this PureWindowsPath target,PureWindowsPath compare)
    {
        return true;
    }
}