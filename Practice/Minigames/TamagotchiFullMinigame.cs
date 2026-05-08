using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice.Minigames;

public static class TamagotchiFullMinigame
{
    private static int _loadQueued;

    private static int _queueMobileInteractiveClick;
    private static int _queueButtonActivate;
    private static int _queueButtonClick;

    private static GameObject _smartPhone;
    private static World _gameWorld;
    private static Camera _camera;

    public static void QueueLoad()
    {
        _loadQueued = 30;

        _queueMobileInteractiveClick = 0;
        _queueButtonActivate = 0;
        _queueButtonClick = 0;

        _smartPhone = null;
        _gameWorld = null;
        _camera = null;
    }

    internal static void Update()
    {
        if (_loadQueued > 0)
        {
            _loadQueued--;
            if (_loadQueued == 0)
            {
                Load();
            }
        }

        if (_queueMobileInteractiveClick > 0)
        {
            _queueMobileInteractiveClick--;
            if (_queueMobileInteractiveClick == 0)
            {
                MobileButtonInteractiveClick();
            }
        }

        if (_queueButtonActivate > 0)
        {
            _queueButtonActivate--;
            if (_queueButtonActivate == 0)
            {
                ButtonActivate();
            }
        }

        if (_queueButtonClick > 0)
        {
            _queueButtonClick--;
            if (_queueButtonClick == 0)
            {
                ButtonClick();
            }
        }
    }

    private static void Load()
    {
        _gameWorld = Object.FindObjectOfType<World>();

        _smartPhone = _gameWorld.transform.Find("World RealRoom/Smartphone").gameObject;

        GameObject gameControlerGameObject = Object.FindObjectOfType<GameController>().gameObject;
        gameControlerGameObject.transform.Find("Player").gameObject.active = true;

        GameObject mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        mobileInteractive.active = true;
        ObjectInteractive mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.active = true;

        CleanupStartingScene(_gameWorld);

        _camera = _gameWorld.transform.Find("CutScenes/CutScene 1 (ДЕНЬ 1)/Camera/MainCamera").GetComponent<Camera>();
        _camera.gameObject.active = false;

        _queueMobileInteractiveClick = 1;
        Time.timeScale = 10f;
    }

    private static void MobileButtonInteractiveClick()
    {
        GameObject mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        ObjectInteractive mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.Click();

        _queueButtonActivate = 300;
    }

    private static void ButtonActivate()
    {
        _smartPhone.transform.Find("3D HintKey OpenMessage").gameObject.active = false;

        GameObject playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        playButton.active = true;

        _queueButtonClick = 2;
    }

    private static void ButtonClick()
    {
        GameObject playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        Interface_KeyHint_Key keyHint = playButton.GetComponent<Interface_KeyHint_Key>();
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
        Time.timeScale = 1f;
        _camera.gameObject.active = true;
    }
}
