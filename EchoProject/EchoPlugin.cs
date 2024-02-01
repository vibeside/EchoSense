using System;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;
namespace EchoProject
{
    [BepInPlugin(modGuid,modName,"1.0.0.0")]
    public class EchoPlugin
    {
        public const string modGuid = "grug.lethalcompany.EchoSense";
        public const string modName = "EchoSense";
        public static AssetBundle bundle;
        public void Awake()
        {
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            bundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "sonarBundle"));
        }
    }
}
