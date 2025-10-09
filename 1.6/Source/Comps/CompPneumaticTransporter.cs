using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class CompProperties_PneumaticTransporter : CompProperties_Transporter
    {
        public CompProperties_PneumaticTransporter()
        {
            compClass = typeof(CompPneumaticTransporter);
        }
    }

    public class CompPneumaticTransporter : CompTransporter
    {
        private bool notifiedCantLoadMorePneumatic;

        public bool NotifiedCantLoadMore => notifiedCantLoadMorePneumatic;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref notifiedCantLoadMorePneumatic, "notifiedCantLoadMorePneumatic", defaultValue: false);
        }

        public override void CompTick()
        {
            if (Props.shouldTickContents)
            {
                innerContainer.DoTick();
            }
            else if (Props.restEffectiveness != 0f)
            {
                for (int i = 0; i < innerContainer.Count; i++)
                {
                    if (innerContainer[i] is Pawn { Dead: false } pawn && pawn.needs.rest != null)
                    {
                        pawn.needs.rest.TickResting(Props.restEffectiveness);
                    }
                }
            }

            if (!parent.IsHashIntervalTick(60) || !parent.Spawned)
            {
                return;
            }

            if (LoadingInProgressOrReadyToLaunch && AnyInGroupHasAnythingLeftToLoad && !notifiedCantLoadMorePneumatic && !AnyPawnCanLoadAnythingNow)
            {
                notifiedCantLoadMorePneumatic = true;
                Messages.Message("VQEA_MessageCantLoadMoreIntoPneumaticTube".Translate(FirstThingLeftToLoadInGroup.LabelNoCount, Faction.OfPlayer.def.pawnsPlural, FirstThingLeftToLoadInGroup), parent, MessageTypeDefOf.CautionInput);
            }
        }
        
        public void ResetNotifiedFlag()
        {
            notifiedCantLoadMorePneumatic = false;
        }
    }
}