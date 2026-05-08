using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Practice.Minigames;

public static class TamagotchiFullMinigame
{
    private static readonly ActionQueue Queue = new();

    private static GameObject _smartPhone;
    private static World _gameWorld;
    private static Camera _camera;

    public static void QueueLoad()
    {
        Queue.Clear();

        _smartPhone = null;
        _gameWorld = null;
        _camera = null;

        Queue.EnqueueWait(seconds: 0.5f);
        Queue.Enqueue(Load);
    }

    internal static void Update()
    {
        Queue.Tick();
    }

    private static void Load()
    {
        _gameWorld = Object.FindObjectOfType<World>();

        _smartPhone = _gameWorld.transform.Find("World RealRoom/Smartphone").gameObject;

        // We need to enable the player otherwise the smartphone grab action won't work
        var gameControlerGameObject = Object.FindObjectOfType<GameController>().gameObject;
        gameControlerGameObject.transform.Find("Player").gameObject.active = true;

        // We enable the mobile interactive so it can be used in a next frame
        var mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        mobileInteractive.active = true;

        var mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.active = true;

        // By cleaning up the starting scene we can skip the cutscene
        CleanupStartingScene(_gameWorld);

        // To prevent the player from seeing the game fastforward we disable the camera shotly
        _camera = _gameWorld.transform.Find("CutScenes/CutScene 1 (ДЕНЬ 1)/Camera/MainCamera").GetComponent<Camera>();
        _camera.gameObject.active = false;

        // By speeding up the game 10 times it'll play the animations faster
        // The phone grab animation has to finish before we can continue, I don't know why
        Time.timeScale = 10f;

        Queue.EnqueueWait(seconds: 0.1f);
        Queue.Enqueue(MobileButtonInteractiveClick);
    }

    private static void MobileButtonInteractiveClick()
    {
        var mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        var mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.Click();

        Queue.EnqueueWait(seconds: 1f);
        Queue.Enqueue(ButtonActivate);
    }

    private static void ButtonActivate()
    {
        _smartPhone.transform.Find("3D HintKey OpenMessage").gameObject.active = false;

        var playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        playButton.active = true;

        Queue.EnqueueWait(seconds: 0.1f);
        Queue.Enqueue(ButtonClick);
    }

    private static void ButtonClick()
    {
        var playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        var keyHint = playButton.GetComponent<Interface_KeyHint_Key>();
        keyHint.KeyDown();
    }

    private static void CleanupStartingScene(World gameWorld)
    {
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during 2DCutting Practice CleanupStart");
            return;
        }

        Transform gameTransform = gameWorld.gameObject.transform;
        gameTransform.Find("CutScenes/CutScene 1 (ДЕНЬ 1)").gameObject.active = false;
    }

    public static void TamagotchiLoaded()
    {
        // Once the tamagotchi game has been loaded we can reset the timescale and enable the camera again
        Time.timeScale = 1f;
        _camera.gameObject.active = true;
    }
}
