using System;
using System.IO;
using System.Reflection;
using BepInEx;
using CustomPostProcessingAPI;
using UnityEngine;
namespace EchoProject
{
    [BepInPlugin(modGuid,modName,"1.0.0.0")]
    [Bep]
    public class EchoPlugin
    {
        public const string modGuid = "grug.lethalcompany.EchoSense";
        public const string modName = "EchoSense";
        public static AssetBundle assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.sonarbundle);
        public static Material assetMat = assetBundle.LoadAsset<Material>("Assets/fulscreen stuff/FS_Material.mat");
        public void Awake()
        {
            PostProcess sonar = new PostProcess("Sonar", assetMat);
            CustomPostProcessingManager.Instance.AddPostProcess(sonar);
        }
    }
}
