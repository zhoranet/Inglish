namespace Inglish.Test
{
    internal class TestKeywords : IKeywords
    {
        public TestKeywords()
        {
            VerbSay = "say";
            VerbTalk = "talk";
            PronounAll = "all";
            PronounEverything = "everything";
            PronounMe = "me";
            NounMyName = "hobbit";
            NounAll = "all";
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