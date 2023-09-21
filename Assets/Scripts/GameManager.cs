using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Diwide.Topdown
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        static public GameManager Instance;
        
        [SerializeField, Tooltip("The prefab to use for representing the master player")]
        private GameObject masterPlayerPrefab;
        
        [SerializeField, Tooltip("The prefab to use for representing the guest player")]
        private GameObject guestPlayerPrefab;
        private GameObject PlayerPrefab => (PhotonNetwork.IsMasterClient) ? masterPlayerPrefab : guestPlayerPrefab;
        
        private void Start()
        {
            Instance = this;

            if (!PhotonNetwork.IsConnected) 
                LoadLobbyScene();

            if (PhotonNetwork.InRoom && PlayerController.LocalPlayerInstance == null)
            {
                Vector2 randomSpawnPoint = Random.insideUnitCircle * 5;
                Vector3 spawnPosition = new Vector3(randomSpawnPoint.x, 0, randomSpawnPoint.y);
                PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPosition, Quaternion.identity);
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            LeaveRoom();
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            LoadLobbyScene();
        }

        private void OnQuitAction(InputAction.CallbackContext obj)
        {
            Debug.Log("Quit application performed");
            QuitApplication();
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            Application.Quit();
#endif
        }

        private void LoadLobbyScene()
        {
            SceneManager.LoadScene(0);
        }
    }
}