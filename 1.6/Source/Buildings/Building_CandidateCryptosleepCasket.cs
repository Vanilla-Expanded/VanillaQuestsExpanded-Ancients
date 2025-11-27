using RimWorld;
using Verse;
using Verse.Grammar;
using Verse.Noise;

namespace VanillaQuestsExpandedAncients
{
    public class Building_CandidateCryptosleepCasket : Building_ContainmentCasket
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && ContainedThing is Pawn specialPawn)
            {
                GrammarRequest firstNameReq = default;
                if (specialPawn.gender == Gender.Male)
                {
                    firstNameReq.Includes.Add(InternalDefOf.VQE_ExperimentMaleNames);
                }
                else
                {
                    firstNameReq.Includes.Add(InternalDefOf.VQE_ExperimentFemaleNames);
                }
                var firstName = GrammarResolver.Resolve("r_first_name", firstNameReq);
                GrammarRequest lastNameReq = default;
                lastNameReq.Includes.Add(InternalDefOf.VQE_ExperimentLastNames);
                var lastName = GrammarResolver.Resolve("r_last_name", lastNameReq);
                var name = new NameTriple(firstName, "", lastName);
                specialPawn.Name = name;

                Hediff hediff = HediffMaker.MakeHediff(InternalDefOf.VQEA_HemocollapseSyndrome, specialPawn);
                hediff.Severity = 0.12f;
                specialPawn.health.AddHediff(hediff);
            }
        }

        public override void EjectContents()
        {
            Pawn pawn = (Pawn)this.ContainedThing;
            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
                Find.LetterStack.ReceiveLetter(
                    "VQEA_ExperimentJoinsLetterLabel".Translate(pawn.Named("PAWN")),
                    "VQEA_ExperimentJoinsLetterDescription".Translate(pawn.Named("PAWN")),
                    LetterDefOf.PositiveEvent,
                    pawn
                );
            }
            base.EjectContents();
        }
    }
}
