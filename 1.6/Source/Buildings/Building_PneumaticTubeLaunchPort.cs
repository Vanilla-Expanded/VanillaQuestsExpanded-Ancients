using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaQuestsExpandedAncients
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public class Building_PneumaticTubeLaunchPort : Building
    {
        private CompPneumaticTransporter transporter;

        private float totalMarketValue;

        private int ticksUntilDelivery;

        private int wastepacksToReturnCount;

        public static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Gizmo/LaunchPneumaticTube");
        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter");

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            transporter = GetComp<CompPneumaticTransporter>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksUntilDelivery, "ticksUntilDelivery", 0);
            Scribe_Values.Look(ref totalMarketValue, "totalMarketValue", 0f);
            Scribe_Values.Look(ref wastepacksToReturnCount, "wastepacksToReturnCount", 0);
        }

        protected override void Tick()
        {
            base.Tick();
            if (Spawned && ticksUntilDelivery > 0)
            {
                ticksUntilDelivery--;
                if (ticksUntilDelivery == 0)
                {
                    Deliver();
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_LoadToTransporter)
                {
                    continue;
                }
                yield return gizmo;
            }

            var command = new Command_Action
            {
                defaultLabel = "VQEA_LoadCargo".Translate(),
                defaultDesc = "VQEA_LoadCargoDesc".Translate(),
                icon = LoadCommandTex,
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_LoadPneumaticTube(this, transporter, Map));
                }
            };
            yield return command;

            if (transporter.innerContainer.Any && (!transporter.AnythingLeftToLoad || transporter.NotifiedCantLoadMore))
            {
                Command_Action launch = new Command_Action();
                launch.defaultLabel = "VQEA_LaunchCapsule".Translate();
                launch.defaultDesc = "VQEA_LaunchCapsuleDesc".Translate();
                launch.icon = LaunchCommandTex;
                launch.action = delegate
                {
                    Launch();
                };
                yield return launch;
            }
            if (DebugSettings.ShowDevGizmos && ticksUntilDelivery > 0)
            {
                Command_Action devDeliver = new Command_Action();
                devDeliver.defaultLabel = "DEV: Trigger delivery";
                devDeliver.action = delegate
                {
                    Deliver();
                    ticksUntilDelivery = 0;
                };
                yield return devDeliver;
            }
        }

        public void Launch()
        {
            Map map = Map;
            InternalDefOf.VQEA_PneumaticLaunch.PlayOneShot(new TargetInfo(Position, map));

            totalMarketValue = transporter.innerContainer.Where(x => x.def != ThingDefOf.Wastepack).Sum(x => x.MarketValue * x.stackCount);
            var sentWastepacks = transporter.innerContainer.Where(x => x.def == ThingDefOf.Wastepack).ToList();
            if (sentWastepacks.Any())
            {
                wastepacksToReturnCount = sentWastepacks.Sum(t => t.stackCount);
            }
            else
            {
                wastepacksToReturnCount = 0;
            }

            transporter.innerContainer.ClearAndDestroyContents();
            ticksUntilDelivery = Rand.Range(2500, 60000);

            transporter.TryRemoveLord(map);
            if (transporter.leftToLoad != null)
            {
                transporter.leftToLoad.Clear();
            }
        }

        private void Deliver()
        {
            InternalDefOf.VQEA_PneumaticArrival.PlayOneShot(new TargetInfo(Position, Map));
            Messages.Message("VQEA_PneumaticCapsuleArrived".Translate(), this, MessageTypeDefOf.PositiveEvent);

            List<Thing> things = new List<Thing>();
            if (wastepacksToReturnCount > 0)
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.Wastepack);
                thing.stackCount = wastepacksToReturnCount;
                things.Add(thing);
                wastepacksToReturnCount = 0;
            }
            if (totalMarketValue > 0)
            {
                float currentValue = 0;
                var possibleThings = DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Item && x.BaseMarketValue > 0.01f && !x.IsCorpse).ToList();
                while ((totalMarketValue - currentValue) > 0f)
                {
                    float remainingValue = totalMarketValue - currentValue;
                    if (!possibleThings.Where(x => Mathf.CeilToInt(remainingValue / x.BaseMarketValue) < 1000).TryRandomElement(out var thingDef))
                    {
                        break;
                    }
                    Thing thing;
                    if (thingDef.MadeFromStuff)
                    {
                        var stuff = GenStuff.RandomStuffFor(thingDef);
                        thing = ThingMaker.MakeThing(thingDef, stuff);
                    }
                    else
                    {
                        thing = ThingMaker.MakeThing(thingDef);
                    }
                    int amount = Mathf.Min(thingDef.stackLimit, Mathf.CeilToInt(remainingValue / thing.MarketValue));
                    if (amount <= 0)
                    {
                        amount = 1;
                    }
                    thing.stackCount = amount;
                    things.Add(thing);
                    currentValue += thing.MarketValue * thing.stackCount;
                }
            }
            foreach (var thing in things)
            {
                GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
            }
            
            totalMarketValue = 0;
        }
    }
}
