using System;
using System.Collections.Generic;
using System.Linq;

namespace Inglish
{
    public class Parser
    {
        private const string TokenSeparatorString = " ,.!_>+'\"";

        private static readonly char[] TokenSeparators = TokenSeparatorString.ToCharArray();

        public static readonly Dictionary<TokenType, Type> TokenKeyTypes = new Dictionary<TokenType, Type>
        {
            {TokenType.Verb, typeof (VerbKey)},
            {TokenType.Noun, typeof (NounKey)},
            {TokenType.Adjective, typeof (AdjectiveKey)},
            {TokenType.Adverb, typeof (AdverbKey)},
            {TokenType.Article, typeof (ArticleKey)},
            {TokenType.Conjunction, typeof (ConjunctionKey)},
            {TokenType.Preposition, typeof (PrepositionKey)},
            {TokenType.Pronoun, typeof (PronounKey)}
        };

        public Command DoCommand(string text)
        {
            var words = Tokenise(text);
            var tokenSet = GetTokenSet(words);
            return ParseSentence(tokenSet);
        }

        //==============================================
        // Language definition
        // SENTENCE = [SAY [TO] [OBJECT] "] COMMAND <CONJUNCTION> COMMAND ["]
        //
        // COMMAND     = [ADVERB LIST] <VERB> [PREPOSITION] [<OBJECT LIST> [ADVERB LIST] [<PREPOSITION> <OBJECT LIST>] [ADVERB LIST]]
        //
        // ADVERB LIST = ADVERB [[<CONJUNCTION>] ADVERB LIST]
        //
        // OBJECT LIST = OBJECT [[<CONJUNCTION>] OBJECT LIST]
        //
        // OBJECT      = [ADJECTIVE LIST] <NOUN>
        //
        // OBJECT      = <PRONOUN>
        //
        // ADJECTIVE LIST = ADJECTIVE [[<CONJUNCTION>] ADJECTIVE LIST]
        //
        //-----------------------------------------------
        internal Command ParseSentence(TokenSet tokens)
        {
            var cmd = new Command();

            tokens.Reset();

            if (!tokens.MoveNext()) return null;

            var token = tokens.Current;

            if (token.TokenType == TokenType.Verb)
            {
                var tokenVerb = token.GetKey<VerbKey>();

                if (tokenVerb == VerbKey.Say || tokenVerb == VerbKey.Talk)
                {
                    cmd.Verb = tokenVerb;
                    if (!tokens.MoveNext()) return null;

                    if (tokens.Current.TokenType == TokenType.Preposition)
                    {
                        cmd.Preposition = tokens.Current.GetKey<PrepositionKey>();
                        if (!tokens.MoveNext()) return null;
                    }

                    ParseSubject(tokens, cmd);

                    do
                    {
                        cmd.NpcTokens.Add(tokens.Current);
                    } while (tokens.MoveNext());

                    return cmd;
                }
            }

            ParseCommand(tokens, cmd);

            while (!tokens.IsFinished && tokens.Current.TokenType == TokenType.Conjunction)
            {
                ParseCommand(tokens, cmd);
            }

            return cmd;
        }

        internal void ParseCommand(TokenSet tokens, Command cmd)
        {
            if (tokens.Current.TokenType == TokenType.Adverb)
            {
                ParseAdverbList(tokens, cmd);
            }

            if (tokens.Current.TokenType == TokenType.Verb)
            {
                cmd.Verb = tokens.Current.GetKey<VerbKey>();
                tokens.MoveNext();
            }

            if (tokens.IsFinished) return;

            if (tokens.Current.TokenType == TokenType.Preposition)
            {
                cmd.Preposition = tokens.Current.GetKey<PrepositionKey>();
            }

            while (tokens.Current.TokenType == TokenType.Preposition && tokens.MoveNext())
            {
            }

            if (tokens.IsFinished) return;

            ParseSubjectList(tokens, cmd);

            if (tokens.IsFinished) return;

            ParseAdverbList(tokens, cmd);

            if (tokens.IsFinished) return;

            while (tokens.Current.TokenType == TokenType.Preposition && tokens.MoveNext())
            {
            }

            if (tokens.IsFinished) return;

            ParseObjectList(tokens, cmd);

            if (tokens.IsFinished) return;

            ParseAdverbList(tokens, cmd);
        }

        internal TokenSet GetTokenSet(string[] words)
        {
            var tokens = new List<Token>();

            foreach (var word in words)
            {
                Token token;
                if (TryConvertToToken(word, out token))
                {
                    tokens.Add(token);
                }
            }
            return new TokenSet(tokens);
        }

        internal bool TryConvertToToken(string word, out Token token)
        {
            token = null;
            var foundToken = false;

            foreach (var tokenKeyType in TokenKeyTypes)
            {
                if (TryCreateToken(tokenKeyType.Key, tokenKeyType.Value, word, out token))
                {
                    foundToken = true;
                    break;
                }
            }

            return foundToken;
        }

        internal bool TryCreateToken(TokenType tokenKeyType, Type tokenType, string word, out Token token)
        {
            token = new Token();
            var foundToken = false;
            foreach (var tokenKey in Enum.GetValues(tokenType))
            {
                if (CompareTokens(word, tokenKey))
                {
                    token.TokenType = tokenKeyType;
                    token.SetKey(tokenKey);
                    foundToken = true;
                    break;
                }
            }

            return foundToken;
        }

