using System;
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
                new TokenSet(new[] {new Token {TokenType = TokenType.Noun}, new Token {TokenType = TokenType.Verb}});
            tokenSet.MoveNext();
            tokenSet.MoveNext();
            tokenSet.Current.TokenType.Should().Be(TokenType.Verb);
        }
    }
}