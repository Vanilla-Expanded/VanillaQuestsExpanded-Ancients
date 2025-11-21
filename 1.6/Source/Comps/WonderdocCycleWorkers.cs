using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class WonderdocCycle_Bioregeneration : WonderdocCycle
    {
        public override void ApplyOnSuccess(Pawn pawn, Thing building)
        {
            var fertilityHediffs = DefDatabase<HediffDef>.AllDefs.Where(def => def.preventsPregnancy == true).ToList();
            var chronicHediffs = DefDatabase<HediffDef>.AllDefs.Where(def => def.chronic == true).ToList();
            var diseases = DefDatabase<HediffDef>.AllDefs.Where(def => def.HasModExtension<DiseaseMarker>()).ToList();

            foreach (var hediff in pawn.health.hediffSet.hediffs.Where(h => h is Hediff_Injury || h.IsPermanent() || h.def == HediffDefOf.ToxicBuildup || h.def == HediffDefOf.Carcinoma || h.def == HediffDefOf.Blindness || h.def == HediffDefOf.Dementia || h.def == HediffDefOf.ResurrectionPsychosis || fertilityHediffs.Contains(h.def) || chronicHediffs.Contains(h.def) || diseases.Contains(h.def)).ToList())
            {
                if (hediff is not Hediff_Addiction)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
            foreach (var hediff in pawn.health.hediffSet.hediffs.Where(h => h.def == HediffDefOf.MissingBodyPart).ToList())
            {
                pawn.health.RestorePart(hediff.Part);
            }
            Messages.Message("VQEA_WonderdocBioregenerationSuccess".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void ApplyOnMalfunction(Pawn pawn, Thing building)
        {
            var part = pawn.health.hediffSet.GetNotMissingParts().Where(p => p.def == BodyPartDefOf.Arm || p.def == BodyPartDefOf.Leg).RandomElement();
            if (part != null)
            {
                pawn.health.AddHediff(HediffDefOf.MissingBodyPart, part);
            }
            for (int i = 0; i < 4; i++)
            {
                var randomPart = pawn.health.hediffSet.GetNotMissingParts().RandomElement();
                if (randomPart != null)
                {
                    var damageInfo = new DamageInfo(DamageDefOf.SurgicalCut, 6f, 0f, -1f, null, randomPart);
                    pawn.TakeDamage(damageInfo);
                }
            }
            var hediff = new List<HediffDef> { InternalDefOf.Frail, InternalDefOf.Cataract, InternalDefOf.BadBack, InternalDefOf.Alzheimers, HediffDefOf.Carcinoma, InternalDefOf.Malaria, InternalDefOf.SleepingSickness, InternalDefOf.Flu, InternalDefOf.Plague, InternalDefOf.GutWorms, InternalDefOf.MuscleParasites }.RandomElement();
            if (hediff != null)
            {
                pawn.health.AddHediff(hediff);
            }
            Messages.Message("VQEA_WonderdocMalfunction".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
        }
    }

    public class WonderdocCycle_Instarehab : WonderdocCycle
    {
        public override void ApplyOnSuccess(Pawn pawn, Thing building)
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs.Where(h => h.def.IsAddiction).ToList())
            {
                pawn.health.RemoveHediff(hediff);
            }
            foreach (var gene in pawn.genes.GenesListForReading.Where(g => g.def.chemical != null).ToList())
            {
                pawn.genes.RemoveGene(gene);
            }
            Messages.Message("VQEA_WonderdocInstarehabSuccess".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void ApplyOnMalfunction(Pawn pawn, Thing building)
        {
            var drugDependencyGene = DefDatabase<GeneDef>.AllDefs.Where(g => g.chemical != null).RandomElement();
            pawn.genes.AddGene(drugDependencyGene, true);
            Messages.Message("VQEA_WonderdocMalfunction".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
        }
    }

    public class WonderdocCycle_TimeReversal : WonderdocCycle
    {
        public override void ApplyOnSuccess(Pawn pawn, Thing building)
        {
            long newAge = pawn.ageTracker.AgeBiologicalTicks - (GenDate.TicksPerYear * 10);
            if (newAge < GenDate.TicksPerYear * 18)
            {
                newAge = GenDate.TicksPerYear * 18;
            }
            pawn.ageTracker.AgeBiologicalTicks = newAge;
            if (pawn.Ideo != null && pawn.Ideo.HasPrecept(PreceptDefOf.AgeReversal_Demanded))
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AgeReversalReceived);
            }
            Messages.Message("VQEA_WonderdocTimeReversalSuccess".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void ApplyOnMalfunction(Pawn pawn, Thing building)
        {
            if (Rand.Bool)
            {
                pawn.ageTracker.AgeBiologicalTicks = GenDate.TicksPerDay;
                foreach (var skill in pawn.skills.skills)
                {
                    skill.xpSinceLastLevel = 0;
                    skill.Level = 0;
                }
                if (pawn.apparel != null)
                {
                    pawn.apparel.DropAll(pawn.Position, true);
                }
                pawn.Strip();
            }
            else
            {
                pawn.ageTracker.AgeBiologicalTicks = GenDate.TicksPerYear * 100;
                pawn.health.AddHediff(InternalDefOf.BadBack);
                pawn.health.AddHediff(InternalDefOf.Frail);
            }
            Messages.Message("VQEA_WonderdocMalfunction".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
        }
    }

    public class WonderdocCycle_SkillReformatting : WonderdocCycle
    {
        public override void ApplyOnSuccess(Pawn pawn, Thing building)
        {
            float totalXp = pawn.skills.skills.Sum(s => s.XpTotalEarned);
            var capableSkills = pawn.skills.skills.Where(s => !s.TotallyDisabled).ToList();
            float xpPerSkill = totalXp / capableSkills.Count;
            foreach (var skill in pawn.skills.skills)
            {
                skill.xpSinceLastLevel = 0;
                skill.Level = 0;
            }
            foreach (var skill in capableSkills)
            {
                skill.Learn(xpPerSkill, true);
            }
            Messages.Message("VQEA_WonderdocSkillReformattingSuccess".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override void ApplyOnMalfunction(Pawn pawn, Thing building)
        {
            float totalXp = pawn.skills.skills.Sum(s => s.XpTotalEarned);
            foreach (var skill in pawn.skills.skills)
            {
                skill.xpSinceLastLevel = 0;
                skill.Level = 0;
            }

            var capableSkills = pawn.skills.skills.Where(s => !s.TotallyDisabled).ToList();
            if (capableSkills.Any())
            {
                var skillWeights = new Dictionary<SkillDef, float>();
                float totalWeight = 0f;

                foreach (var skill in capableSkills)
                {
                    float weight = Rand.Range(0.01f, 1f);
                    skillWeights[skill.def] = weight;
                    totalWeight += weight;
                }
                foreach (var skill in capableSkills)
                {
                    float skillShare = (skillWeights[skill.def] / totalWeight) * totalXp;
                    skill.Learn(skillShare, true);
                }
            }
            Messages.Message("VQEA_WonderdocMalfunction".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
        }
    }
}