        internal bool CompareTokens(string word, object tokenValue)
        {
            return word.Equals(tokenValue.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        internal string[] Tokenise(string text)
        {
            return text.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLowerInvariant()).ToArray();
        }

        internal void ParseAdverbList(TokenSet tokens, Command cmd)
        {
            var finished = false;

            if (tokens.Current.TokenType != TokenType.Adverb) return;

            while (!finished)
            {
                switch (tokens.Current.TokenType)
                {
                    case TokenType.Adverb:
                        cmd.Adverbs.Add(tokens.Current.GetKey<AdverbKey>());
                        finished = !tokens.MoveNext();
                        break;
                    case TokenType.Conjunction:
                        finished = !tokens.MoveNext();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
        }

        internal void ParseAdjectiveList(TokenSet tokens, TokenObject tokenObject)
        {
            if (tokens.Current.TokenType != TokenType.Adjective) return;

            var finished = false;

            while (!finished)
            {
                switch (tokens.Current.TokenType)
                {
                    case TokenType.Adjective:
                        tokenObject.Adjectives.Add(tokens.Current.GetKey<AdjectiveKey>());
                        finished = !tokens.MoveNext();
                        break;
                    case TokenType.Conjunction:
                        finished = !tokens.MoveNext();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
        }

        internal void ParseObjectList(TokenSet tokens, Command cmd)
        {
            var finished = false;

            while (!finished)
            {
                switch (tokens.Current.TokenType)
                {
                    case TokenType.Adjective:
                    case TokenType.Noun:
                    case TokenType.Pronoun:
                        ParseObject(tokens, cmd);
                        finished = tokens.IsFinished;
                        break;

                    case TokenType.Conjunction:
                    case TokenType.Article:
                        finished = !tokens.MoveNext();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
        }

        //--------------------------------------
        // OBJECT      = [ADJECTIVE LIST] <NOUN>
        // OBJECT      = <PRONOUN>
        internal bool ParseObject(TokenSet tokens, Command cmd)
        {
            var finished = false;
            var tokenObject = new TokenObject();

            if (tokens.Current.TokenType == TokenType.Adjective)
            {
                ParseAdjectiveList(tokens, tokenObject);
                finished = tokens.IsFinished;
            }

            if (!finished)
            {
                switch (tokens.Current.TokenType)
                {
                    case TokenType.Noun:
                        tokenObject.Noun = tokens.Current.GetKey<NounKey>();
                        tokens.MoveNext();
                        cmd.Objects.Add(tokenObject);
                        finished = true;
                        break;
                    case TokenType.Pronoun:
                        finished = ParseObjectPronoun(tokens.Current.GetKey<PronounKey>(), tokenObject, cmd);
                        tokens.MoveNext();
                        break;
                }
            }

            return finished;
        }

        internal bool ParseObjectPronoun(PronounKey pronounKey, TokenObject tokenObject, Command cmd)
        {
            var result = false;

            switch (pronounKey)
            {
                case PronounKey.All:
                case PronounKey.Everything:
                    tokenObject.Noun = NounKey._ALL_;
                    cmd.Objects.Add(tokenObject);
                    result = true;
                    break;
                case PronounKey.Me:
                    tokenObject.Noun = NounKey.Hobbit;
                    cmd.Objects.Add(tokenObject);
                    result = true;
                    break;
            }

            return result;
        }

        internal void ParseSubjectList(TokenSet tokens, Command cmd)
        {
            var finished = false;

            while (!finished)
            {
                switch (tokens.Current.TokenType)
                {
                    case TokenType.Adjective:
                    case TokenType.Noun:
                    case TokenType.Pronoun:
                        ParseSubject(tokens, cmd);
                        finished = tokens.IsFinished;
                        break;
                    case TokenType.Conjunction:
                    case TokenType.Article:
                        finished = !tokens.MoveNext();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
        }

        internal bool ParseSubject(TokenSet tokens, Command cmd)
        {
            var tokenObject = new TokenObject();

            var result = false;
            if (tokens.Current.TokenType == TokenType.Adjective)
            {
                ParseAdjectiveList(tokens, tokenObject);
                if (tokens.Current == null) return result;
            }

            switch (tokens.Current.TokenType)
            {
                case TokenType.Noun:
                    tokenObject.Noun = tokens.Current.GetKey<NounKey>();
                    tokens.MoveNext();
                    cmd.Subjects.Add(tokenObject);
                    result = true;
                    break;
                case TokenType.Pronoun:
                    result = ParseSubjectPronoun(tokens.Current.GetKey<PronounKey>(), tokenObject, cmd);
                    tokens.MoveNext();
                    break;
            }

            return result;
        }

        internal bool ParseSubjectPronoun(PronounKey pronoun, TokenObject tokenObject, Command cmd)
        {
            var result = false;

            switch (pronoun)
            {
                case PronounKey.All:
                case PronounKey.Everything:
                    tokenObject.Noun = NounKey._ALL_;
                    cmd.Subjects.Add(tokenObject);
                    result = true;
                    break;
                case PronounKey.Me:
                    tokenObject.Noun = NounKey.Hobbit;
                    cmd.Subjects.Add(tokenObject);
                    result = true;
                    break;
            }

            return result;
        }
    }
}