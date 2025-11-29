using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class InjectionBlacklistDef : Def
    {
        public List<string> blacklistedGenes = new List<string>();
        public List<string> blacklistedExclusionTags = new List<string>();
    }
}