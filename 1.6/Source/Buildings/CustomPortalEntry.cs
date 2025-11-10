using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    public class CustomPortalEntry : MapPortal
    {
        private Map destinationMap;
        private MapPortal destinationExit;
        private CompCustomPortal CustomPortalComp => GetComp<CompCustomPortal>();
        public override string EnterString => CustomPortalComp.Props.enterCommandKey.Translate();
        protected override Texture2D EnterTex => ContentFinder<Texture2D>.Get(CustomPortalComp.Props.enterTexPath);
        public override bool AutoDraftOnEnter => true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref destinationMap, "destinationMap");
            Scribe_References.Look(ref destinationExit, "destinationExit");
        }

        public override Map GetOtherMap()
        {
            if (destinationMap == null)
            {
                GenerateDestinationMap();
            }
            return destinationMap;
        }

        public override IntVec3 GetDestinationLocation()
        {
            return destinationExit?.Position ?? IntVec3.Invalid;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (destinationMap != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = CustomPortalComp.Props.viewDestinationCommandKey.Translate(),
                    defaultDesc = CustomPortalComp.Props.viewDestinationDescKey.Translate(),
                    icon = ContentFinder<Texture2D>.Get(CustomPortalComp.Props.viewDestinationTexPath),
                    action = delegate
                    {
                        CameraJumper.TryJumpAndSelect(destinationExit);
                    }
                };
            }
        }

        private void GenerateDestinationMap()
        {
            PocketMapUtility.currentlyGeneratingPortal = this;
            destinationMap = PocketMapUtility.GeneratePocketMap(new IntVec3(def.portal.pocketMapSize, 1, def.portal.pocketMapSize), def.portal.pocketMapGenerator, null, base.Map);
            destinationExit = destinationMap.listerThings.ThingsOfDef(def.portal.exitDef).First() as MapPortal;
            if (destinationExit != null)
            {
                ((CustomPortalExit)destinationExit).portalEntry = this;
            }
            PocketMapUtility.currentlyGeneratingPortal = null;
        }
    }
}