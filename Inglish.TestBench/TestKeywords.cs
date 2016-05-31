namespace Inglish.Test
{
    internal class TestKeywords : IKeywords
    {
        public TestKeywords()
        {
            VerbSay = "Say";
            VerbTalk = "Talk";
            PronounAll = "All";
            PronounEverything = "Everything";
            PronounMe = "Me";
            NounMyName = "Hobbit";
            NounAll = "All";
        }
        public string VerbSay { get; }
        public string VerbTalk { get; }
        public string PronounAll { get; }
        public string PronounEverything { get; }
        public string PronounMe { get; }
        public string NounMyName { get; }
        public string NounAll { get; }
    }
}