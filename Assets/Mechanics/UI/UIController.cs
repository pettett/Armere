using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{

    public GameObject tabMenu;


    public static UIController singleton;



    private void Awake() {

        singleton=this;
    }

    public static void SetTabMenu(bool active){
       singleton.tabMenu.SetActive(active);
    }
}
