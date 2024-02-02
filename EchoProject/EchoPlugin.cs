using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using CustomPostProcessingAPI;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
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
        public static Hook? hudManagerHook;
        public static Hook? startOfRoundHook;
        public static Hook? hudManagerUpdateHook;
        public void Awake()
        {
            mls = Logger;
            hudManagerHook = new(
            typeof(HUDManager).GetMethod(nameof(HUDManager.PingScan_performed), (BindingFlags)int.MaxValue),
            (Action<HUDManager, InputAction.CallbackContext> original, HUDManager self, InputAction.CallbackContext context) =>
            {
                assetMat.SetFloat("_Distance", 0);
                original(self, context);
            }); 
            hudManagerUpdateHook = new(
            typeof(HUDManager).GetMethod(nameof(HUDManager.Update), (BindingFlags)int.MaxValue),
            (Action<HUDManager> original, HUDManager self) =>
            {
                assetMat.SetFloat("_Distance", assetMat.GetFloat("_Distance") + Time.deltaTime);
                original(self);
            });
            startOfRoundHook = new(
            typeof(HUDManager).GetMethod(nameof(HUDManager.Start), (BindingFlags)int.MaxValue),
            (Action<HUDManager> original, HUDManager self) =>
            {
                StartCoroutine(WaitUntilPostProcessApi());
                original(self);
            });
        }
        public static IEnumerator WaitUntilPostProcessApi()
        {
            yield return new WaitUntil(() => CustomPostProcessingManager.Instance != null);
            PostProcess sonar = new PostProcess("Sonar", assetMat)
            {
                InjectionType = InjectionType.AfterPostProcess
            };
            sonarPass = CustomPostProcessingManager.Instance.AddPostProcess(sonar);
        }
    }
}
