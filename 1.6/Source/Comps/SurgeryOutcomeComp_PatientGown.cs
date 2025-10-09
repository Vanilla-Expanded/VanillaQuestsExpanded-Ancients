using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class SurgeryOutcomeComp_PatientGown : SurgeryOutcomeComp
    {
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            if (patient?.apparel?.WornApparel != null)
            {
                foreach (Apparel apparel in patient.apparel.WornApparel)
                {
                    if (apparel.def == InternalDefOf.VQEA_Apparel_PatientGown)
                    {
                        quality *= 1.1f;
                        break;
                    }
                }
            }
        }
    }
}