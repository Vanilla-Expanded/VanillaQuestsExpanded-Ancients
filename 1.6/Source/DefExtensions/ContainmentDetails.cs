using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaQuestsExpandedAncients
{

    public class ContainmentDetails : DefModExtension
    {

        public ThingDef buildingLeft = null;
        public SoundDef deconstructSound = null;
        public PawnKindDef containmentSpawn;
        public int numToSpawn = 1;

    }


}
