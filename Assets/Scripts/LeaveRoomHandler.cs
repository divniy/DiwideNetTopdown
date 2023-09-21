using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diwide.Topdown
{
    [RequireComponent(typeof(GameManager))]
    public class LeaveRoomHandler : MonoBehaviour
    {
        [SerializeField] private InputAction leaveRoomAction;

        private GameManager _gameManager;

        private void OnEnable() => leaveRoomAction.Enable();
        private void OnDisable() => leaveRoomAction.Disable();

        private void Start()
        {
            _gameManager = GetComponent<GameManager>();
            leaveRoomAction.performed += OnLeaveRoomActionPerformed;
        }

        private void OnLeaveRoomActionPerformed(InputAction.CallbackContext obj)
        {
            _gameManager.LeaveRoom();
        }
    }
}