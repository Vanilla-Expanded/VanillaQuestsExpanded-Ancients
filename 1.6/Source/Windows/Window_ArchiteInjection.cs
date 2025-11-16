using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class Window_ArchiteInjection : Window
    {
        private Building_ArchogenInjector archogenInjector;

        public override Vector2 InitialSize => new Vector2(820f, 720f);

        private const float LeftColumnWidth = 250f;
        private const float RightColumnWidth = 400f;
        private const float ColumnSpacing = 80f;
        private const float TopPadding = 30f;
        private const float BottomPadding = 20f;
        private const float SidePadding = 20f;
        private const float ButtonHeight = 40f;
        private const float SectionSpacing = 15f;

        public Window_ArchiteInjection(Building_ArchogenInjector injector)
        {
            archogenInjector = injector;
            forcePause = true;
            doCloseX = false;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float curY = 0f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(SidePadding, curY, inRect.width - SidePadding * 2f, TopPadding), "VQEA_ArchiteInjectionTitle".Translate());

            Text.Font = GameFont.Small;
            GUI.color = Color.grey;
            var injectorSize = Text.CalcSize(InternalDefOf.VQEA_ArchogenInjector.LabelCap).x;
            Widgets.Label(new Rect(inRect.width - injectorSize, curY, injectorSize, 24), InternalDefOf.VQEA_ArchogenInjector.LabelCap);
            GUI.color = Color.white;
            curY += TopPadding + 10f;

            float mainDescHeight = Text.CalcHeight("VQEA_ArchiteInjection_MainDesc".Translate(archogenInjector.Occupant.Named("PAWN")), inRect.width - SidePadding * 2f);
            Widgets.Label(new Rect(SidePadding, curY, inRect.width - SidePadding * 2f, mainDescHeight), "VQEA_ArchiteInjection_MainDesc".Translate(archogenInjector.Occupant.Named("PAWN")));
            curY += mainDescHeight + SectionSpacing;

            float columnHeight = inRect.height - curY - ButtonHeight - BottomPadding - 20f;
            Rect leftColumn = new Rect(SidePadding, curY, LeftColumnWidth, columnHeight);
            DoLeftColumn(leftColumn);
            Rect rightColumn = new Rect(leftColumn.xMax + ColumnSpacing, curY, RightColumnWidth, columnHeight);
            DoRightColumn(rightColumn);

            DoBottomButtons(new Rect(0, inRect.height - ButtonHeight - BottomPadding, inRect.width, ButtonHeight));
        }

        private void DoLeftColumn(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            Text.Font = GameFont.Small;

            listing.Label("VQEA_ExpectedInfusionDuration".Translate() + ": " + "VQEA_Days".Translate(Mathf.RoundToInt(archogenInjector.GetExpectedInfusionDuration() / (float)GenDate.TicksPerDay)));
            listing.Label("VQEA_InfusionComaDuration".Translate() + ": " + "VQEA_Days".Translate(Mathf.RoundToInt(archogenInjector.GetExpectedComaDuration() / (float)GenDate.TicksPerDay)));
            listing.Gap(SectionSpacing);

            listing.Label("VQEA_PawnGeneticComplexity".Translate(archogenInjector.Occupant.Name.ToStringFull) + ": " + archogenInjector.GetGeneticComplexityForUI());
            if (archogenInjector.Occupant.apparel.WornApparel.Any(x => x.def == InternalDefOf.VQEA_Apparel_PatientGown))
            {
                listing.Label("VQEA_PatientGownBonus".Translate().Colorize(ColorLibrary.Green));
            }
            int metabolism = archogenInjector.GetTargetMetabolismEfficiency();
            listing.Label("VQEA_ExpectedMetabolismSideEffect".Translate() + ": " + "VQEA_Metabolism".Translate(metabolism.ToStringWithSign()));
            listing.Gap(SectionSpacing);

            if (archogenInjector.HasLinkedFacility(InternalDefOf.VQEA_SpliceframeUplink))
            {
                listing.Label("VQEA_SuccessChance".Translate() + ": " + archogenInjector.GetOutcomeChance(InternalDefOf.VQEA_ArchiteInjection_Success).ToStringPercent().Colorize(ColorLibrary.Green));
                listing.Label("VQEA_RejectionChance".Translate() + ": " + archogenInjector.GetOutcomeChance(InternalDefOf.VQEA_ArchiteInjection_Rejection).ToStringPercent().Colorize(ColoredText.SubtleGrayColor));
                listing.Label("VQEA_SplicelingChance".Translate() + ": " + archogenInjector.GetOutcomeChance(InternalDefOf.VQEA_ArchiteInjection_Spliceling).ToStringPercent().Colorize(ColorLibrary.RedReadable));
                listing.Label("VQEA_SplicehulkChance".Translate() + ": " + archogenInjector.GetOutcomeChance(InternalDefOf.VQEA_ArchiteInjection_Splicehulk).ToStringPercent().Colorize(ColorLibrary.RedReadable));
                listing.Label("VQEA_SplicefiendChance".Translate() + ": " + archogenInjector.GetOutcomeChance(InternalDefOf.VQEA_ArchiteInjection_Splicefiend).ToStringPercent().Colorize(ColorLibrary.RedReadable));
            }
            listing.End();
        }

        private void DoRightColumn(Rect rect)
        {
            float curY = rect.y;
            float totalContentHeight = 0f;
            Widgets.Label(new Rect(rect.x, curY, rect.width, 32f), "VQEA_LinkedLabEquipment".Translate());
            curY += 32f;
            totalContentHeight += 32f;
            var compProperties = InternalDefOf.VQEA_ArchogenInjector.GetCompProperties<CompProperties_AffectedByFacilities>();
            var allLabEquipment = compProperties.linkableFacilities.ToList();
            var linkedEquipment = archogenInjector.GetLinkedLabEquipment();
            bool isEvenRow = false;

            foreach (var equipmentDef in allLabEquipment)
            {
                int count = linkedEquipment.Count(e => e.defName == equipmentDef.defName);
                int maxCount = GetMaxCountForEquipment(equipmentDef);
                float rowHeight = DrawEquipmentRow(rect, isEvenRow, equipmentDef, count, maxCount, curY);
                curY += rowHeight;
                totalContentHeight += rowHeight;
                isEvenRow = !isEvenRow;
            }
            curY += 2f;
            Rect boxRect = rect;
            boxRect.height = totalContentHeight;
            boxRect = boxRect.ExpandedBy(9f);
            boxRect.width += 10f;

            using (new TextBlock(new Color(0.25f, 0.25f, 0.25f)))
            {
                Widgets.DrawBox(boxRect, 2);
            }
            Rect infoTextRect = new Rect(rect.x, curY + 10f, rect.width, 60f);
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(infoTextRect, "VQEA_CompleteMoreAncientLabQuests".Translate());
            GUI.color = Color.white;
        }

        private float DrawEquipmentRow(Rect viewRect, bool isEvenRow, ThingDef equipmentDef, int count, int maxCount, float y)
        {
            const float rowHeight = 28f;
            Rect rowContentRect = new Rect(viewRect.x + 5, y, viewRect.width - 10, rowHeight);

            if (isEvenRow)
            {
                Widgets.DrawLightHighlight(rowContentRect);
            }

            Rect iconRect = new Rect(rowContentRect.x, rowContentRect.y + (rowContentRect.height - 24f) / 2f, 24f, 24f);
            Rect infoButtonRect = new Rect(iconRect.xMax + 4f, rowContentRect.y + (rowContentRect.height - 24f) / 2f, 24f, 24f);
            Rect countRect = new Rect(rowContentRect.xMax - 50f, rowContentRect.y, 50f, rowContentRect.height);
            Rect labelRect = new Rect(infoButtonRect.xMax + 6f, rowContentRect.y, countRect.x - (infoButtonRect.xMax + 6f), rowContentRect.height);

            Widgets.DrawTextureFitted(iconRect, equipmentDef.uiIcon, 1f);
            if (Widgets.ButtonImage(infoButtonRect, TexButton.Info))
            {
                Find.WindowStack.Add(new Dialog_InfoCard(equipmentDef));
            }

            if (count >= maxCount) GUI.color = ColorLibrary.Green;
            else if (count > 0) GUI.color = ColoredText.SubtleGrayColor;
            else GUI.color = ColorLibrary.RedReadable;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, equipmentDef.LabelCap);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(countRect, $"{count} / {maxCount}");

            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rowContentRect))
            {
                Widgets.DrawHighlight(rowContentRect);
                TooltipHandler.TipRegion(rowContentRect, new TipSignal(equipmentDef.description));
            }
            return rowHeight;
        }

        private int GetMaxCountForEquipment(ThingDef equipmentDef)
        {
            var facilityProps = equipmentDef.GetCompProperties<CompProperties_Facility>();
            if (facilityProps != null)
            {
                return facilityProps.maxSimultaneous;
            }
            return 1;
        }

        private void DoBottomButtons(Rect rect)
        {
            float buttonWidth = 140f;

            Rect cancelButtonRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
            {
                archogenInjector.CancelProcess();
                Close();
            }

            Rect injectButtonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);
            Rect textRect = new Rect(cancelButtonRect.xMax, rect.y, injectButtonRect.x - cancelButtonRect.xMax - 20f, rect.height);

            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = Color.grey;
            string capsuleText = "VQEA_RequiresArchiteCapsule".Translate() + "\n" + "VQEA_AvailableArchiteCapsules".Translate(Find.CurrentMap.resourceCounter.GetCount(ThingDefOf.ArchiteCapsule));
            Widgets.Label(textRect, capsuleText);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            bool noCapsules = Find.CurrentMap.resourceCounter.GetCount(ThingDefOf.ArchiteCapsule) == 0;
            if (noCapsules)
            {
                TooltipHandler.TipRegion(injectButtonRect, "VQEA_NoArchiteCapsules".Translate());
                Widgets.ButtonText(injectButtonRect, "VQEA_Inject".Translate(), true);
            }
            else
            {
                if (Widgets.ButtonText(injectButtonRect, "VQEA_Inject".Translate()))
                {
                    archogenInjector.ConfirmInjection();
                    Close();
                }
            }
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]

    public class HotSwappableAttribute : Attribute
    {
    }
}
