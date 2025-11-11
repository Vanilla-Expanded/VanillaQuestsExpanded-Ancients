using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    public class CustomPortalExit : PocketMapExit
    {
        public MapPortal portalEntry;

        private CompCustomPortal CustomPortalComp => GetComp<CompCustomPortal>();

        public override string EnterString => CustomPortalComp.Props.enterCommandKey.Translate();

        protected override Texture2D EnterTex => ContentFinder<Texture2D>.Get(CustomPortalComp.Props.enterTexPath);

        public override Map GetOtherMap()
        {
            return portalEntry.Map;
        }

        public override IntVec3 GetDestinationLocation()
        {
            return portalEntry.Position;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action action && action.Label == "CommandViewSurface".Translate())
                {
                    continue;
                }
                yield return gizmo;
            }
            yield return new Command_Action
            {
                defaultLabel = CustomPortalComp.Props.viewDestinationCommandKey.Translate(),
                defaultDesc = CustomPortalComp.Props.viewDestinationDescKey.Translate(),
                icon = ContentFinder<Texture2D>.Get(CustomPortalComp.Props.viewDestinationTexPath),
                action = delegate
                {
                    CameraJumper.TryJumpAndSelect(new TargetInfo(portalEntry));
                }
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref portalEntry, "portalEntry");
        }
    }
}
