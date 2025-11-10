using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class Hediff_HemocollapseSyndrome : HediffWithComps
    {
        public override float BleedRate
        {
            get
            {
                if (Severity >= 0.90f)
                {
                    return 0.06f;
                }
                return 0f;
            }
        }
    }
}