using System.Collections.Generic;

namespace Inglish
{
    public interface IThesaurus
    {
        IKeywords Keywords { get; }
        Dictionary<MorphType, IList<Token>> MorphTypes { get; }
    }
}