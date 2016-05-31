using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Inglish.Test
{
    public class Thesaurus : IThesaurus
    {
        public Thesaurus()
        {
            MorphTypes = new Dictionary<MorphType, IList<Token>>();
            Keywords = new TestKeywords();
        }
        
        public IKeywords Keywords { get; set; }

        public Dictionary<MorphType, IList<Token>> MorphTypes { get; set; }

        public static Thesaurus Deserialize(string jsonText)
        {
            var thesaurus = new Thesaurus();

            var jsonRoot  = JObject.Parse(jsonText);
            var jsonMorphTypes  = (JObject) jsonRoot["morphTypes"];

            foreach (var jsonMorphType in jsonMorphTypes)
            {
                var morphType = GetMorphType(jsonMorphType.Key);

                var morphTokens = new List<Token>();

                for (var i = 0; i <jsonMorphType.Value.Count(); i++)
                {
                    morphTokens.Add(new Token { Index = i, Value = jsonMorphType.Value[i].Value<string>(), MorphType = morphType});
                }

                thesaurus.MorphTypes.Add(morphType, morphTokens);
            }
            
            return thesaurus;
        }

        private static MorphType GetMorphType(string key)
        {
            return (MorphType) Enum.Parse(typeof (MorphType), key, true);
        }

        public static string Serialize(Thesaurus thesaurus)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(thesaurus, Formatting.Indented, settings);
        }
    }
}