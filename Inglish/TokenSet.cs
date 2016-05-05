using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inglish
{
    public class TokenSet : IEnumerator<Token>
    {
        private readonly List<Token> _tokens;
        private int _position = -1;

        public TokenSet(IEnumerable<Token> tokens = null)
        {
            _tokens = tokens?.ToList() ?? new List<Token>();
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _position++;
            return _position < _tokens.Count;
        }

        public void Reset()
        {
            _position = -1;
        }

        public Token Current => _position >= 0 && _position < _tokens.Count ? _tokens[_position] : null;

        public bool IsFinished => Current == null;

        object IEnumerator.Current => Current;
        
        public Token[] GetTokens()
        {
            return _tokens.ToArray();
        }
    }
}