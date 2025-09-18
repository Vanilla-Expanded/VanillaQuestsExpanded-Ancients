using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class AncientPipelineOff : Building
    {

        public int counterToChange = 0;
        public int tickCounter = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.counterToChange, "counterToChange");
            Scribe_Values.Look(ref this.tickCounter, "tickCounter");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                counterToChange = new IntRange(90000, 240000).RandomInRange;
            }
        }

        protected override void Tick()
        {
            base.Tick();

            if (tickCounter > counterToChange)
            {

                Notify_ChangeToActive();
            }

            tickCounter++;

        }

        public void Notify_ChangeToActive()
        {
            GenSpawn.Spawn(ThingMaker.MakeThing(InternalDefOf.VQEA_AncientPipelineJunction_On), Position, Map, Rotation);

            if (this.Spawned)
            {
                this.DeSpawn();
            }
        }

    }
}
