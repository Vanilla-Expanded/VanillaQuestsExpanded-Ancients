using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using static HarmonyLib.Code;
using Verse.Noise;
using Verse.AI;




namespace VanillaQuestsExpandedAncients
{
    public class CompKickablePower : CompPowerPlant
    {

        public bool active = true;
        public int extraJuice;
        public int countDown;


        public new CompProperties_KickablePower Props => (CompProperties_KickablePower)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref active, "active", true);
            Scribe_Values.Look(ref countDown, "countDown", 0);
            Scribe_Values.Look(ref extraJuice, "extraJuice", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            RandomizeCountDownAndJuice();
        }

        public void RandomizeCountDownAndJuice()
        {
            countDown = Props.timeInterval.RandomInRange;
            extraJuice = Props.extraJuice.RandomInRange;
        }

        public void FlickOn()
        {
            this.flickableComp.SwitchIsOn = true;
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                if (!active)
                {
                    return 0;
                }
                return base.DesiredPowerOutput + extraJuice;
            }

        }

        public override void CompTick()
        {
            base.CompTick();
            if (active)
            {
                countDown--;
                if (countDown <= 0)
                {
                    active = false;
                    this.flickableComp.SwitchIsOn = false;
                }
            }
        }
       

        public override void PostDraw()
        {
            base.PostDraw();

            if (!active)
            {
                Vector3 drawPos = parent.DrawPos;
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.181818187f;
                float num = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(parent.thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
                num = 0.3f + num * 0.7f;
                Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom("UI/Overlays/NeedsKicking", ShaderDatabase.MetaOverlay), num);
                Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
            }
        }

       

    }
}
