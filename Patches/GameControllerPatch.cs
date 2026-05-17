using HarmonyLib;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.Chapters;
using SpeedrunMod.Practice.Minigames;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(GameController))]
public class GameControllerPatch
{
    [HarmonyPatch(nameof(GameController.ExitGame))]
    [HarmonyPrefix]
    private static void ExitGamePatch()
    {
        PracticeManager.CurrentChapter = ChapterResolver.None;
        PracticeManager.CurrentMinigame = MinigameResolver.None;
        ChapterSelector.Reset();
    }
}