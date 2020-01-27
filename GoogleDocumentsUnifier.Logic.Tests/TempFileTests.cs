using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class TempFileTests
    {
        [TestMethod]
        public void TempFileTest()
        {
            string path;
            using (var temp = new TempFile())
            {
                Assert.IsNotNull(temp);
                path = temp.Path;

                Assert.IsNotNull(path);
                Assert.IsTrue(File.Exists(path));
            }
            Assert.IsFalse(File.Exists(path));
        }
    }
}