using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inglish.Test
{
    [TestClass]
    public class TokenSetTests
    {
        [TestMethod]
        public void Current_MoveNext_CurrentHasValue()
        {
            var tokenSet = new TokenSet(new[] {new Token()});
            tokenSet.MoveNext();
            tokenSet.Current.Should().NotBeNull();
        }

        [TestMethod]
        public void Current_InitTwoTokes_CurrentHasValue()
        {
            var tokenSet =
                new TokenSet(new[] {new Token {MorphType = MorphType.Noun}, new Token {MorphType = MorphType.Verb}});
            tokenSet.MoveNext();
            tokenSet.MoveNext();
            tokenSet.Current.MorphType.Should().Be(MorphType.Verb);
        }
    }
}