using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Inglish.Test
{
    [TestClass]
    public class CommandTests
    {
        private Thesaurus _thesaurus;

        [TestInitialize]
        public void Setup()
        {
            _thesaurus = Thesaurus.Deserialize(File.ReadAllText(@"..\..\Dictionary.json"));
        }

        [TestMethod]
        public void Command_OneVerbOneSubject_DeserializedAndEqual()
        {
            
            var parser = new Parser(_thesaurus);
            var cmdSource = parser.DoCommand("take map");

            var cmdtext = JsonConvert.SerializeObject(cmdSource);
            var cmdTarget = JsonConvert.DeserializeObject<TokenCommand>(cmdtext);

            cmdTarget.ShouldBeEquivalentTo(cmdSource);
        }
    }
}