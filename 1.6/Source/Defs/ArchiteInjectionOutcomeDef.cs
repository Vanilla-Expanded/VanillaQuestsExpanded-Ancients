using Verse;
using RimWorld;

namespace VanillaQuestsExpandedAncients
{
    public enum OutcomeType { Success, Rejection, Mutation }

    public class ArchiteInjectionOutcomeDef : Def
    {
        public int baseWeight;
        public PawnKindDef pawnKind;
        public OutcomeType outcomeType;
    }
}