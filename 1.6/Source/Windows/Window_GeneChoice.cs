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
    public class Window_GeneChoice : Window
    {
        private List<GeneDef> architeGenes;
        private List<GeneDef> sideEffectGenes;
        private GeneDef selectedArchiteGene;
        private GeneDef selectedSideEffectGene;
        private Action<GeneDef, GeneDef> onGenesSelected;

        private static float GeneRectSize => 140f;
        private static float GeneRectGap = 15f;
        private static float SectionHeight => GeneRectSize + GeneRectGap + 30;

        public override Vector2 InitialSize
        {
            get
            {
                float height = 130f;
                if (architeGenes.Any()) height += SectionHeight;
                if (sideEffectGenes.Any()) height += SectionHeight;
                return new Vector2(350f, height);
            }
        }
        public Window_GeneChoice(List<GeneDef> architeGenes, List<GeneDef> sideEffectGenes, Action<GeneDef, GeneDef> onGenesSelected)
        {
            this.architeGenes = architeGenes;
            this.sideEffectGenes = sideEffectGenes;
            this.onGenesSelected = onGenesSelected;
            this.forcePause = true;
            this.doCloseX = false;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.closeOnCancel = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "VQEA_ChooseGenesTitle".Translate());

            Text.Font = GameFont.Small;
            float curY = 40f;

            if (architeGenes.Any())
            {
                DrawGeneSection(new Rect(0, curY, inRect.width, SectionHeight), "VQEA_ArchiteGenes".Translate(), architeGenes, ref selectedArchiteGene, "VQEA_NoArchiteGenes".Translate());
                curY += SectionHeight + 10f;
            }

            if (sideEffectGenes.Any())
            {
                DrawGeneSection(new Rect(0, curY, inRect.width, SectionHeight), "VQEA_SideEffectGenes".Translate(), sideEffectGenes, ref selectedSideEffectGene, "VQEA_NoSideEffectGenes".Translate());
            }

            Rect buttonRect = new Rect(inRect.width / 2f - 100f, inRect.height - 32f, 200f, 32f);
            if (Widgets.ButtonText(buttonRect, "Confirm".Translate()))
            {
                bool architeRequirementMet = !architeGenes.Any() || selectedArchiteGene != null;
                bool sideEffectRequirementMet = !sideEffectGenes.Any() || selectedSideEffectGene != null;
                if (architeRequirementMet && sideEffectRequirementMet)
                {
                    onGenesSelected(selectedArchiteGene, selectedSideEffectGene);
                    Close();
                }
                else
                {
                    if (!architeRequirementMet && !sideEffectRequirementMet)
                    {
                        Messages.Message("VQEA_MustSelectGenes".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (!architeRequirementMet)
                    {
                        Messages.Message("VQEA_MustSelectArchiteGene".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        Messages.Message("VQEA_MustSelectSideEffectGene".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                }
            }
        }

        private void DrawGeneSection(Rect rect, string label, List<GeneDef> genes, ref GeneDef selectedGene, string noGenesLabel)
        {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);
            Rect outerRect = new Rect(rect.x, rect.y + 24f, rect.width, rect.height - 24f);
            Widgets.DrawMenuSection(outerRect);

            if (genes != null && genes.Any())
            {
                float totalWidth = genes.Count * GeneRectSize + (genes.Count - 1) * GeneRectGap;
                float startX = outerRect.x + (outerRect.width - totalWidth) / 2f;
                float startY = outerRect.y + (outerRect.height - GeneRectSize) / 2f;

                for (int i = 0; i < genes.Count; i++)
                {
                    Rect geneRect = new Rect(startX + i * (GeneRectSize + GeneRectGap), startY, GeneRectSize, GeneRectSize);

                    GeneType geneType = (genes[i].biostatArc > 0) ? GeneType.Endogene : GeneType.Xenogene;
                    GeneUIUtility.DrawGeneDef(genes[i], geneRect, geneType, doBackground: true, clickable: false);

                    if (selectedGene == genes[i])
                    {
                        Widgets.DrawHighlightSelected(geneRect);
                    }
                    if (Widgets.ButtonInvisible(geneRect))
                    {
                        selectedGene = genes[i];
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                }
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(outerRect, noGenesLabel);
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }
    }
}
