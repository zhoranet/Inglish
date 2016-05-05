using System;

namespace Inglish
{
    public class Token
    {
        public Token()
        {
        }

        public Token(TokenType tokenType, long tokenId)
        {
            TokenType = tokenType;
            Id = tokenId;
        }

        public TokenType TokenType { get; set; }

        public T GetKey<T>() where T : struct, IConvertible
        {
            return (T) EnumConverter<T>.Convert(Id);
        }

        public long Id { get; private set; }

        public void SetKey<T>(T tokenKey) where T : struct, IConvertible
        {
            Id = Convert.ToInt64(tokenKey);
        }

        public void SetKey(object tokenKey)
        {
            Id = Convert.ToInt64(tokenKey);
        }

        public static Token Create<T>(TokenType tokenType, T tokenKey) where T : struct, IConvertible
        {
            var token = new Token {TokenType = tokenType};
            token.SetKey(tokenKey);
            return token;
        }

        public override string ToString()
        {
            return $"Type:{TokenType},Id:{Id}";
        }
    }
}