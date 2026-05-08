using HarmonyLib;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.Minigames;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(Tamagotchi_Main))]
public class Tamagotchi_MainPatch
{
    [HarmonyPatch(nameof(Tamagotchi_Main.GameStart))]
    [HarmonyPrefix]
    public static void GameStartPatch()
    {
        if (PracticeManager.CurrentMinigame == ChapterMinigame.TamagotchiFull)
        {
            TamagotchiFullMinigame.TamagotchiLoaded();
        }
    }
}