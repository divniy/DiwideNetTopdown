using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Diwide.Topdown
{
    public class Launcher : MonoBehaviourPunCallbacks
    {

        private bool _isConnecting;

        [SerializeField] private string gameVersion = "0.0.1";
        
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private TMP_Text consoleTextArea;
        
        
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void Connect()
        {
            consoleTextArea.text = "";
            _isConnecting = true;
            controlsPanel.SetActive(false);

            if (PhotonNetwork.IsConnected)
            {
                LogState("Joining random room...");
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                LogState("Connecting Photon Online Server...");
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            Application.Quit();
#endif
        }
        
        void LogState(string message)
        {
            if (consoleTextArea == null) return;
            consoleTextArea.text += System.Environment.NewLine+message;
        }

        #region PUN Callbacks

        public override void OnConnectedToMaster()
        {
            if (_isConnecting)
            {
                LogState("OnConnectedToMaster. Try to join any room...");
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            LogState("OnJoinRandomFailed. Creating new one.");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        }

        public override void OnJoinedRoom()
        {
            LogState($"OnJoinedRoom. Current players number is {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log("OnPlayerEnteredRoom. Loading arena.");
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("NetGameScene");
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            LogState("OnDisconnected. Enable controls");
            _isConnecting = false;
            controlsPanel.SetActive(true);
        }

        #endregion
        
    }
}