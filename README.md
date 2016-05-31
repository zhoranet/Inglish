# Inglish

This is C# port of 'Inglish language' parser created in 1983 for 'The Hobbit' C64 video game

Inspired by [bjarkig/the-hobbit](https://github.com/bjarkig/the-hobbit)

[Wiki: The Hobbit (1982 video game)](https://en.wikipedia.org/wiki/The_Hobbit_%281982_video_game%29)

    // load thesaurus
    var thesaurus = Thesaurus.Deserialize(File.ReadAllText(@"..\..\Dictionary.json"));
    var parser = new Parser(_thesaurus);
    // parse input text
    var command = parser.DoCommand("QUICKLY THROW LONG ROPE ACROSS NARROW RIVER");
    
    // verify result command 
    cmd.Verb.Value.Should().Be("throw");
    cmd.Adverbs[0].Value.Should().Be("quickly");
    cmd.Subjects[0].Noun.Should().Be("rope");
    cmd.Subjects[0].Adjectives[0].Value.Should().Be("long");
    cmd.Preposition.Should().Be("across");
    cmd.Objects[0].Noun.Should().Be("river");
    cmd.Objects[0].Adjectives[0].Value.Should().Be("narrow");