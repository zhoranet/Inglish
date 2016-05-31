using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inglish.Test
{
    [TestClass]
    public class ParserTests
    {
        private Parser _parser;
        private IThesaurus _thesaurus;

        [TestInitialize]
        public void TestSetup()
        {
            _thesaurus = Thesaurus.Deserialize(File.ReadAllText(@"..\..\Dictionary.json"));
            _parser = new Parser(_thesaurus);
        }

        [TestMethod]
        public void TryConvertToToken_CanConvert()
        {
            Token token;
            var result = _parser.TryConvertToToken("go", out token);

            result.Should().Be(true);
            token.Should().NotBeNull();
            token.Value.Should().Be("go");
            token.MorphType.Should().Be(MorphType.Verb);
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

            tokens[0].MorphType.Should().Be(MorphType.Preposition);
            tokens[0].Value.Should().Be("with");

            tokens[1].MorphType.Should().Be(MorphType.Article);
            tokens[1].Value.Should().Be("the");

            tokens[2].MorphType.Should().Be(MorphType.Noun);
            tokens[2].Value.Should().Be("sword");

            tokens[3].MorphType.Should().Be(MorphType.Adverb);
            tokens[3].Value.Should().Be("carefully");

            tokens[4].MorphType.Should().Be(MorphType.Verb);
            tokens[4].Value.Should().Be("attack");

            tokens[5].MorphType.Should().Be(MorphType.Article);
            tokens[5].Value.Should().Be("the");

            tokens[6].MorphType.Should().Be(MorphType.Noun);
            tokens[6].Value.Should().Be("troll");
        }

        [TestMethod]
        public void ParseSentence_CorrectPhrase_SuccessfullyParsed()
        {
            var words = _parser.Tokenise("CAREFULLY ATTACK THE TROLL WITH THE SWORD.");
            var tokenSet = _parser.GetTokenSet(words);
            var cmd = _parser.ParseSentence(tokenSet);

            cmd.Should().NotBeNull();
            cmd.Verb.Value.Should().Be("attack");
        }

        [TestMethod]
        public void ParseSentence_PhraseWithSayVerb_SuccessfullyParsed()
        {
            var words = _parser.Tokenise("SAY TO THORIN \"OPEN WINDOW\"");
            var tokenSet = _parser.GetTokenSet(words);
            var cmd = _parser.ParseSentence(tokenSet);

            cmd.Should().NotBeNull();
            cmd.Verb.Value.Should().Be("say");
            cmd.Subjects[0].Noun.Should().Be("thorin");
            cmd.NpcTokens.Count.Should().Be(2);
            cmd.NpcTokens[0].Value.Should().Be("open");
            cmd.NpcTokens[1].Value.Should().Be("window");
        }

        [TestMethod]
        public void ParseSentence_SingleSayWord_NullCommand()
        {
            var tokenSet =
                new TokenSet(new[] 
                {new Token() { Index = 0, MorphType = MorphType.Verb, Value = "say"}, 
                new Token() { Index = 0, MorphType = MorphType.Preposition, Value = "to"}});
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
            cmd.Verb.Value.Should().Be("throw");
            cmd.Adverbs[0].Value.Should().Be("quickly");
            cmd.Subjects[0].Noun.Should().Be("rope");
            cmd.Subjects[0].Adjectives[0].Value.Should().Be("long");
            cmd.Preposition.Should().Be("across");
            cmd.Objects[0].Noun.Should().Be("river");
            cmd.Objects[0].Adjectives[0].Value.Should().Be("narrow");
        }

        [TestMethod]
        public void DoCommand_UsePronoun_CanParse()
        {
            var cmd = _parser.DoCommand("TAKE KEY WITH ME");
            cmd.Verb.Value.Should().Be("take");
            cmd.Subjects[0].Noun.Should().Be("key");
            //cmd.Preposition.Should().Be(PrepositionKey.With);
            cmd.Objects[0].Noun.Should().Be("hobbit");
        }

        [TestMethod]
        public void DoCommand_UseConjuctionBetweenAdjectives_CanParse()
        {
            var cmd = _parser.DoCommand("READ HIDDEN AND VICIOUS MAP");
            cmd.Verb.Value.Should().Be("read");
            cmd.Subjects[0].Noun.Should().Be("map");
            cmd.Subjects[0].Adjectives[0].Value.Should().Be("hidden");
            cmd.Subjects[0].Adjectives[1].Value.Should().Be("vicious");
        }

        [TestMethod]
        public void DoCommand_UseObjectPronounAll_CanParse()
        {
            var cmd = _parser.DoCommand("OPEN THE GREEN BARREL WITH EVERYTHING");
            cmd.Verb.Value.Should().Be("open");
            cmd.Subjects[0].Noun.Should().Be("barrel");
            cmd.Objects[0].Noun.Should().Be("all");
        }

        [TestMethod]
        public void DoCommand_SayPhraseWithPronoun_CanParse()
        {
            var cmd = _parser.DoCommand("SAY TO GANDALF \"GIVE ME MAP\"");
            cmd.Verb.Value.Should().Be("say");
            cmd.Subjects[0].Noun.Should().Be("gandalf");

            cmd.NpcTokens[0].MorphType.Should().Be(MorphType.Verb);
            cmd.NpcTokens[0].Value.Should().Be("give");

            cmd.NpcTokens[1].MorphType.Should().Be(MorphType.Pronoun);
            cmd.NpcTokens[1].Value.Should().Be("me");
        }

        [TestMethod]
        public void DoCommand_UseConjuction_CanParse()
        {
            var cmd = _parser.DoCommand("Get rope and sword");
            cmd.Verb.Value.Should().Be("get");
            cmd.Subjects[0].Noun.Should().Be("rope");
            cmd.Subjects[1].Noun.Should().Be("sword");
        }

        [TestMethod]
        public void DoCommand_UseConjuction2_CanParse()
        {
            var cmd = _parser.DoCommand("take the map and run");
            cmd.Verb.Value.Should().Be("take");
            cmd.Subjects[0].Noun.Should().Be("map");
        }

        [TestMethod]
        public void DoCommand_UseEverything_CanParse()
        {
            var cmd = _parser.DoCommand("drop everything");
            cmd.Verb.Value.Should().Be("drop");
            cmd.Subjects[0].Noun.Should().Be("all");
        }

        [TestMethod]
        public void DoCommand_DirectPhraseWithPronoun_CanParse()
        {
            var cmd = _parser.DoCommand("GIVE ME MAP");
            cmd.Verb.Value.Should().Be("give");
            cmd.Subjects[0].Noun.Should().Be("hobbit");
            cmd.Subjects[1].Noun.Should().Be("map");
        }

        [TestMethod]
        public void ParseCommand_SingleNounToken_CanParse()
        {
            var cmd = new TokenCommand();

            var tokens = new TokenSet(new[] {new Token { MorphType = MorphType.Noun, Value = "map"}});
            tokens.MoveNext();
            _parser.ParseCommand(tokens, cmd);
            cmd.Subjects.Should().HaveCount(1);
            cmd.Subjects[0].Noun.Should().Be("map");
        }
    }
}