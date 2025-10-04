
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VanillaQuestsExpandedAncients
{

    public class DeathActionWorker_VanishToots : DeathActionWorker
    {
        public DeathActionProperties_VanishToots Props => (DeathActionProperties_VanishToots)props;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            if (Props.fleck != null)
            {
                FleckMaker.Static(corpse.PositionHeld, corpse.MapHeld, Props.fleck);
            }
            if (Props.filth != null)
            {
                int randomInRange = Props.filthCountRange.RandomInRange;
                for (int i = 0; i < randomInRange; i++)
                {
                    FilthMaker.TryMakeFilth(corpse.PositionHeld, corpse.MapHeld, Props.filth);
                }
            }
            IntVec3 randomCell = new CellRect(corpse.PositionHeld.x, corpse.PositionHeld.z, 3, 3).ClipInsideMap(corpse.MapHeld).RandomCell;
            ThingDef filth_Blood = ThingDefOf.Filth_Blood;
            if (randomCell.InBounds(corpse.MapHeld) && GenSight.LineOfSight(randomCell, corpse.PositionHeld, corpse.MapHeld))
            {
                FilthMaker.TryMakeFilth(randomCell, corpse.MapHeld, filth_Blood);
            }

            GasUtility.AddGas(corpse.PositionHeld, corpse.MapHeld, GasType.RotStink, Props.gasAmount);


            corpse.Destroy();
        }
    }
}