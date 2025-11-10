using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class QuestPart_AncientLab : QuestPart_Site
    {
        public string inSignalSuccess;
        public string inSignalFail;
        public ThingDef questBuilding;

        public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
        {
            get
            {
                if (questBuilding != null)
                {
                    yield return new Dialog_InfoCard.Hyperlink(questBuilding);
                }
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == inSignalSuccess &&
                signal.args.TryGetArg("SUBJECT", out Site subject) &&
                subject == mapParent)
            {
                quest.End(QuestEndOutcome.Success, sendLetter: true);
            }

            if (signal.tag == inSignalFail &&
                signal.args.TryGetArg("SUBJECT", out Site subject2) &&
                subject2 == mapParent)
            {
                quest.End(QuestEndOutcome.Fail, sendLetter: true);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignalSuccess, "inSignalSuccess");
            Scribe_Values.Look(ref inSignalFail, "inSignalFail");
            Scribe_Defs.Look(ref questBuilding, "questBuilding");
        }
    }
}
