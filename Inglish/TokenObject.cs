using System.Collections;
using System.Collections.Generic;

namespace Inglish
{
    public class TokenObject
    {
        public NounKey Noun { get; set; }
        public IList<AdjectiveKey> Adjectives { get; set; }
        public IList<TokenObject> Objects { get; set; }

        public TokenObject()
        {
            Objects = new List<TokenObject>();
            Adjectives = new List<AdjectiveKey>();
        }
    }
}