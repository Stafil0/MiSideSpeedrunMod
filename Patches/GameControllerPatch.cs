using HarmonyLib;
using SpeedrunMod.Practice;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(GameController))]
public class GameControllerPatch
{
    [HarmonyPatch(nameof(GameController.ExitGame))]
    [HarmonyPrefix]
    private static void ExitGamePatch()
    {
        PracticeManager.CurrentChapter = GameChapter.None;
        PracticeManager.CurrentMinigame = ChapterMinigame.None;
        ChapterSelector.Reset();
    }
}