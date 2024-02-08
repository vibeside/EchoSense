using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CustomPostProcessingAPI;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
namespace EchoProject
{
    [BepInPlugin(modGuid,modName,"1.0.0.0")]
    [BepInDependency("Chaos.CustomPostProcessingAPI")]
    public class EchoPlugin : BaseUnityPlugin
    {
        public const string modGuid = "grug.lethalcompany.EchoSense";
        public const string modName = "EchoSense";
        public static ManualLogSource mls;
        public static AssetBundle assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.sonarbundle);
        public static Material assetMat = assetBundle.LoadAsset<Material>("Assets/fulscreen stuff/FS_Material.mat");
        public static FullScreenCustomPass? sonarPass;
        public static ConfigEntry<float>? sonarSpeed;
        public static ConfigEntry<string>? sonarColor;
        public static ConfigEntry<float>? sonarRange;
        public static ConfigEntry<bool>? nightmareMode;
        public static Hook? hudManagerHook;
        public static Hook? startOfRoundHook;
        public static Hook? hudManagerUpdateHook;
        public void Awake()
        {
            mls = Logger;
            assetMat.SetFloat("_Distance", 100f);
            sonarSpeed = Config.Bind("Sonar settings", "Sonar speed", 15f, "How fast the sonar expands");
            sonarColor = Config.Bind("Sonar settings", "Sonar Color", "0,9,255", "255,0,0 for red, 0,255,0 for green, 0,0,255 for blue");
            sonarRange = Config.Bind("Sonar settings", "Sonar Range", 50f, "How far the sonar goes");
            nightmareMode = Config.Bind("Sonar settings", "Nightmare mode", false, "Only for the brave hearted");

            string[] sonarColorSplit = sonarColor.Value.Split(',');
            int[] colorInts = {0,0,0};
            int.TryParse(sonarColorSplit[0], out colorInts[0]);
            int.TryParse(sonarColorSplit[1], out colorInts[1]);
            int.TryParse(sonarColorSplit[2], out colorInts[2]);
            if (nightmareMode.Value) {
                assetMat.SetInt("_Fade", 1);
                assetMat.SetFloat("_FadeOffset",-1f);
            }
            assetMat.SetFloat("_FadeAfter",sonarRange.Value);
            assetMat.SetColor("_BorderColor", new Color(colorInts[0]/255f, colorInts[1]/255f, colorInts[2]/255f));
            
            hudManagerHook = new(
            typeof(HUDManager).GetMethod(nameof(HUDManager.PingScan_performed), (BindingFlags)int.MaxValue),
            (Action<HUDManager, InputAction.CallbackContext> original, HUDManager self, InputAction.CallbackContext context) =>
            {
                assetMat.SetFloat("_Distance", 0);
                assetMat.SetVector("_Position",StartOfRound.Instance.localPlayerController.transform.position);
                original(self, context);
            }); 


            hudManagerUpdateHook = new(
            typeof(HUDManager).GetMethod(nameof(HUDManager.Update), (BindingFlags)int.MaxValue),
            (Action<HUDManager> original, HUDManager self) =>
            {
                if (!StartOfRound.Instance.inShipPhase)
                {
                    assetMat.SetFloat("_Distance", assetMat.GetFloat("_Distance") + Time.deltaTime * sonarSpeed.Value);
                }
                if (sonarPass != null)
                {
                    sonarPass.enabled = !StartOfRound.Instance.inShipPhase;
                }
                original(self);
            });
        }
        private void Start()
        {
            if (CustomPostProcessingManager.Initialized)
            {
                DoStuff();
            }
            else
            {
                CustomPostProcessingManager.OnLoad += CustomPostProcessingManager_OnLoad;
            }
        }
        private void CustomPostProcessingManager_OnLoad(object sender, EventArgs e)
        {
            DoStuff();
        }

        private void DoStuff()
        {
            PostProcess sonar = new PostProcess("Sonar", assetMat)
            {
                InjectionType = InjectionType.AfterPostProcess
            };
            sonarPass = CustomPostProcessingManager.Instance.AddPostProcess(sonar);
        }
    }
}
