using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class ButtonInput : MonoBehaviour
{
    public InputActionReference exitAction;

    void Update()
    {
        if (exitAction != null && exitAction.action != null && exitAction.action.WasPressedThisFrame())
        {
            QuitGame();
        }

        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            QuitGame();
        }
    }

    void QuitGame()
    {
        Debug.Log("Quit Game Triggered!");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
