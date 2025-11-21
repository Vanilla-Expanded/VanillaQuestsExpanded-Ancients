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
        private float animCounter;
        private Vector3 topGraphicOffset;
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
                    glowGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/AncientWonderdoc/Wonderdoc_Glow", ShaderDatabase.MoteGlow, def.graphicData.drawSize, Color.white);
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
                malfunctionChance = Mathf.Clamp01(malfunctionChance + Rand.Range(0.1f, 0.5f));
            }
            Reset();
        }

        public override Vector3 PawnDrawOffset
        {
            get
            {
                if (Rotation == Rot4.North)
                {
                    return new Vector3(0f, 0f, 0f);
                }
                else if (Rotation == Rot4.East)
                {
                    return new Vector3(-0.2f, 0f, 0.2f);
                }
                else if (Rotation == Rot4.South)
                {
                    return new Vector3(0f, 0f, 0.3f);
                }
                else if (Rotation == Rot4.West)
                {
                    return new Vector3(0.2f, 0f, 0.2f);
                }
                return Vector3.zero;
            }
        }

        protected override void Tick()
        {
            base.Tick();
            if (topGraphicOffset == Vector3.zero)
            {
                topGraphicOffset = new Vector3(0f, 1f, 0f);
            }
            if (Occupant != null && selectedCycleIndex >= 0 && PowerOn)
            {
                var cycle = comp.Props.cycles[selectedCycleIndex];
                if (GlowGraphic.color != cycle.glowColor)
                {
                    glowGraphic = GlowGraphic.GetColoredVersion(GlowGraphic.Shader, cycle.glowColor, cycle.glowColor);
                }
                animCounter += 0.08f;
                float amplitude = 0.8f;
                float offsetValue = Mathf.Sin(animCounter + thingIDNumber) * amplitude;
                offsetValue += 0.5f;
                Vector3 offsetVector;
                if (Rotation == Rot4.North || Rotation == Rot4.South)
                {
                    offsetVector = new Vector3(0f, 1f, offsetValue);
                }
                else
                {
                    offsetVector = new Vector3(offsetValue, 1f, 0f);
                    if (Rotation == Rot4.East)
                    {
                        offsetVector.z += 0.4f;
                        offsetVector.x -= 0.4f;
                    }
                    else
                    {
                        offsetVector.z += 0.4f;
                        offsetVector.x -= 0.6f;
                    }
                }
                topGraphicOffset = offsetVector + new Vector3(0f, 1f, 0f);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Vector3 drawPos = DrawPos;
            bool shouldDrawGlow = false;
            if (Occupant != null && selectedCycleIndex >= 0 && PowerOn)
            {
                shouldDrawGlow = true;
            }
            TopGraphic.Draw(drawPos + topGraphicOffset, Rotation, this, 0f);
            if (shouldDrawGlow)
            {
                Vector3 glowOffset = topGraphicOffset - new Vector3(0f, 1f, 0f);
                GlowGraphic.Draw(drawPos + glowOffset, Rotation, this, 0f);
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
                yield return CreateInsertPawnGizmo("VQEA_InsertPerson", "VQEA_InsertPersonDesc", ContentFinder<Texture2D>.Get("UI/Gizmo/Gizmo_InsertPawnIntoWonderdoc"), "VQEA_NoPawnsAvailableForWonderdoc");
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
            if (Occupant != null && selectedCycleIndex >= 0)
            {
                stringBuilder.AppendLine("VQEA_WonderdocContains".Translate(Occupant.Name.ToStringFull));
                stringBuilder.AppendLine("VQEA_WonderdocCycle".Translate(comp.Props.cycles[selectedCycleIndex].label));
                stringBuilder.AppendLine("VQEA_WonderdocTimeRemaining".Translate(TicksRemaining.ToStringTicksToPeriod()));
            }
            stringBuilder.AppendLine("VQEA_ChanceToMalfunction".Translate(malfunctionChance.ToStringPercent()));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref malfunctionChance, "malfunctionChance", 0.1f);
            Scribe_Values.Look(ref selectedCycleIndex, "selectedCycleIndex", -1);
            Scribe_Values.Look(ref topGraphicOffset, "topGraphicOffset", Vector3.zero);
            Scribe_Values.Look(ref animCounter, "animCounter", 0f);
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
            Occupant?.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc + PawnDrawOffset, null, neverAimWeapon: true);
        }
    }
}
