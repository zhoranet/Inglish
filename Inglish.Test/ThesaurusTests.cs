using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inglish.Test
{
    [TestClass]
    public class ThesaurusTests
    {
        

        [TestMethod]
        public void Load_SmokeTest()
        {
            var thesaurus = Thesaurus.Deserialize(File.ReadAllText(@"..\..\Dictionary.json"));
            thesaurus.MorphTypes.Should().HaveCount(8);
        }
    }
}
