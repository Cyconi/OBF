using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSL
{
    public class Client : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("TestSL initialized successfully!");
            OnTest();
        }
        public override void OnApplicationQuit()
        {
            // This method is called when the application quits
            MelonLogger.Msg("OnApplicationQuit called");
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            // This method is called when a new scene is loaded
            MelonLogger.Msg($"OnSceneWasLoaded called for scene: {sceneName} (Build Index: {buildIndex})");
        }
        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            // This method is called when a scene is unloaded
            MelonLogger.Msg($"OnSceneWasUnloaded called for scene: {sceneName} (Build Index: {buildIndex})");
        }

        public static void OnTest()
        {
            MelonLogger.Msg("Duplicate String");
            MelonLogger.Msg("Duplicate String");
        }
    }
}
