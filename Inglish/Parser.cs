using System;
using System.Collections.Generic;
using System.Linq;

namespace Inglish
{
    public class Parser : IParser
    {
        private readonly IThesaurus _thesaurus;
        private const string TokenSeparatorString = " ,.!_>+'\"";

        private static readonly char[] TokenSeparators = TokenSeparatorString.ToCharArray();

        
        public Parser(IThesaurus thesaurus)
        {
            _thesaurus = thesaurus;
        }

        public TokenCommand DoCommand(string text)
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
        internal TokenCommand ParseSentence(TokenSet tokens)
        {
            var cmd = new TokenCommand();

            tokens.Reset();

            if (!tokens.MoveNext()) return null;

            var token = tokens.Current;

            if (token.MorphType == MorphType.Verb)
            {
                var tokenVerb = token;

                if (tokenVerb.Value == _thesaurus.Keywords.VerbSay || tokenVerb.Value == _thesaurus.Keywords.VerbTalk)
                {
                    cmd.Verb = tokenVerb;
                    if (!tokens.MoveNext()) return null;

                    if (tokens.Current.MorphType == MorphType.Preposition)
                    {
                        cmd.Preposition = tokens.Current.Value;
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

            while (!tokens.IsFinished && tokens.Current.MorphType == MorphType.Conjunction)
            {
                ParseCommand(tokens, cmd);
            }

            return cmd;
        }

        internal void ParseCommand(TokenSet tokens, TokenCommand cmd)
        {
            if (tokens.Current.MorphType == MorphType.Adverb)
            {
                ParseAdverbList(tokens, cmd);
            }

            if (tokens.Current.MorphType == MorphType.Verb)
            {
                cmd.Verb = tokens.Current;
                tokens.MoveNext();
            }

            if (tokens.IsFinished) return;

            if (tokens.Current.MorphType == MorphType.Preposition)
            {
                cmd.Preposition = tokens.Current.Value;
            }

            while (tokens.Current.MorphType == MorphType.Preposition && tokens.MoveNext())
            {
            }

            if (tokens.IsFinished) return;

            ParseSubjectList(tokens, cmd);

            if (tokens.IsFinished) return;

            ParseAdverbList(tokens, cmd);

            if (tokens.IsFinished) return;

            while (tokens.Current.MorphType == MorphType.Preposition )
            {
                if (cmd.Preposition == null)
                {
                    cmd.Preposition = tokens.Current.Value;
                }
                if( !tokens.MoveNext() ) break;
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

            foreach (var morphType in _thesaurus.MorphTypes)
            {
                if (TryCreateToken(morphType.Key, morphType.Value, word, out token))
                {
                    foundToken = true;
                    break;
                }
            }

            return foundToken;
        }

        internal bool TryCreateToken(MorphType morphKeyType, IList<Token> tokens, string word, out Token token)
        {
            token = new Token();
            var foundToken = false;
            foreach (var morph in tokens)
            {
                if (CompareTokens(word, morph.Value))
                {
                    token = morph;
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

        internal void ParseAdverbList(TokenSet tokens, TokenCommand cmd)
        {
            var finished = false;

            if (tokens.Current.MorphType != MorphType.Adverb) return;

            while (!finished)
            {
                switch (tokens.Current.MorphType)
                {
                    case MorphType.Adverb:
                        cmd.Adverbs.Add(tokens.Current);
                        finished = !tokens.MoveNext();
                        break;
                    case MorphType.Conjunction:
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
            if (tokens.Current.MorphType != MorphType.Adjective) return;

            var finished = false;

            while (!finished)
            {
                switch (tokens.Current.MorphType)
                {
                    case MorphType.Adjective:
                        tokenObject.Adjectives.Add(tokens.Current);
                        finished = !tokens.MoveNext();
                        break;
                    case MorphType.Conjunction:
                        finished = !tokens.MoveNext();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
        }

        internal void ParseObjectList(TokenSet tokens, TokenCommand cmd)
        {
            var finished = false;

            while (!finished)
            {
                switch (tokens.Current.MorphType)
                {
                    case MorphType.Adjective:
                    case MorphType.Noun:
                    case MorphType.Pronoun:
                        ParseObject(tokens, cmd);
                        finished = tokens.IsFinished;
                        break;

                    case MorphType.Conjunction:
                    case MorphType.Article:
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
        internal bool ParseObject(TokenSet tokens, TokenCommand cmd)
        {
            var finished = false;
            var tokenObject = new TokenObject();

            if (tokens.Current.MorphType == MorphType.Adjective)
            {
                ParseAdjectiveList(tokens, tokenObject);
                finished = tokens.IsFinished;
            }

            if (!finished)
            {
                switch (tokens.Current.MorphType)
                {
                    case MorphType.Noun:
                        tokenObject.Noun = tokens.Current.Value;
                        tokens.MoveNext();
                        cmd.Objects.Add(tokenObject);
                        finished = true;
                        break;
                    case MorphType.Pronoun:
                        finished = ParseObjectPronoun(tokens.Current, tokenObject, cmd);
                        tokens.MoveNext();
                        break;
                }
            }

            return finished;
        }

        internal bool ParseObjectPronoun(Token pronounKey, TokenObject tokenObject, TokenCommand cmd)
        {
            var result = false;

            if (pronounKey.Value == _thesaurus.Keywords.PronounAll || pronounKey.Value == _thesaurus.Keywords.PronounEverything)
            {
                tokenObject.Noun = _thesaurus.Keywords.NounAll;
                cmd.Objects.Add(tokenObject);
                result = true;
            }
            else if (pronounKey.Value == _thesaurus.Keywords.PronounMe)
            {
                tokenObject.Noun = _thesaurus.Keywords.NounMyName;
                cmd.Objects.Add(tokenObject);
                result = true;
            }

            return result;
        }

        internal void ParseSubjectList(TokenSet tokens, TokenCommand cmd)
        {
            var finished = false;

            while (!finished)
            {
                switch (tokens.Current.MorphType)
                {
                    case MorphType.Adjective:
                    case MorphType.Noun:
                    case MorphType.Pronoun:
                        ParseSubject(tokens, cmd);
                        finished = tokens.IsFinished;
                        break;
                    case MorphType.Conjunction:
                    case MorphType.Article:
                        finished = !tokens.MoveNext();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
        }

        internal bool ParseSubject(TokenSet tokens, TokenCommand cmd)
        {
            var tokenObject = new TokenObject();

            var result = false;
            if (tokens.Current.MorphType == MorphType.Adjective)
            {
                ParseAdjectiveList(tokens, tokenObject);
                if (tokens.Current == null) return result;
            }

            switch (tokens.Current.MorphType)
            {
                case MorphType.Noun:
                    tokenObject.Noun = tokens.Current.Value;
                    tokens.MoveNext();
                    cmd.Subjects.Add(tokenObject);
                    result = true;
                    break;
                case MorphType.Pronoun:
                    result = ParseSubjectPronoun(tokens.Current, tokenObject, cmd);
                    tokens.MoveNext();
                    break;
            }

            return result;
        }

        internal bool ParseSubjectPronoun(Token pronoun, TokenObject tokenObject, TokenCommand cmd)
        {
            var result = false;

            if (pronoun.Value == _thesaurus.Keywords.PronounAll || pronoun.Value == _thesaurus.Keywords.PronounEverything)
            {
                tokenObject.Noun = _thesaurus.Keywords.NounAll;
                cmd.Subjects.Add(tokenObject);
                result = true;
            }
            else if (pronoun.Value == _thesaurus.Keywords.PronounMe)
            {
                tokenObject.Noun = _thesaurus.Keywords.NounMyName;
                cmd.Subjects.Add(tokenObject);
                result = true;
            }

            return result;
        }
    }
}