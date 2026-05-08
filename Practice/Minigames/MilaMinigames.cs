using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SpeedrunMod.Practice.Minigames;

public static class MilaMinigames
{
    public enum MilaMinigameModes
    {
        Laser,
        Towers,
        Shapes,
        Invaders
    }

    private static readonly ActionQueue Queue = new();

    private static GameObject _minigame1Clone;
    private static GameObject _minigame2Clone;
    private static GameObject _minigame3Clone;
    private static GameObject _minigame4Clone;

    private static GameObject _minigameGameObject;
    private static Camera _camera;
    public static MilaMinigameModes MilaMinigameMode = MilaMinigameModes.Laser;
    public static bool LoopThroughAllMinigames = false;

    public static void QueueLoad()
    {
        Queue.Clear();

        _minigame1Clone = null;
        _minigame2Clone = null;
        _minigame3Clone = null;
        _minigame4Clone = null;
        _minigameGameObject = null;
        _camera = null;

        HideCamera();

        Time.timeScale = 10f;

        Queue.EnqueueWait(seconds: 1f);
        Queue.Enqueue(Load);
    }

    internal static void Update()
    {
        Queue.Tick();
    }

    private static void HideCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Plugin.Log.LogInfo("Unable to disable Camera because it can't be found");
        }
        else
        {
            _camera = camera;
            _camera.gameObject.active = false;
        }
    }

    private static void QueueReload()
    {
        Queue.EnqueueConditional(
            () => 
            _minigameGameObject == null &&
            _minigame1Clone != null &&
            _minigame2Clone != null &&
            _minigame3Clone != null &&
            _minigame4Clone != null,
            () =>
            {
                ReloadGame();
                QueueReload();
            });
    }

    private static void QueueStartGame(Location19_GlitchGame game)
    {
        game.gameObject.active = true;
        _minigameGameObject = game.gameObject;
        Queue.EnqueueWait(seconds: 0.05f);
        Queue.Enqueue(() => StartGame(game));
    }

    private static void Load()
    {
        Time.timeScale = 1f;
        CleanupStartingScene();

        Location19_GlitchGame[] games = Object.FindObjectsOfType<Location19_GlitchGame>(true);
        foreach (Location19_GlitchGame glitchGame in games)
        {
            switch (glitchGame.gameObject.name)
            {
                case "GlitchGame 1":
                    _minigame1Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame1Clone.active = false;
                    break;
                case "GlitchGame 2":
                    _minigame2Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame2Clone.active = false;
                    break;
                case "GlitchGame 3":
                    _minigame3Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame3Clone.active = false;
                    break;
                case "GlitchGame 4":
                    _minigame4Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame4Clone.active = false;
                    break;
            }
        }

        ReloadGame();
        QueueReload();
    }

    private static void StartGame(Location19_GlitchGame game)
    {
        Time.timeScale = 1f;
        if (_camera != null)
        {
            _camera.gameObject.active = true;
            _camera = null;
        }

        game.gameObject.active = true;
        _minigameGameObject = game.gameObject;

        game.PlayGame();
    }

    private static void CleanupStartingScene()
    {
        World gameWorld = Object.FindObjectOfType<World>();

        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during LaserMinigame Practice CleanupStart");
            return;
        }

        Transform gameTransform = gameWorld.gameObject.transform;
        gameTransform.Find("Dialogues").gameObject.active = false;
        gameTransform.Find("Quests/General").gameObject.active = false;
        gameTransform.Find("Quests/Quest 1 Знакомство").gameObject.active = false;
        gameTransform.Find("Quests/Quest 2 Симулятор жизни").gameObject.active = true;
    }

    private static void ReloadGame()
    {
        if (LoopThroughAllMinigames)
        {
            MilaMinigameMode += 1;
            if (MilaMinigameMode > MilaMinigameModes.Invaders) MilaMinigameMode = MilaMinigameModes.Laser;
        }

        GameObject go = null;
        switch (MilaMinigameMode)
        {
            case MilaMinigameModes.Laser:
                go = Object.Instantiate(_minigame1Clone, _minigame1Clone.transform.parent);
                break;
            case MilaMinigameModes.Towers:
                go = Object.Instantiate(_minigame2Clone, _minigame2Clone.transform.parent);
                break;
            case MilaMinigameModes.Shapes:
                go = Object.Instantiate(_minigame3Clone, _minigame3Clone.transform.parent);
                break;
            case MilaMinigameModes.Invaders:
                go = Object.Instantiate(_minigame4Clone, _minigame4Clone.transform.parent);
                break;
        }

        if (go == null)
        {
            Plugin.Log.LogError("While playing Mila minigames and reloading a gameobject couldn't be created");
            return;
        }

        Location19_GlitchGame game = go.GetComponent<Location19_GlitchGame>();

        QueueStartGame(game);
    }

    public static void GameEnded(Location19_GlitchGame game)
    {
        Time.timeScale = 10f;
        game.eventReady = new UnityEvent();
        HideCamera();
    }
}
