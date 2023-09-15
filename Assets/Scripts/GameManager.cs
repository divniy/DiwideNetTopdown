using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diwide.Topdown
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private InputAction quitAction;

        private void OnEnable() => quitAction.Enable();

        private void OnDisable() => quitAction.Disable();

        private void Start()
        {
            quitAction.performed += OnQuitAction;
        }

        private void OnQuitAction(InputAction.CallbackContext obj)
        {
            Debug.Log("Quit performed");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            Application.Quit();
#endif
        }
    }
}