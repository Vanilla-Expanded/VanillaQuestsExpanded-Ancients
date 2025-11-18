using System;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace VanillaQuestsExpandedAncients
{
    [StaticConstructorOnStartup]
    public static class ModCompatibility
    {
        private static bool? VEAndroidsActive;
        private static MethodBase isAndroidGeneMethod;
        static ModCompatibility()
        {
            CheckModCompatibility();
        }
        
        public static void CheckModCompatibility()
        {
            VEAndroidsActive = ModsConfig.IsActive("vanillaracesexpanded.android");
            if (VEAndroidsActive == true)
            {
                try
                {
                    Type utilsType = GenTypes.GetTypeInAnyAssembly("VREAndroids.Utils");
                    isAndroidGeneMethod = AccessTools.Method(utilsType, "IsAndroidGene");
                }
                catch (Exception ex)
                {
                    Log.Warning($"[VQE Ancients] Failed to get VREA Android compatibility methods: {ex.Message}");
                    VEAndroidsActive = false;
                }
            }
        }
        
        public static bool IsAndroidGene(GeneDef geneDef)
        {
            if (VEAndroidsActive != true || geneDef == null)
            {
                return false;
            }
            try
            {
                return (bool)isAndroidGeneMethod.Invoke(null, new object[] { geneDef });
            }
            catch (Exception ex)
            {
                Log.Error($"[VQE Ancients] Failed to call IsAndroidGene method: {ex.Message}");
                return false;
            }
        }
    }
}
