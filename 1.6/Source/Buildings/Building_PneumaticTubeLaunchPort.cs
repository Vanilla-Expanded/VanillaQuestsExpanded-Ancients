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

        private Dictionary<ThingDef, int> thingsToReturn = new Dictionary<ThingDef, int>();

        public static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Gizmo/LaunchPneumaticTube");
        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter");

        private static List<ThingDef> possibleThings;

        static Building_PneumaticTubeLaunchPort()
        {
            possibleThings = DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Item && x.BaseMarketValue >= 0.01f && !x.IsCorpse && x.destroyOnDrop is false && x.tradeability != Tradeability.None && x.genericMarketSellable && 
            x.defName != "Apparel_CerebrexNode" && x.defName != "VPE_Psyring" && x.thingCategories?.Contains(InternalDefOf.Books)!=true).ToList();
            //Log.Message("All possible things: " + possibleThings.Select(x => x.label).ToStringSafeEnumerable());
        }

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
            Scribe_Collections.Look(ref thingsToReturn, "thingsToReturn", LookMode.Def, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                thingsToReturn ??= new Dictionary<ThingDef, int>();
            }
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

            thingsToReturn ??= new Dictionary<ThingDef, int>();
            totalMarketValue = 0f;
            foreach (var item in transporter.innerContainer)
            {
                if (item.MarketValue == 0f || item.def.HasComp(typeof(CompDissolutionEffect_Goodwill)))
                {
                    if (thingsToReturn.ContainsKey(item.def))
                    {
                        thingsToReturn[item.def] += item.stackCount;
                    }
                    else
                    {
                        thingsToReturn[item.def] = item.stackCount;
                    }
                }
                else
                {
                    totalMarketValue += item.MarketValue * item.stackCount;
                }
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
            if (thingsToReturn != null && thingsToReturn.Any())
            {
                foreach (var keyValuePair in thingsToReturn)
                {
                    try
                    {
                        var thing = ThingMaker.MakeThing(keyValuePair.Key);
                        thing.stackCount = keyValuePair.Value;
                        things.Add(thing);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"[PneumaticTube] Failed to create thing {keyValuePair.Key.defName}: {ex.Message}");
                    }
                }
                thingsToReturn.Clear();
            }
            if (totalMarketValue > 0)
            {
                float currentValue = 0;
                int safetyCounter = 0;
                const int maxIterations = 1000;

                while ((totalMarketValue - currentValue) >= 0.01f && safetyCounter < maxIterations)
                {
                    safetyCounter++;

                    float remainingValue = totalMarketValue - currentValue;
                    var eligibleThings = possibleThings.Where(x =>
                        remainingValue >= x.BaseMarketValue && x.BaseMarketValue >= 0.001f &&
                        Mathf.CeilToInt(remainingValue / x.BaseMarketValue) < 1000
                    ).ToList();


                    if (!eligibleThings.TryRandomElement(out var thingDef))
                    {
                        break;
                    }

                    Thing thing;
                    try
                    {
                        if (thingDef.MadeFromStuff)
                        {
                            var stuff = GenStuff.RandomStuffFor(thingDef);
                            thing = ThingMaker.MakeThing(thingDef, stuff);
                        }
                        else
                        {
                            thing = ThingMaker.MakeThing(thingDef);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"[PneumaticTube] Failed to create thing {thingDef.defName}: {ex.Message}");
                        continue;
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

                if (safetyCounter >= maxIterations)
                {
                    Log.Error($"[PneumaticTube] Hit maximum iteration limit during delivery generation. Remaining value: {totalMarketValue - currentValue}");
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
