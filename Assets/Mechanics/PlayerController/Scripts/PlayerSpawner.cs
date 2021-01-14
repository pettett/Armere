using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Armere.PlayerController
{
    public class PlayerSpawner : MonoSaveable
    {
        public PlayerController player;
        private void Start()
        {
            SceneManager.sceneLoaded += OnLevelLoaded;
            player = PlayerController.activePlayerController;
        }
        public void OnLevelLoaded(Scene s, LoadSceneMode l)
        {
            //Debug.Log("Level Loaded");
            player = PlayerController.activePlayerController;
        }

        public override void SaveBin(GameDataWriter writer)
        {
            player = PlayerController.activePlayerController;
            player.SaveBin(writer);
        }

        public override void LoadBin(Version saveVersion, GameDataReader reader)
        {
            Debug.Log("Player Loaded");
            player = PlayerController.activePlayerController;
            player.LoadBin(saveVersion, reader);
        }
    }
}
