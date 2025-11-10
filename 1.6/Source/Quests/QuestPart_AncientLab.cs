using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class QuestPart_AncientLab : QuestPart
    {
        public string inSignalSuccess;
        public string inSignalFail;
        public Site site;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            Log.Message("Received signal: " + signal.tag);
            Log.Message("inSignalSuccess: " + inSignalSuccess + " - inSignalFail: " + inSignalFail);
            if (signal.tag == inSignalSuccess && 
                signal.args.TryGetArg("SUBJECT", out Site subject) && 
                subject == site)
            {
                quest.End(QuestEndOutcome.Success, sendLetter: true);
            }

            if (signal.tag == inSignalFail && 
                signal.args.TryGetArg("SUBJECT", out Site subject2) && 
                subject2 == site)
            {
                quest.End(QuestEndOutcome.Fail, sendLetter: true);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignalSuccess, "inSignalSuccess");
            Scribe_Values.Look(ref inSignalFail, "inSignalFail");
            Scribe_References.Look(ref site, "site");
        }
    }
}
