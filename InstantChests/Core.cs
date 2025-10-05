using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.UI.InGame.Rewards;
using MelonLoader;
using System.Collections;

[assembly: MelonInfo(typeof(InstantChests.Core), "InstantChests", "1.0.0", "Wooshibou", null)]
[assembly: MelonGame("Ved", "Megabonk")]

namespace InstantChests
{
    public class Core : MelonMod
    {
        private static ChestWindowUi currentChestWindowUiInstance;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }


        // Auto open chest and store instance
        [HarmonyPatch(typeof(ChestWindowUi), "Open", new Type[] { typeof(EEncounter) })]
        private static class Patch_ClickOnOpen
        {
            private static void Postfix(ChestWindowUi __instance)
            {
                // Trigger the "Open" button automatically
                __instance.OpenButton();

                // Store the instance for later use
                currentChestWindowUiInstance = __instance;
            }

           
        }

        // Prevent the Open button from ever showing
        [HarmonyPatch(typeof(ChestWindowUi), "ShowOpenButton")]
        private static class Patch_ShowOpenButton
        {
            private static bool Prefix() => false;
        }


        // Skip chest animations and instantly open 
        [HarmonyPatch(typeof(ChestOpening), "OpenChest", new Type[] { typeof(Il2Cpp.ItemData) })]
        private static class Patch_OpenChest
        {
            private static void Postfix(ChestOpening __instance, Il2Cpp.ItemData itemData)
            {

                // Skip the animation of the chest, and make the delay between tiers super fast
                __instance.skipped = true; // Equivalent to skipping manually
                __instance.spinning = false; // Don't know if it's useful
                __instance.timeBetweenTiers = 0.01f;

                MelonCoroutines.Start(DelayedOpening(__instance));

            }

            private static IEnumerator DelayedOpening(ChestOpening __instance)
            {
                // Waits for ChestWindowUi.Open() to initialize
                yield return null;

                if (currentChestWindowUiInstance == null)
                {
                    Melon<Core>.Logger.Warning("ChestWindowUi is null — skipping first chest.");
                    yield break;
                }

                if (__instance.itemData == null)
                {
                    Melon<Core>.Logger.Warning("ItemData is null — skipping OpeningFinished.");
                    yield break;
                }

                // Adds the item instance to the UI.
                currentChestWindowUiInstance.itemData = __instance.itemData;
                currentChestWindowUiInstance.OpeningFinished(__instance.itemData);
            }


        }
    }
}