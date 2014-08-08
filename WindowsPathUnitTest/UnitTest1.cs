using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathLib;

namespace WindowsPathUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var path = new NtPath(@"C:\Program Files (x86)\..\..\..\..\Users\nemecd\tmp\sym\metadata.json");
            path.Resolve();
        }
    }
}
