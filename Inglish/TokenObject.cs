using System.Collections.Generic;

namespace Inglish
{
    public class TokenObject
    {
        public string Noun { get; set; }
        public IList<Token> Adjectives { get; set; }
        public IList<TokenObject> Objects { get; set; }

        public TokenObject()
        {
            Objects = new List<TokenObject>();
            Adjectives = new List<Token>();
        }
    }
}