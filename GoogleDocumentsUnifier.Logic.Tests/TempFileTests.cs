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
                Assert.IsNotNull(temp.File);
                Assert.IsTrue(temp.File.Exists);

                path = temp.File.FullName;
            }
            Assert.IsFalse(File.Exists(path));
        }
    }
}