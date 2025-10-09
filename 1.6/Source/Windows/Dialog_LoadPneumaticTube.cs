using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class Dialog_LoadPneumaticTube : Window
    {
        private Building_PneumaticTubeLaunchPort parent;
        private CompPneumaticTransporter transporter;
        private Map map;
        private List<TransferableOneWay> transferables;
        private TransferableOneWayWidget itemsTransfer;
        private float lastMassFlashTime = -9999f;
        private bool massUsageDirty = true;
        private float cachedMassUsage;
        private const float TitleRectHeight = 35f;
        private const float BottomAreaHeight = 55f;
        private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
        public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);
        protected override float Margin => 0f;
        private float MassCapacity => transporter.MassCapacity;

        private float MassUsage
        {
            get
            {
                if (massUsageDirty)
                {
                    massUsageDirty = false;
                    cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMass: true);
                }
                return cachedMassUsage;
            }
        }

        public Dialog_LoadPneumaticTube(Building_PneumaticTubeLaunchPort parent, CompPneumaticTransporter transporter, Map map)
        {
            this.parent = parent;
            this.transporter = transporter;
            this.map = map;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override void PostOpen()
        {
            base.PostOpen();
            CalculateAndRecacheTransferables();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 0f, inRect.width, 35f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "VQEA_LoadCargo".Translate());
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect massRect = new Rect(12f, 25f, inRect.width - 24f, 40f);
            DrawMassUsageIndicator(massRect);

            inRect.yMin += 70f;
            Widgets.DrawMenuSection(inRect);
            inRect = inRect.ContractedBy(17f);
            inRect.height += 17f;
            Widgets.BeginGroup(inRect);
            Rect rect2 = inRect.AtZero();
            DoBottomButtons(rect2);
            Rect inRect2 = rect2;
            inRect2.yMax -= 76f;
            bool anythingChanged = false;
            itemsTransfer.OnGUI(inRect2, out anythingChanged);
            if (anythingChanged)
            {
                CountToTransferChanged();
            }
            Widgets.EndGroup();
        }

        private void DoBottomButtons(Rect rect)
        {
            if (Widgets.ButtonText(new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f - 17f, BottomButtonSize.x, BottomButtonSize.y), "ResetButton".Translate()))
            {
                CalculateAndRecacheTransferables();
            }
            if (Widgets.ButtonText(new Rect(0f, rect.height - 55f - 17f, BottomButtonSize.x, BottomButtonSize.y), "CancelButton".Translate()))
            {
                Close();
            }
            if (Widgets.ButtonText(new Rect(rect.width - BottomButtonSize.x, rect.height - 55f - 17f, BottomButtonSize.x, BottomButtonSize.y), "AcceptButton".Translate()))
            {
                if (TryAccept())
                {
                    Close();
                }
            }
        }

        private void CalculateAndRecacheTransferables()
        {
            transferables = new List<TransferableOneWay>();
            AddItemsToTransferables();
            itemsTransfer = new TransferableOneWayWidget(transferables, null, null, null, drawMass: true,
                IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMassInMassUsage: true, () => MassCapacity - MassUsage, 0f);
            CountToTransferChanged();
        }

        private bool TryAccept()
        {
            if (MassUsage > MassCapacity)
            {
                Messages.Message("TooBigTransportersMassUsage".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            transporter.ResetNotifiedFlag();
            TransporterUtility.InitiateLoading(new List<CompTransporter> { transporter });
            foreach (var transferable in transferables)
            {
                if (transferable.CountToTransfer > 0)
                {
                    transporter.AddToTheToLoadList(transferable, transferable.CountToTransfer);
                }
            }
            return true;
        }

        private void AddItemsToTransferables()
        {
            foreach (Thing item in TransporterUtility.AllSendableItems(new List<CompTransporter> { transporter }, map))
            {
                if (item.IsInValidStorage() is false)
                {
                    continue;
                }
                AddToTransferables(item);
            }
        }

        private void AddToTransferables(Thing t)
        {
            TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                transferables.Add(transferableOneWay);
            }
            transferableOneWay.things.Add(t);
        }

        private void DrawMassUsageIndicator(Rect rect)
        {
            GUI.BeginGroup(rect);

            GUI.color = Color.grey;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0f, 5f, rect.width, 24f), "Mass".Translate());
            GUI.color = Color.white;

            Text.Font = GameFont.Small;
            float massUsage = MassUsage;
            float massCapacity = MassCapacity;
            string massText = massUsage.ToStringEnsureThreshold(massCapacity, 0) + " / " + massCapacity.ToString("F0") + " " + "kg".Translate();

            Color massColor = GetMassColor(massUsage, massCapacity);

            GUI.color = massColor;
            Widgets.Label(new Rect(0f, 20f, rect.width, 24f), massText);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        private Color GetMassColor(float massUsage, float massCapacity)
        {
            if (massUsage > massCapacity)
            {
                return Color.red;
            }
            return Color.white;
        }

        private void CountToTransferChanged()
        {
            massUsageDirty = true;
        }
    }
}
