using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public abstract class WonderdocCycle
    {
        public string label;
        public string description;
        public int durationTicks;
        public Color glowColor;
        public string iconPath;

        [Unsaved(false)]
        private Texture2D iconTex;

        public Texture2D Icon
        {
            get
            {
                if (iconTex == null)
                {
                    if (!iconPath.NullOrEmpty())
                    {
                        iconTex = ContentFinder<Texture2D>.Get(iconPath);
                    }
                }
                return iconTex;
            }
        }

        public abstract void ApplyOnSuccess(Pawn pawn, Thing building);
        public abstract void ApplyOnMalfunction(Pawn pawn, Thing building);
    }
}