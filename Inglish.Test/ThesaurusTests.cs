using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
