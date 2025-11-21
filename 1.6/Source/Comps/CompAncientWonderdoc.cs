using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VanillaQuestsExpandedAncients
{
    [StaticConstructorOnStartup]
    public class CompAncientWonderdoc : ThingComp
    {
        public CompProperties_AncientWonderdoc Props => (CompProperties_AncientWonderdoc)props;
    }
}
