using System;
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
    public class CompBioBattery : CompPowerPlant, IThingHolderWithDrawnPawn, ISuspendableThingHolder
    {
        [Unsaved(false)]
        private Effecter bubbleEffecter;

        [Unsaved(false)]
        private Sustainer sustainerWorking;

        private ThingOwner innerContainer;
        private float massLeft = -1;
        private int ticksTillConsume = -1;
        private static List<ThingDef> cachedBatteries;

        public CompBioBattery() => innerContainer = new ThingOwner<Thing>(this);

        public Pawn Occupant => innerContainer.OfType<Pawn>().FirstOrDefault();
        protected override float DesiredPowerOutput => Occupant is null ? 0 : base.DesiredPowerOutput * Occupant.RaceProps.baseBodySize;

        public bool IsContentsSuspended => true;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public float HeldPawnDrawPos_Y => parent.def.altitudeLayer.AltitudeFor(Altitudes.AltInc);
        public float HeldPawnBodyAngle => Rot4.North.AsAngle;
        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        public override void CompTick()
        {
            base.CompTick();
            ticksTillConsume--;
            if (ticksTillConsume == 0)
            {
                massLeft--;
                ticksTillConsume = 2500;
            }

            if (massLeft <= 0f)
            {
                innerContainer.ClearAndDestroyContents();
                ticksTillConsume = -1;
                massLeft = -1;
            }

            if (Occupant != null && this.parent.Map!=null)
            {
                if (bubbleEffecter == null)
                {
                    bubbleEffecter = InternalDefOf.VQEA_Bubbles.SpawnAttached(this.parent, this.parent.MapHeld);
                }
                bubbleEffecter.EffectTick(this.parent, this.parent);
                if (sustainerWorking == null || sustainerWorking.Ended)
                {
                    sustainerWorking = SoundDefOf.GrowthVat_Working.TrySpawnSustainer(SoundInfo.InMap(this.parent, MaintenanceType.PerTick));
                }
                else
                {
                    sustainerWorking.Maintain();
                }
            }
        }

        public void InsertPawn(Pawn pawn)
        {
            innerContainer.TryAddOrTransfer(pawn, false);
            massLeft = pawn.GetStatValue(StatDefOf.Mass);
            ticksTillConsume = 2500;
            while (HealthUtility.FixWorstHealthCondition(pawn) != null) ;
            if (pawn.Downed)
            {
                var hediffs = pawn.health?.hediffSet?.hediffs;
                if (hediffs != null)
                {
                    for (var i = hediffs.Count - 1; i >= 0; i--)
                    {
                        var capMod = hediffs[i].CapMods?.FirstOrDefault(x => x.capacity == PawnCapacityDefOf.Consciousness);
                        if (capMod != null)
                        {
                            if (capMod.setMax < 1 || capMod.offset < 0)
                            {
                                pawn.health.RemoveHediff(hediffs[i]);
                            }
                        }
                    }
                }
            }
        }

        public virtual bool CanAcceptPawn(Pawn pawn) => Occupant is null;

       

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksTillConsume, "ticksTillConsume");
            Scribe_Values.Look(ref massLeft, "massLeft");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }

        public override string CompInspectStringExtra() => base.CompInspectStringExtra() + (Occupant is not null
            ? "\n" + "VQEA_MassLeft".Translate(massLeft.ToStringMass(), Occupant.GetStatValue(StatDefOf.Mass).ToStringMass(), (
                massLeft / Occupant.GetStatValue(StatDefOf.Mass)).ToStringPercent()) + "\n" + "Contains".Translate() + ": " + Occupant.NameShortColored.Resolve().CapitalizeFirst()
            : "");

        public static Thing FindBatteryFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            if (cachedBatteries == null)
            {
                cachedBatteries = DefDatabase<ThingDef>.AllDefs.Where(def => def.comps.Any(comp => comp.compClass == typeof(CompBioBattery))).ToList();
            }
            foreach (ThingDef cachedBattery in cachedBatteries)
            {
                bool queuing = KeyBindingDefOf.QueueOrder.IsDownEvent;
                Thing building_Battery = GenClosest.ClosestThingReachable(p.PositionHeld, p.MapHeld, ThingRequest.ForDef(cachedBattery), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, Validator);
                if (building_Battery != null)
                {
                    return building_Battery;
                }
                bool Validator(Thing x)
                {
                    if (x.TryGetComp<CompBioBattery>().Occupant is null && (!queuing || !traveler.HasReserved(x)))
                    {
                        return traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations);
                    }
                    return false;
                }
            }
            return null;
        }


      

        public override void PostDraw()
        {
            base.PostDraw();
            var s = new Vector3(parent.def.graphicData.drawSize.x, 1f, parent.def.graphicData.drawSize.y);
            var drawPos = parent.DrawPos;
            drawPos.y += Altitudes.AltInc * 2;
            GraphicsCache.graphicTop.DrawFromDef(drawPos, Rot4.North, null);
            
            if (Occupant is null) return;
            var drawLoc = parent.DrawPos;
            drawLoc.y += Altitudes.AltInc;
            drawLoc.z += 0.25f;
            if (Occupant.RaceProps.Humanlike)
            {
                Occupant.Drawer.renderer.DynamicDrawPhaseAt(DrawPhase.Draw, drawLoc, null, neverAimWeapon: true);
            }
            else
            {
                Vector2 size = Occupant.kindDef.lifeStages.Last().bodyGraphicData.drawSize.magnitude > (parent.def.graphicData.drawSize / 2).magnitude ? parent.def.graphicData.drawSize / 2 : Occupant.kindDef.lifeStages.Last().bodyGraphicData.drawSize;
                Graphic graphic = Occupant.kindDef.lifeStages.Last().bodyGraphicData.Graphic.GetCopy(size, null);
                graphic?.DrawFromDef(drawLoc, Rot4.South, null);

            }
            
          
        }
    }
}