using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Building_AncientWonderdoc : Building_PawnProcessor, IThingHolderWithDrawnPawn
    {
        private float malfunctionChance;
        private int selectedCycleIndex = -1;
        private CompAncientWonderdoc comp;
        public float HeldPawnDrawPos_Y => DrawPos.y + 0.03658537f;
        public float HeldPawnBodyAngle => Rotation.AsAngle;
        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;
        private Graphic topGraphic;
        private Graphic TopGraphic
        {
            get
            {
                if (topGraphic == null)
                {
                    topGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/AncientWonderdoc/Wonderdoc_Top", ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
                }
                return topGraphic;
            }
        }

        private Graphic glowGraphic;
        private Graphic GlowGraphic
        {
            get
            {
                if (glowGraphic == null)
                {
                    glowGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/AncientWonderdoc/Wonderdoc_Glow", ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
                }
                return glowGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            comp = GetComp<CompAncientWonderdoc>();
            if (!respawningAfterLoad)
            {
                malfunctionChance = Rand.Range(0.1f, 0.5f);
            }
        }

        public override AcceptanceReport CanAcceptPawn(Pawn p)
        {
            if (SelectedPawn != null && SelectedPawn != p)
            {
                return false;
            }
            if (p.DevelopmentalStage.Baby())
            {
                return "SubcoreScannerBabyNotAllowed".Translate();
            }
            if (!p.RaceProps.Humanlike)
            {
                return false;
            }
            return true;
        }

        protected override void OnAccept(Pawn p)
        {
            base.OnAccept(p);
            StartProcessing(comp.Props.cycles[selectedCycleIndex].durationTicks);
        }

        protected override void FinishProcess()
        {
            var pawn = Occupant;
            EjectPawn();
            if (Rand.Chance(malfunctionChance))
            {
                comp.Props.cycles[selectedCycleIndex].ApplyOnMalfunction(pawn, this);
                var map = Map;
                var pos = Position;
                var rot = Rotation;
                var fac = Faction;
                Destroy();
                var busted = ThingMaker.MakeThing(InternalDefOf.VQEA_BustedAncientWonderdoc);
                busted.SetFaction(fac);
                GenSpawn.Spawn(busted, pos, map, rot);
            }
            else
            {
                comp.Props.cycles[selectedCycleIndex].ApplyOnSuccess(pawn, this);
                malfunctionChance += Rand.Range(0.1f, 0.5f);
            }
            Reset();
        }

        public override Vector3 PawnDrawOffset => Vector3.zero;

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (SelectedPawn != null && selectedCycleIndex >= 0)
            {
                var cycle = comp.Props.cycles[selectedCycleIndex];
                Vector3 drawPos = DrawPos;
                if (GlowGraphic.color != cycle.glowColor)
                {
                    glowGraphic = GlowGraphic.GetColoredVersion(GlowGraphic.Shader, cycle.glowColor, cycle.glowColor);
                }
                
                float speed = 0.05f;
                float amplitude = 0.8f;
                float offsetValue = Mathf.Sin((Find.TickManager.TicksGame + thingIDNumber) * speed) * amplitude;
                offsetValue += 0.5f;
                Vector3 offsetVector;
                if (Rotation == Rot4.North || Rotation == Rot4.South)
                {
                    offsetVector = new Vector3(0, 1, offsetValue);
                }
                else
                {
                    offsetVector = new Vector3(offsetValue, 1, 0);
                }
                
                Vector3 finalPos = drawPos + offsetVector;
                GlowGraphic.Draw(finalPos, Rotation, this, 0f);
                TopGraphic.Draw(finalPos + new Vector3(0, 1, 0), Rotation, this, 0f);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (selectedCycleIndex >= 0)
            {
                yield return CreateInsertPawnGizmo("VQEA_InsertPerson", "VQEA_InsertPersonDesc", ContentFinder<Texture2D>.Get("UI/Icons/InsertPersonSubcoreScanner"), "VQEA_NoPawnsAvailableForWonderdoc");
            }
            else
            {
                for (int i = 0; i < comp.Props.cycles.Count; i++)
                {
                    var cycle = comp.Props.cycles[i];
                    Command_Action command = new Command_Action
                    {
                        defaultLabel = cycle.label,
                        defaultDesc = cycle.description,
                        icon = cycle.Icon,
                        action = () =>
                        {
                            selectedCycleIndex = comp.Props.cycles.IndexOf(cycle);
                        }
                    };
                    yield return command;
                }
            }

            foreach (var gizmo in GetPawnProcessorGizmos())
            {
                yield return gizmo;
            }
        }

        protected override void Reset()
        {
            base.Reset();
            selectedCycleIndex = -1;
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString() + "\n");
            Log.Message("selectedCycleIndex: " + selectedCycleIndex);
            if (Occupant != null && selectedCycleIndex >= 0)
            {
                stringBuilder.AppendLine("VQEA_WonderdocContains".Translate(Occupant.Name.ToStringFull));
                stringBuilder.AppendLine("VQEA_WonderdocCycle".Translate(comp.Props.cycles[selectedCycleIndex].label));
                stringBuilder.AppendLine("VQEA_WonderdocTimeRemaining".Translate(TicksRemaining.ToStringTicksToPeriod()));
                stringBuilder.AppendLine("VQEA_ChanceToMalfunction".Translate(malfunctionChance.ToStringPercent()));
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref malfunctionChance, "malfunctionChance", 0.1f);
            Scribe_Values.Look(ref selectedCycleIndex, "selectedCycleIndex", -1);
        }

        protected override SoundDef GetOperatingSound()
        {
            return InternalDefOf.SubcoreSoftscanner_Working;
        }

        protected override SoundDef GetStartSound()
        {
            return InternalDefOf.SubcoreSoftscanner_Start;
        }

        protected override bool ShouldShowProgressBar()
        {
            return SelectedPawn != null && selectedCycleIndex >= 0;
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            Occupant?.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc, null, neverAimWeapon: true);
        }
    }
}
