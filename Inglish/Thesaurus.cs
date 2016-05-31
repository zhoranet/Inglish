using System.Collections.Generic;

namespace Inglish
{
    public class Thesaurus : IThesaurus
    {
        public IKeywords Keywords { get; set; }

        public Dictionary<MorphType, IList<Token>> MorphTypes { get; set; }
    }
}