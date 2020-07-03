using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{

    public UIMenu tabMenu;
    public GameObject buyMenu;
    public GameObject sellMenu;
    public WorldIndicator itemIndicator;
    public WorldIndicator npcIndicator;
    public static UIController singleton;


    private void Awake()
    {

        singleton = this;
    }

    public static void SetTabMenu(bool active)
    {
        if (active)
            singleton.tabMenu.OpenMenu();
        else
            singleton.tabMenu.CloseMenu();
    }
}
