using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Inglish.Test
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void Command_OneVerbOneSubject_DeserializedAndEqual()
        {
            var parser = new Parser();
            var cmdSource = parser.DoCommand("take map");

            var cmdtext = JsonConvert.SerializeObject(cmdSource);
            var cmdTarget = JsonConvert.DeserializeObject<Command>(cmdtext);

            cmdTarget.ShouldBeEquivalentTo(cmdSource);
        }
    }
}