using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
public enum LevelName
{
    test,
    stealthTest
}
public static class LevelController
{
    public static LevelName currentLevel;
    static System.Action<Scene, LoadSceneMode> onLevelLoaded;
    public static void ChangeToLevel(LevelName level, System.Action<Scene, LoadSceneMode> onLevelLoadedEvent)
    {
        currentLevel = level;
        onLevelLoaded = onLevelLoadedEvent;
        LoadLevelAsync(level);
    }
    async static Task LoadLevelAsync(LevelName level)
    {
        AsyncOperation a = SceneManager.LoadSceneAsync(level.ToString());
        SceneManager.sceneLoaded += OnSceneLoaded;

        System.GC.Collect();
        while (a.progress < 1)
        {
            await Task.Delay(100);
        }
    }
    static void OnSceneLoaded(Scene s, LoadSceneMode l)
    {
        onLevelLoaded?.Invoke(s, l);
    }

}
