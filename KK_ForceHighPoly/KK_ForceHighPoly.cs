﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using HarmonyLib;
using System;
using System.Collections;

namespace KK_Plugins
{
    /// <summary>
    /// Replaces all _low assets with normal assets, forcing everything to load as high poly
    /// </summary>
    [BepInPlugin(GUID, PluginName, Version)]
    public class KK_ForceHighPoly : BaseUnityPlugin
    {
        public const string GUID = "com.deathweasel.bepinex.forcehighpoly";
        public const string PluginName = "Force High Poly";
        public const string PluginNameInternal = "KK_ForceHighPoly";
        public const string Version = "1.2";

        public static ConfigEntry<bool> Enabled { get; private set; }

        internal void Main()
        {
            Enabled = Config.AddSetting("Config", "High poly mode", true, "Whether or not to load high poly assets. May require exiting to main menu to take effect.");

            HarmonyWrapper.PatchAll(typeof(KK_ForceHighPoly));
        }

        [HarmonyPrefix]
        [HarmonyBefore(new string[] { "com.bepis.bepinex.resourceredirector" })]
        [HarmonyPatch(typeof(AssetBundleManager), nameof(AssetBundleManager.LoadAsset), new[] { typeof(string), typeof(string), typeof(Type), typeof(string) })]
        public static void LoadAssetPrefix(ref string assetName)
        {
            if (Enabled.Value && assetName.EndsWith("_low"))
                assetName = assetName.Substring(0, assetName.Length - 4);
        }

        [HarmonyPrefix]
        [HarmonyBefore(new string[] { "com.bepis.bepinex.resourceredirector" })]
        [HarmonyPatch(typeof(AssetBundleManager), nameof(AssetBundleManager.LoadAssetAsync), new[] { typeof(string), typeof(string), typeof(Type), typeof(string) })]
        public static void LoadAssetAsyncPrefix(ref string assetName)
        {
            if (Enabled.Value && assetName.EndsWith("_low"))
                assetName = assetName.Substring(0, assetName.Length - 4);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "ChangeHairAsync", new Type[] { typeof(int), typeof(int), typeof(bool), typeof(bool) })]
        public static void ChangeHairAsyncPostHook(ChaControl __instance, int kind, ref IEnumerator __result)
        {
            if (!Enabled.Value) return;

            var orig = __result;
            __result = new IEnumerator[] { orig, ChangeHairAsyncPostfix(__instance, kind) }.GetEnumerator();
        }

        private static IEnumerator ChangeHairAsyncPostfix(ChaControl instance, int kind)
        {
            var hairObject = instance.objHair[kind];
            if (hairObject != null)
                foreach (var dynamicBone in hairObject.GetComponentsInChildren<DynamicBone>(true))
                    dynamicBone.enabled = true;

            yield break;
        }
    }
}
