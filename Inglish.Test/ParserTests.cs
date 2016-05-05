using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inglish.Test
{
    [TestClass]
    public class ParserTests
    {
        private Parser _parser;

        [TestInitialize]
        public void TestSetup()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void TryConvertToToken_CanConvert()
        {
            Token token;
            var result = _parser.TryConvertToToken("go", out token);

            result.Should().Be(true);
            token.Should().NotBeNull();
            token.GetKey<VerbKey>().Should().Be(VerbKey.Go);
            token.TokenType.Should().Be(TokenType.Verb);
        }

        [TestMethod]
        public void Tokenize_SingleWord_Pass()
        {
            var words = _parser.Tokenise("go");

            words.Length.Should().Be(1);
            words.First().Should().Be("go");
        }

        [TestMethod]
        public void Tokenize_WordsWithApostrophes_RemovedApostrophesAndPass()
        {
            var words = _parser.Tokenise("\"OPEN");

            words.Length.Should().Be(1);
            words.First().Should().Be("open");
        }

        [TestMethod]
        public void Tokenize_FourWords_Pass()
        {
            var words = _parser.Tokenise("go to the right");

            words.Length.Should().Be(4);
            words.First().Should().Be("go");
            words.ElementAt(1).Should().Be("to");
            words.ElementAt(2).Should().Be("the");
            words.ElementAt(3).Should().Be("right");
        }

        [TestMethod]
        public void Tokenize_ThreeWordsWithPunctuation_Pass()
        {
            var words = _parser.Tokenise("go to, the right.");

            words.Length.Should().Be(4);
            words.First().Should().Be("go");
            words.ElementAt(1).Should().Be("to");
            words.ElementAt(2).Should().Be("the");
            words.ElementAt(3).Should().Be("right");
        }

        [TestMethod]
        public void GetTokenSet_CorrectPhrase_SuccessfullyParsed()
        {
            var words = _parser.Tokenise("WITH THE SWORD CAREFULLY ATTACK THE TROLL.");
            var tokenSet = _parser.GetTokenSet(words);
            tokenSet.Should().NotBeNull();

            var tokens = tokenSet.GetTokens();

            tokens[0].TokenType.Should().Be(TokenType.Preposition);
            tokens[0].GetKey<PrepositionKey>().Should().Be(PrepositionKey.With);

            tokens[1].TokenType.Should().Be(TokenType.Article);
            tokens[1].GetKey<ArticleKey>().Should().Be(ArticleKey.The);

            tokens[2].TokenType.Should().Be(TokenType.Noun);
            tokens[2].GetKey<NounKey>().Should().Be(NounKey.Sword);

            tokens[3].TokenType.Should().Be(TokenType.Adverb);
            tokens[3].GetKey<AdverbKey>().Should().Be(AdverbKey.Carefully);

            tokens[4].TokenType.Should().Be(TokenType.Verb);
            tokens[4].GetKey<VerbKey>().Should().Be(VerbKey.Attack);

            tokens[5].TokenType.Should().Be(TokenType.Article);
            tokens[5].GetKey<ArticleKey>().Should().Be(ArticleKey.The);

            tokens[6].TokenType.Should().Be(TokenType.Noun);
            tokens[6].GetKey<NounKey>().Should().Be(NounKey.Troll);
        }

        [TestMethod]
        public void ParseSentence_CorrectPhrase_SuccessfullyParsed()
        {
            var words = _parser.Tokenise("WITH THE SWORD CAREFULLY ATTACK THE TROLL.");
            var tokenSet = _parser.GetTokenSet(words);
            var cmd = _parser.ParseSentence(tokenSet);

            cmd.Should().NotBeNull();
            cmd.Verb.Should().Be(VerbKey.Attack);
        }

        [TestMethod]
        public void ParseSentence_PhraseWithSayVerb_SuccessfullyParsed()
        {
            var words = _parser.Tokenise("SAY TO THORIN \"OPEN WINDOW\"");
            var tokenSet = _parser.GetTokenSet(words);
            var cmd = _parser.ParseSentence(tokenSet);

            cmd.Should().NotBeNull();
            cmd.Verb.Should().Be(VerbKey.Say);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Thorin);
            cmd.NpcTokens.Count.Should().Be(2);
            cmd.NpcTokens[0].GetKey<VerbKey>().Should().Be(VerbKey.Open);
            cmd.NpcTokens[1].GetKey<NounKey>().Should().Be(NounKey.Window);
        }

        [TestMethod]
        public void ParseSentence_SingleSayWord_NullCommand()
        {
            var tokenSet =
                new TokenSet(new[]
                {Token.Create(TokenType.Verb, VerbKey.Say), Token.Create(TokenType.Preposition, PrepositionKey.To)});
            var cmd = _parser.ParseSentence(tokenSet);
            cmd.Should().BeNull();
        }

        [TestMethod]
        public void ParseSentence_EmptyTokenSet_NullCommand()
        {
            var tokenSet = new TokenSet();
            var cmd = _parser.ParseSentence(tokenSet);

            cmd.Should().BeNull();
        }

        [TestMethod]
        public void DoCommand_Phrase_CanParse()
        {
            var cmd = _parser.DoCommand("QUICKLY THROW LONG ROPE ACROSS NARROW RIVER");
            cmd.Verb.Should().Be(VerbKey.Throw);
            cmd.Adverbs[0].Should().Be(AdverbKey.Quickly);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Rope);
            cmd.Subjects[0].Adjectives[0].Should().Be(AdjectiveKey.Long);
            cmd.Preposition.Should().Be(PrepositionKey.Across);
            cmd.Objects[0].Noun.Should().Be(NounKey.River);
            cmd.Objects[0].Adjectives[0].Should().Be(AdjectiveKey.Narrow);
        }

        [TestMethod]
        public void DoCommand_UsePronoun_CanParse()
        {
            var cmd = _parser.DoCommand("TAKE KEY WITH ME");
            cmd.Verb.Should().Be(VerbKey.Take);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Key);
            //cmd.Preposition.Should().Be(PrepositionKey.With);
            cmd.Objects[0].Noun.Should().Be(NounKey.Hobbit);
        }

        [TestMethod]
        public void DoCommand_UseConjuctionBetweenAdjectives_CanParse()
        {
            var cmd = _parser.DoCommand("READ HIDDEN AND VICIOUS MAP");
            cmd.Verb.Should().Be(VerbKey.Read);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Map);
            cmd.Subjects[0].Adjectives[0].Should().Be(AdjectiveKey.Hidden);
            cmd.Subjects[0].Adjectives[1].Should().Be(AdjectiveKey.Vicious);
        }

        [TestMethod]
        public void DoCommand_UseObjectPronounAll_CanParse()
        {
            var cmd = _parser.DoCommand("OPEN THE GREEN BARREL WITH EVERYTHING");
            cmd.Verb.Should().Be(VerbKey.Open);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Barrel);
            cmd.Objects[0].Noun.Should().Be(NounKey._ALL_);
        }

        [TestMethod]
        public void DoCommand_SayPhraseWithPronoun_CanParse()
        {
            var cmd = _parser.DoCommand("SAY TO GANDALF \"GIVE ME MAP\"");
            cmd.Verb.Should().Be(VerbKey.Say);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Gandalf);

            cmd.NpcTokens[0].TokenType.Should().Be(TokenType.Verb);
            cmd.NpcTokens[0].GetKey<VerbKey>().Should().Be(VerbKey.Give);

            cmd.NpcTokens[1].TokenType.Should().Be(TokenType.Pronoun);
            cmd.NpcTokens[1].GetKey<PronounKey>().Should().Be(PronounKey.Me);
        }

        [TestMethod]
        public void DoCommand_UseConjuction_CanParse()
        {
            var cmd = _parser.DoCommand("Get rope and sword");
            cmd.Verb.Should().Be(VerbKey.Get);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Rope);
            cmd.Subjects[1].Noun.Should().Be(NounKey.Sword);
        }

        [TestMethod]
        public void DoCommand_UseConjuction2_CanParse()
        {
            var cmd = _parser.DoCommand("take the map and run");
            cmd.Verb.Should().Be(VerbKey.Take);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Map);
        }

        [TestMethod]
        public void DoCommand_UseEverything_CanParse()
        {
            var cmd = _parser.DoCommand("drop everything");
            cmd.Verb.Should().Be(VerbKey.Drop);
            cmd.Subjects[0].Noun.Should().Be(NounKey._ALL_);
        }

        [TestMethod]
        public void DoCommand_DirectPhraseWithPronoun_CanParse()
        {
            var cmd = _parser.DoCommand("GIVE ME MAP");
            cmd.Verb.Should().Be(VerbKey.Give);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Hobbit);
            cmd.Subjects[1].Noun.Should().Be(NounKey.Map);
        }

        [TestMethod]
        public void ParseCommand_SingleNounToken_CanParse()
        {
            var cmd = new Command();

            var tokens = new TokenSet(new[] {new Token(TokenType.Noun, (long) NounKey.Map)});
            tokens.MoveNext();
            _parser.ParseCommand(tokens, cmd);
            cmd.Subjects.Should().HaveCount(1);
            cmd.Subjects[0].Noun.Should().Be(NounKey.Map);
        }
    }
}