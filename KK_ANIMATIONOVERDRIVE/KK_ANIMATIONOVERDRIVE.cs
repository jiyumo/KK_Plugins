﻿using BepInEx;
using BepInEx.Harmony;
using HarmonyLib;
using Studio;
using System.Linq;
using UnityEngine.UI;

namespace KK_Plugins
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class KK_AnimationOverdrive : BaseUnityPlugin
    {
        public const string GUID = "com.deathweasel.bepinex.animationoverdrive";
        public const string PluginName = "Animation Overdrive";
        public const string Version = "1.0";

        internal void Main() => HarmonyWrapper.PatchAll(typeof(KK_AnimationOverdrive));

        /// <summary>
        /// Copy/paste decompiled code in to the prefix, return false. 10/10 programing skills
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(AnimeControl), "OnEndEditSpeed")]
        public static bool OnEndEditSpeed(string _text, AnimeControl __instance)
        {
            float speed = StringToFloat(_text);

            InputField inputSpeed = Traverse.Create(__instance).Field("inputSpeed").GetValue<InputField>();
            bool isUpdateInfo = Traverse.Create(__instance).Field("isUpdateInfo").GetValue<bool>();
            Slider sliderSpeed = Traverse.Create(__instance).Field("sliderSpeed").GetValue<Slider>();
            ObjectCtrlInfo[] arrayTarget = Traverse.Create(__instance).Field("arrayTarget").GetValue<ObjectCtrlInfo[]>();
            int num = Traverse.Create(__instance).Field("num").GetValue<int>();
            float[] oldValue = Traverse.Create(__instance).Field("oldValue").GetValue<float[]>();

            arrayTarget = (from v in Singleton<Studio.Studio>.Instance.treeNodeCtrl.selectObjectCtrl
                           where v.kind == 0 || v.kind == 1
                           select v).ToArray();
            num = arrayTarget.Length;
            oldValue = (from v in arrayTarget
                        select v.animeSpeed).ToArray();

            Singleton<UndoRedoManager>.Instance.Do(new AnimeCommand.SpeedCommand((from v in arrayTarget
                                                                                  select v.objectInfo.dicKey).ToArray(), speed, oldValue));

            isUpdateInfo = true;
            inputSpeed.text = _text;
            if (speed > 3)
                sliderSpeed.maxValue = speed;
            else
                sliderSpeed.maxValue = 3;
            sliderSpeed.value = speed;
            isUpdateInfo = false;
            arrayTarget = null;
            num = 0;

            return false;
        }

        public static float StringToFloat(string _text) => float.TryParse(_text, out float num) ? num : 0f;
    }
}
