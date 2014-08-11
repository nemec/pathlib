using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathLib.Utils;

namespace PathLib.UnitTest
{
    [TestClass]
    public class PurePathUnitTest
    {
        // TODO A relative path has neither a drive nor a root

        // TODO NT Paths can have a drive or a root

        // TODO POSIX path drive always empty

        // TODO empty path changes to '.'

        private class MockParser : IPathParser
        {
            private readonly char[] _reservedCharacters =
            {
                '<', '>', '|'
            };

            private string PathSeparator { get; set; }

            public MockParser(string pathSeparator)
            {
                PathSeparator = pathSeparator;
            }

            public string ParseDrive(string path)
            {
                return !String.IsNullOrEmpty(path)
                    ? PathUtils.GetPathRoot(path, PathSeparator).TrimEnd(PathSeparator[0])
                    : null;
            }

            public string ParseRoot(string path)
            {
                if (String.IsNullOrEmpty(path))
                {
                    return null;
                }

                var root = PathUtils.GetPathRoot(path, PathSeparator);
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
                return PathUtils.GetDirectoryName(remainingPath, PathSeparator) ?? "";
            }

            public string ParseBasename(string remainingPath)
            {
                return !String.IsNullOrEmpty(remainingPath)
                    ? remainingPath != PathUtils.CurrentDirectoryIdentifier
                        ? PathUtils.GetFileNameWithoutExtension(remainingPath, PathSeparator)
                            : PathUtils.CurrentDirectoryIdentifier
                    : null;
            }

            public string ParseExtension(string remainingPath)
            {
                return !String.IsNullOrEmpty(remainingPath)
                    ? PathUtils.GetExtension(remainingPath, PathSeparator)
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
            public MockPath()
            { }

            public MockPath(params IPurePath[] paths)
                : base(paths)
            { }

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
        public void GetExtensions_WithMultipleExtensions_ReturnsExtensionsInOrder()
        {
            var expected = new [] {".txt", ".tar", ".gz"};
            var path = new MockPath(@"c:\users\nemec\file.txt.tar.gz");

            var actual = path.Extensions;

            CollectionAssert.AreEqual(expected, actual);
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
        public void RelativeTo_WithParent_ReturnsRelativePath()
        {
            var expected = new MockPath(@"nemec\file.txt");

            var parent = new MockPath(@"c:\users\");
            var path = new MockPath(@"c:\users\nemec\file.txt");

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
        public void AsUri_WithPath_ReturnsAUri()
        {
            var path = new MockPath(@"C:\nemec");
            var expected = new Uri("file://C:/nemec");

            var actual = path.ToUri();

            Assert.AreEqual(expected, actual);
        }
    }
}
