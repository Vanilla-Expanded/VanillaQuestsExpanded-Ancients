using System;
using UnityEngine;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {

        public static readonly Graphic graphicTop = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>("Things/Building/Power/AncientBiobattery_Top", ShaderDatabase.Transparent, Vector2.one *4.5f, Color.white);    

    }
}
