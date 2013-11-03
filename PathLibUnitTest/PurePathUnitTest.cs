using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PathLib.UnitTest
{
    [TestClass]
    public class PurePathUnitTest
    {
        // TODO A relative path has neither a drive nor a root

        // TODO NT Paths can have a drive or a root

        // TODO POSIX path drive always empty

        // TODO empty path changes to '.'

        [TestMethod]
        public void Constructor_WithSameInput_CreatesEqualPaths()
        {
            var path1 = new PureNtPath(@"a\b");
            var path2 = new PureNtPath(@"a\b");

            Assert.AreEqual(path1, path2);
        }
        [TestMethod]
        public void Constructor_WithSameInputAndDifferentSeparator_CreatesEqualPaths()
        {
            var path1 = new PureNtPath(@"a\b");
            var path2 = new PureNtPath(@"a/b");

            Assert.AreEqual(path1, path2);
        }

        [TestMethod]
        public void Constructor_WithDriveLetterAndRoot_DoesNotDropTrailingSlash()
        {
            var path1 = new PureNtPath(@"C:\");
            const string expected = @"\";

            Assert.AreEqual(expected, path1.Root);
        }

        [TestMethod]
        public void Constructor_WithOnlyDriveLetter_HasNoRoot()
        {
            var path1 = new PureNtPath(@"C:");
            const string expected = "";

            Assert.AreEqual(expected, path1.Root);
        }

        [TestMethod]
        public void Constructor_WithJoiningPaths_CreatesEqualPaths()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath("a", "b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithOneContainingTrailingSlash_CreatesEqualPaths()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath(@"a", @"b\");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithBothContainingTrailingSlash_CreatesEqualPaths()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath(@"a\", @"b\");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithOneContainingEmptyComponent_CreatesEqualPaths()
        {
            var expected = new PureNtPath(@"a\\b\");
            var actual = new PureNtPath(@"a\b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithEmptyComponentAtBeginning_LeavesEmptyComponentOut()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath("", "a", "b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithEmptyComponentInMiddle_LeavesEmptyComponentOut()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath("a", "", "b");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithAbsoluteComponentInMiddle_DropsComponentBeforeAbsoluteComponent()
        {
            var expected = new PureNtPath(@"\b\c");
            var actual = new PureNtPath(@"a", @"\b", @"c");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithAbsoluteAndEmptyComponent_DropsComponentBeforeAbsoluteComponent()
        {
            var expected = new PureNtPath(@"\b\c");
            var actual = new PureNtPath(@"a", @"\b\\", @"c");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithEmptyComponentAtEnd_LeavesEmptyComponentOut()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath("a", "b", "");
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithOnePathRelative_PathsAreNotEqual()
        {
            var expected = new PureNtPath(@"a\b");
            var actual = new PureNtPath(@"\a\b");
            
            Assert.AreNotEqual(expected, actual);
        }

        [TestMethod]
        public void Constructor_WithDifferentPaths_PathsAreNotEqual()
        {
            var path1 = new PureNtPath(@"a");
            var path2 = new PureNtPath(@"a\b");

            Assert.AreNotEqual(path1, path2);
        }

        [TestMethod]
        public void GetRoot_WithUncShare_ReturnsUncRoot()
        {
            var path = new PureNtPath(@"\\some\share");
            const string expected = @"\";

            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void GetRoot_WithLocalRoot_ReturnsRoot()
        {
            var path = new PureNtPath(@"c:\ProgramFiles\");
            const string expected = @"\";

            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void GetRoot_WithRelativePathOnDrive_ReturnsEmptyRoot()
        {
            var path = new PureNtPath(@"c:ProgramFiles\");
            const string expected = "";

            Assert.AreEqual(expected, path.Root);
        }

        [TestMethod]
        public void Join_WithAnotherPath_CreatesPathEqualToCombinedPath()
        {
            var path = new PureNtPath(@"a");
            var expected = new PureNtPath(@"a\b");

            var joined = path.Join("b");

            Assert.AreEqual(expected, joined);
        }

        [TestMethod]
        public void GetParent_WithAParent_ReturnsTheParent()
        {
            var path = new PureNtPath(@"C:\Users\nemec");

            var expected = new PureNtPath(@"C:\Users");

            var actual = path.Parent();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParents_WithAParent_ReturnsTheParent()
        {
            var path = new PureNtPath(@"C:\nemec");

            var expected = new PureNtPath(@"C:\");

            var parents = path.Parents().GetEnumerator();
            parents.MoveNext();
            var actual = parents.Current;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParents_WithMultipleParents_ReturnsTheParentsFromMostToLeastSpecific()
        {
            var path = new PureNtPath(@"C:\users\nemec");

            var expected = new[]
                {
                    new PureNtPath(@"C:\users"),
                    new PureNtPath(@"C:\")
                };

            Assert.IsTrue(expected.SequenceEqual(path.Parents()));
        }

        [TestMethod]
        public void GetParents_WithAParent_DoesNotReturnSelf()
        {
            var path = new PureNtPath(@"C:\nemec");

            var parents = path.Parents();

            Assert.AreEqual(1, parents.Count());
        }

        [TestMethod]
        public void GetExtension_WithSingleExtension_ReturnsThatExtension()
        {
            const string expected = ".txt";
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            var actual = path.Extension;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetExtension_WithMultipleExtensions_ReturnsLastExtension()
        {
            const string expected = ".gz";
            var path = new PureNtPath(@"c:\users\nemec\file.txt.tar.gz");

            var actual = path.Extension;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetExtensions_WithMultipleExtensions_ReturnsExtensionsInOrder()
        {
            var expected = new [] {".txt", ".tar", ".gz"};
            var path = new PureNtPath(@"c:\users\nemec\file.txt.tar.gz");

            var actual = path.Extensions;

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_WithOneExtensionPrependedWithDot_ReturnsPathWithNewExtension()
        {
            var expected = new PureNtPath(@"c:\users\nemec\file.xml");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            var actual = path.WithExtension(".xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_WithMultipleExtensionPrependedWithDot_ReturnsPathWithNewLastExtension()
        {
            var expected = new PureNtPath(@"c:\users\nemec\file.tar.xml");
            var path = new PureNtPath(@"c:\users\nemec\file.tar.txt");

            var actual = path.WithExtension(".xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WithExtension_WithMultipleExtensionAndNoDot_ReturnsPathWithNewLastExtension()
        {
            var expected = new PureNtPath(@"c:\users\nemec\file.xml");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            var actual = path.WithExtension("xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithParent_ReturnsRelativePath()
        {
            var expected = new PureNtPath(@"nemec\file.txt");

            var parent = new PureNtPath(@"c:\users\");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            var actual = path.RelativeTo(parent);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeTo_WithParent_IsAbsoluteIsFalse()
        {
            var parent = new PureNtPath(@"c:\users\");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);

            Assert.IsTrue(path.IsAbsolute());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithParentInDifferentDir_ThrowsException()
        {
            var parent = new PureNtPath(@"\Program Files\");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithParentContainingPartialFilename_ThrowsException()
        {
            var parent = new PureNtPath(@"c:\users\nemec\file");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RelativeTo_WithParentLackingDrive_ThrowsException()
        {
            var parent = new PureNtPath(@"\users\");
            var path = new PureNtPath(@"c:\users\nemec\file.txt");

            path.RelativeTo(parent);
        }

        [TestMethod]
        public void AsUri_WithPath_ReturnsAUri()
        {
            var path = new PureNtPath(@"C:\nemec");
            var expected = new Uri("file://C:/nemec");

            var actual = path.AsUri();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsFile_WithDirectoryAndNoFile_ReturnsFalse()
        {
            var path = new PureNtPath(@"C:\nemec\");
            const bool expected = false;

            var actual = path.IsFile();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsFile_NoDirectoryAndWithFile_ReturnsTrue()
        {
            var path = new PureNtPath(@"foo.txt");
            const bool expected = true;

            var actual = path.IsFile();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsFile_WithDirectoryAndWithFile_ReturnsFalse()
        {
            var path = new PureNtPath(@"C:\nemec\foo.txt");
            const bool expected = false;

            var actual = path.IsFile();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsDir_WithDirectoryAndNoFile_ReturnsTrue()
        {
            var path = new PureNtPath(@"C:\nemec\");
            const bool expected = true;

            var actual = path.IsDir();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsFile_NoDirectoryAndWithFile_ReturnsFalse()
        {
            var path = new PureNtPath(@"foo.txt");
            const bool expected = false;

            var actual = path.IsDir();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsDir_WithDirectoryAndWithFile_ReturnsFalse()
        {
            var path = new PureNtPath(@"C:\nemec\foo.txt");
            const bool expected = false;

            var actual = path.IsFile();

            Assert.AreEqual(expected, actual);
        }
    }
}
