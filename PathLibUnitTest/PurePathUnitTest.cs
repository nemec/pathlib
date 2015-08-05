using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PathLib.UnitTest
{
    [TestClass]
    public class PurePathUnitTest
    {
        // TODO NT Paths can have a drive or a root

        // TODO POSIX path drive always empty

        private class MockParser : IPathParser
        {
            private readonly char[] _reservedCharacters =
            {
                '<', '>', '|'
            };

            private string PathSeparator { get; set; }

            private readonly PrivateType _pathUtils = new PrivateType(
                "pathlib", "PathLib.Utils.PathUtils");

            public MockParser(string pathSeparator)
            {
                PathSeparator = pathSeparator;
            }

            public string ParseDrive(string path)
            {
                return !String.IsNullOrEmpty(path)
                    ? ((string)_pathUtils.InvokeStatic(
                        "GetPathRoot", 
                        new object[]{path, PathSeparator})).TrimEnd(PathSeparator[0])
                    : null;
            }

            public string ParseRoot(string path)
            {
                if (String.IsNullOrEmpty(path))
                {
                    return null;
                }

                var root = ((string) _pathUtils.InvokeStatic(
                    "GetPathRoot",
                    new object[] {path, PathSeparator}));
                if (root.StartsWith(PathSeparator))
                {
                    return PathSeparator;
                }
                return root.EndsWith(PathSeparator)
                           ? PathSeparator
                           : null;
            }

            public string ParseDirname(string remainingPath)
            {
                return ((string) _pathUtils.InvokeStatic(
                    "GetDirectoryName",
                    new object[]{remainingPath, PathSeparator})) ?? "";
            }

            public string ParseBasename(string remainingPath)
            {
                var currentDirectoryIdentifier = ((string) _pathUtils.GetStaticField("CurrentDirectoryIdentifier"));
                return !String.IsNullOrEmpty(remainingPath)
                    ? remainingPath != currentDirectoryIdentifier
                        ? ((string) _pathUtils.InvokeStatic(
                            "GetFileNameWithoutExtension",
                            new object[]{remainingPath, PathSeparator}))
                            : currentDirectoryIdentifier
                    : null;
            }

            public string ParseExtension(string remainingPath)
            {
                return !String.IsNullOrEmpty(remainingPath)
                    ? ((string) _pathUtils.InvokeStatic(
                        "GetExtension",
                        new object[]{remainingPath, PathSeparator}))
                    : null;
            }

            public bool ReservedCharactersInPath(string path, out char reservedCharacter)
            {
                foreach (var ch in _reservedCharacters)
                {
                    if (path.IndexOf(ch) >= 0)
                    {
                        reservedCharacter = ch;
                        return true;
                    }
                }
                reservedCharacter = default(char);
                return false;
            }
        }

        private class MockPath : PurePath<MockPath>
        {
            public MockPath(params string[] paths)
                : base(new MockParser(@"\"), paths)
            { }

            public MockPath(string drive, string root, string dirname, string basename, string extension)
                : base(drive, root, dirname, basename, extension)
            { }

            protected override string PathSeparator
            {
                get { return @"\"; }
            }

            public override bool Equals(object otherObj)
            {
                var other = otherObj as MockPath;
                if (other == null)
                {
                    return false;
                }
                return Drive == other.Drive &&
                       Root == other.Root &&
                       Dirname == other.Dirname &&
                       Basename == other.Basename &&
                       Extension == other.Extension;
            }

            public override int GetHashCode()
            {
                return (Drive ?? "").GetHashCode() +
                       (Root ?? "").GetHashCode() +
                       (Dirname ?? "").GetHashCode() +
                       (Basename ?? "").GetHashCode() +
                       (Extension ?? "").GetHashCode();
            }

            public override bool IsReserved()
            {
                throw new NotImplementedException(
                    "A mock should not be used to test this.");
            }

            public override bool Match(string pattern)
            {
                throw new NotImplementedException(
                    "A mock should not be used to test this.");
            }

            public override MockPath NormCase(CultureInfo currentCulture)
            {
                throw new NotImplementedException(
                    "A mock should not be used to test this.");
            }

            protected override MockPath PurePathFactory(string path)
            {
                return new MockPath(new [] {path});
            }

            protected override MockPath PurePathFactoryFromComponents(
                string drive, string root, string dirname, string basename, string extension)
            {
                return new MockPath(drive, root, dirname, basename, extension);
            }
        }

        [TestMethod]
        public void Constructor_WithEmptyString_ReturnsCurrentDir()
        {
            var path = new MockPath("");
            var expected = new MockPath(".");
            Assert.AreEqual(expected, path);
        }

        [TestMethod]
        public void Constructor_WithSameInput_CreatesEqualPaths()
        {
            var path1 = new MockPath(@"a\b");
            var path2 = new MockPath(@"a\b");

            Assert.AreEqual(path1, path2);
        }
        [TestMethod]
        public void Constructor_WithSameInputAndDifferentSeparator_CreatesEqualPaths()
        {
            var path1 = new MockPath(@"a\b");
            var path2 = new MockPath(@"a/b");

            Assert.AreEqual(path1, path2);
        }

        [TestMethod]
        public void Constructor_WithDriveLetterAndRoot_DoesNotDropTrailingSlash()
        {
            var path1 = new MockPath(@"C:\");
            const string expected = @"\";

            Assert.AreEqual(expected, path1.Root);
        }

        [TestMethod]
        public void Constructor_WithOnlyDriveLetter_HasNoRoot()
        {
            var path1 = new MockPath(@"C:");
            const string expected = "";

            Assert.AreEqual(expected, path1.Root);
        }

        [TestMethod]
        public void Constructor_WithJoiningPaths_CreatesEqualPaths()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath("a", "b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithOneContainingTrailingSlash_CreatesEqualPaths()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath(@"a", @"b\");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithBothContainingTrailingSlash_CreatesEqualPaths()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath(@"a\", @"b\");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithOneContainingEmptyComponent_CreatesEqualPaths()
        {
            var expected = new MockPath(@"a\\b\");
            var actual = new MockPath(@"a\b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithEmptyComponentAtBeginning_LeavesEmptyComponentOut()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath("", "a", "b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithEmptyComponentInMiddle_LeavesEmptyComponentOut()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath("a", "", "b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithAbsoluteComponentInMiddle_DropsComponentBeforeAbsoluteComponent()
        {
            var expected = new MockPath(@"\b\c");
            var actual = new MockPath(@"a", @"\b", @"c");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithAbsoluteAndEmptyComponent_DropsComponentBeforeAbsoluteComponent()
        {
            var expected = new MockPath(@"\b\c");
            var actual = new MockPath(@"a", @"\b\\", @"c");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithEmptyComponentAtEnd_LeavesEmptyComponentOut()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath("a", "b", "");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithOnePathRelative_PathsAreNotEqual()
        {
            var expected = new MockPath(@"a\b");
            var actual = new MockPath(@"\a\b");
            
            Assert.AreNotEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithDifferentPaths_PathsAreNotEqual()
        {
            var path1 = new MockPath(@"a");
            var path2 = new MockPath(@"a\b");

            Assert.AreNotEqual(path1, path2);
        }

        [TestMethod]
        public void Constructor_WithFileUri_StripsUriPrefix()
        {
            var path1 = new MockPath(@"c:\users");
            var path2 = new MockPath(@"file://c:/users");


            Assert.AreEqual(path1, path2);
        }

        [TestMethod]
        public void Constructor_WithParentDirectoryPart_DoesNotResolvePart()
        {
            const string expected = @"c:\users\userA\..\userB";
            var path = new MockPath(expected);


            Assert.AreEqual(expected, path.ToString());
        }

        [TestMethod]
        public void GetRoot_WithUncShare_ReturnsUncRoot()
        {
            var path = new MockPath(@"\\some\share");
            const string expected = @"\";

            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void GetRoot_WithLocalRoot_ReturnsRoot()
        {
            var path = new MockPath(@"c:\ProgramFiles\");
            const string expected = @"\";

            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void GetRoot_WithRelativePathOnDrive_ReturnsEmptyRoot()
        {
            var path = new MockPath(@"c:ProgramFiles\");
            const string expected = "";

            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void Join_WithAnotherPath_CreatesPathEqualToCombinedPath()
        {
            var path = new MockPath(@"a");
            var expected = new MockPath(@"a\b");

            var joined = path.Join("b");

            Assert.AreEqual(expected, joined);
        }

        [TestMethod]
        public void Join_WithEmptyPathAsInitial_CreatesPathEqualToSecondPath()
        {
            var path = new PurePosixPath();
            var expected = new PurePosixPath(@"\Users\nemecd\tmp\testfiles");

            var joined = path.Join(@"\Users\nemecd\tmp\testfiles");

            Assert.AreEqual(expected, joined);
        }

        [TestMethod]
        public void SafeJoin_WithRelative_CreatesPathEqualToCombinedPath()
        {
            var path = new MockPath(@"a");
            var expected = new MockPath(@"a\b");

            MockPath joined;
            Assert.IsTrue(path.TrySafeJoin("b", out joined));

            Assert.AreEqual(expected, joined);
        }

        [TestMethod]
        public void SafeJoin_WithRelativeParentTraversal_FailsJoin()
        {
            var path = new MockPath(@"a");

            MockPath joined;
            Assert.IsFalse(path.TrySafeJoin("..", out joined));
        }

        [TestMethod]
        public void SafeJoin_WithComplexRelativeParentTraversal_FailsJoin()
        {
            var path = new MockPath(@"a");

            MockPath joined;
            Assert.IsFalse(path.TrySafeJoin(@"b\c\d\..\f\..\..\..\g\..\..", out joined));
        }

        [TestMethod]
        public void SafeJoin_WithSiblingRelativeParentTraversal_FailsJoin()
        {
            var path = new MockPath(@"a");

            MockPath joined;
            Assert.IsFalse(path.TrySafeJoin(@"..\c\d", out joined));
        }

        [TestMethod]
        public void SafeJoin_WithSiblinStartsWithTraversal_FailsJoin()
        {
            var path = new MockPath(@"a");

            MockPath joined;
            Assert.IsFalse(path.TrySafeJoin(@"..\ab\d", out joined));
        }

        [TestMethod]
        public void GetParent_WithAParent_ReturnsTheParent()
        {
            var path = new MockPath(@"C:\Users\nemec");

            var expected = new MockPath(@"C:\Users");

            var actual = path.Parent();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParents_WithAParent_ReturnsTheParent()
        {
            var path = new MockPath(@"C:\nemec");

            var expected = new MockPath(@"C:\");

            var parents = path.Parents().GetEnumerator();
            parents.MoveNext();
            var actual = parents.Current;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParents_WithMultipleParents_ReturnsTheParentsFromMostToLeastSpecific()
        {
            var path = new MockPath(@"C:\users\nemec");

            var expected = new[]
                {
                    new MockPath(@"C:\users"),
                    new MockPath(@"C:\")
                };

            Assert.IsTrue(expected.SequenceEqual(path.Parents()));
        }

        [TestMethod]
        public void GetParents_WithAParent_DoesNotReturnSelf()
        {
            var path = new MockPath(@"C:\nemec");

            var parents = path.Parents();

            Assert.AreEqual(1, parents.Count());
        }

        [TestMethod]
        public void GetExtension_WithSingleExtension_ReturnsThatExtension()
        {
            const string expected = ".txt";
            var path = new MockPath(@"c:\users\nemec\file.txt");

            var actual = path.Extension;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetExtension_WithMultipleExtensions_ReturnsLastExtension()
        {
            const string expected = ".gz";
            var path = new MockPath(@"c:\users\nemec\file.txt.tar.gz");

            var actual = path.Extension;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetExtension_WithDotfileNoExtension_ReturnsEmptyString()
        {
            const string expected = "";
            var path = new MockPath(@"c:\users\nemec\.bashrc");

            var actual = path.Extension;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetExtensions_WithMultipleExtensions_ReturnsExtensionsInOrder()
        {
            var expected = new [] {".txt", ".tar", ".gz"};
            var path = new MockPath(@"c:\users\nemec\file.txt.tar.gz");

            var actual = path.Extensions;

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetExtensions_WithDotfileAndNoExtension_ReturnsEmptyEnumerable()
        {
            const int expected = 0;
            var path = new MockPath(@"c:\users\nemec\.bashrc");

            var actual = path.Extensions.Count();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetBasenameWithoutExtension_WithExtension_ReturnsBasename()
        {
            
            var path = new MockPath(@"c:\users\nemec\file.txt");

            var expected = path.Basename;
            var actual = path.BasenameWithoutExtensions;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetBasenameWithoutExtension_WithMultipleExtensions_ReturnsBasenameMinusExtensions()
        {
            const string expected = "file";
            var path = new MockPath(@"c:\users\nemec\file.txt.gz");

            var actual = path.BasenameWithoutExtensions;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_WithOneExtensionPrependedWithDot_ReturnsPathWithNewExtension()
        {
            var expected = new MockPath(@"c:\users\nemec\file.xml");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            var actual = path.WithExtension(".xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_AddingTwoExtensionsPrependedWithDot_ReturnsPathWithNewExtensions()
        {
            var expected = new MockPath(@"c:\users\nemec\file.tar.gz");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            var actual = path.WithExtension(".tar.gz");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_AddingTwoExtensions_ReturnsPathWithNewExtensions()
        {
            var expected = new MockPath(@"c:\users\nemec\file.tar.gz");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            var actual = path.WithExtension("tar.gz");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_WithMultipleExtensionPrependedWithDot_ReturnsPathWithNewLastExtension()
        {
            var expected = new MockPath(@"c:\users\nemec\file.tar.xml");
            var path = new MockPath(@"c:\users\nemec\file.tar.txt");

            var actual = path.WithExtension(".xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_WithNoExtension_ReturnsPathWithNewExtension()
        {
            var expected = new MockPath(@"c:\users\nemec\file.xml");
            var path = new MockPath(@"c:\users\nemec\file");

            var actual = path.WithExtension("xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsAbsolute_WithAbsolutePath_ReturnsFalse()
        {
            var path = new MockPath("c:/hello/world");
            Assert.IsTrue(path.IsAbsolute());
        }

        [TestMethod]
        public void IsAbsolute_WithRelativePath_ReturnsFalse()
        {
            var path = new MockPath("hello/world");
            Assert.IsFalse(path.IsAbsolute());
        }

        [TestMethod]
        public void IsAbsolute_WithCurrentDirectory_ReturnsFalse()
        {
            var path = new MockPath("");
            Assert.IsFalse(path.IsAbsolute());
        }

        [TestMethod]
        public void IsAbsolute_WithParentDirectory_ReturnsFalse()
        {
            var path = new MockPath("../hello");
            Assert.IsFalse(path.IsAbsolute());
        }

        [TestMethod]
        public void RelativeTo_WithParent_ReturnsRelativePath()
        {
            var expected = new MockPath(@"nemec\file.txt");

            var parent = new MockPath(@"c:\users\");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            var actual = path.RelativeTo(parent);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithLongPath_ReturnsRelativePath()
        {
            var expected = new MockPath(@"nemec\tmp\filestorage\file.txt");

            var parent = new MockPath(@"c:\users\");
            var path = new MockPath(@"c:\users\nemec\tmp\filestorage\file.txt");

            var actual = path.RelativeTo(parent);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithLongUncPath_ReturnsRelativePath()
        {
            var expected = new MockPath(@"subdir\file.exe");

            var parent = new MockPath(@"\\some\share\");
            var path = new MockPath(@"\\some\share\subdir\file.exe");

            var actual = path.RelativeTo(parent);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithParent_IsAbsoluteIsFalse()
        {
            var parent = new MockPath(@"c:\users\");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);

            Assert.IsTrue(path.IsAbsolute());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithParentInDifferentDir_ThrowsException()
        {
            var parent = new MockPath(@"\Program Files\");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithParentContainingPartialFilename_ThrowsException()
        {
            var parent = new MockPath(@"c:\users\nemec\file");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithParentLackingDrive_ThrowsException()
        {
            var parent = new MockPath(@"\users\");
            var path = new MockPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);
        }

        [TestMethod]
        public void WithDirname_WithRegularDirname_ReplacesDirname()
        {
            const string dirname = "new/dirname";
            var path = new MockPath("C:/some/directory/file.txt");
            var expected = new MockPath("C:/new/dirname/file.txt");

            var actual = path.WithDirname(dirname);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithDirname_WithDirnameAndRoot_ReplacesDirname()
        {
            const string dirname = "/new/dirname";
            var path = new MockPath("C:/some/directory/file.txt");
            var expected = new MockPath("C:/new/dirname/file.txt");

            var actual = path.WithDirname(dirname);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithDirname_WithDirnameAndDrive_ReplacesDirname()
        {
            const string dirname = "F:/new/dirname";
            var path = new MockPath("C:/some/directory/file.txt");
            var expected = new MockPath("C:/new/dirname/file.txt");

            var actual = path.WithDirname(dirname);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithDirname_WithDirnameAndRelativeOrigin_ReplacesDirnameAndAddsDriveAndRoot()
        {
            const string dirname = "C:/new/dirname";
            var path = new MockPath("file.txt");
            var expected = new MockPath("C:/new/dirname/file.txt");

            var actual = path.WithDirname(dirname);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithDirname_WithDirnameAndBothRelative_ReplacesDirname()
        {
            const string dirname = "new/dirname";
            var path = new MockPath("file.txt");
            var expected = new MockPath("new/dirname/file.txt");

            var actual = path.WithDirname(dirname);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AsUri_WithPath_ReturnsAUri()
        {
            var path = new MockPath(@"C:\nemec");
            var expected = new Uri("file://C:/nemec");

            var actual = path.ToUri();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToString_WithTrailingDot_DoesNotStripTrailingDot()
        {
            const string expected = @"c:\nemec\file...\other.txt";
            var path = new MockPath(expected);

            var actual = path.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_WithTypeConverter_CreatesPathForPlatform()
        {
            var isWindows = true;
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                isWindows = false;
            }

            const string path = @"C:\users\tmp";
            var converter = TypeDescriptor.GetConverter(typeof (IPurePath));
            var expected = isWindows
                ? typeof (PureWindowsPath)
                : typeof (PurePosixPath);

            var actual = converter.ConvertFromInvariantString(path);

            Assert.IsInstanceOfType(actual, expected);
        }
    }
}
