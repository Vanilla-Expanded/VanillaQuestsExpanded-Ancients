using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VEF;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;


namespace VanillaQuestsExpandedAncients
{
    public class Building_Containment : Building_Trap
    {

        public bool signalSprung = false;
        public int deletionCounter = 0;
        private Effecter wakeUpEffect;

        ContainmentDetails cachedExtension;

        public ContainmentDetails CachedExtension
        {

            get
            {
                if (cachedExtension is null)
                {
                    cachedExtension = this.def.GetModExtension<ContainmentDetails>();
                }
                return cachedExtension;
            }

        }


        protected override void Tick()
        {

            if (signalSprung)
            {
                deletionCounter++;
                wakeUpEffect?.EffectTick(this, this);
                if (deletionCounter > 30)
                {
                    IntVec3 pos = this.PositionHeld;
                    Map map = this.Map;

                    PopUpMonster(pos, map);

                    if (CachedExtension.buildingLeft != null)
                    {


                        Thing buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(CachedExtension.buildingLeft), Position, Map, Rotation);

                        if (buildingToMake.def.CanHaveFaction)
                        {
                            buildingToMake.SetFaction(this.Faction);
                        }
                    }


                    if (this.Spawned)
                    {
                        this.Destroy();
                    }

                }
            }
            if (!signalSprung && Spawned && this.IsHashIntervalTick(10))
            {
                int numCells = GenRadial.NumCellsInRadius(6);
                for (int i = 0; i < numCells; i++)
                {
                    IntVec3 intVec = this.Position + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(this.Map))
                    {
                        foreach (Thing thing in intVec.GetThingList(this.Map))
                        {
                            if (thing != null && thing is Pawn detectedPawn && detectedPawn.RaceProps.Humanlike && detectedPawn.GetRoom() == this.GetRoom())
                            {
                                this.SpringSub(detectedPawn);
                               
                            }
                        }


                    }
                }
            }
          

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.signalSprung, "signalSprung");

        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);

            this.SpringSub(null);
        }


        protected override void SpringSub(Pawn p)
        {

            wakeUpEffect = InternalDefOf.CocoonWakingUp.SpawnAttached(this, this.Map);
            signalSprung = true;

        }

        public void PopUpMonster(IntVec3 pos, Map map)
        {
            if (CachedExtension.deconstructSound != null)
            {
                CachedExtension.deconstructSound.PlayOneShot(this);
            }


            Pawn pawn = PawnGenerator.GeneratePawn(CachedExtension.containmentSpawn);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(pos, map, 1), map);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);

            

        }




    }
}
