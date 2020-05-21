using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PlayerController
{
    [CustomEditor(typeof(Player_CharacterController))]
    public class Player_CharacterControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}

