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
    public static void ChangeToLevel(LevelName level)
    {
        LoadLevelAsync(level);
    }
    async static Task LoadLevelAsync(LevelName level)
    {
        AsyncOperation a = SceneManager.LoadSceneAsync(level.ToString());
        System.GC.Collect();
        while (a.progress < 1)
        {
            await Task.Delay(1000);
            Debug.Log(a.progress);
        }
    }

}
