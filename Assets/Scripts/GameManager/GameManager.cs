using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using GNW2.Input;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GNW2.GameManager
{
    public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _runner;

        [SerializeField] private NetworkPrefabRef _playerPrefab;
        private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
        private bool _isMouseButton0Pressed;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_InputField _input;
        #region NetworkRunner Callbacks
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Vector3 customLocation = new Vector3(1 * runner.SessionInfo.PlayerCount, 5, 0);
                NetworkObject playerNetworkObject = runner.Spawn(_playerPrefab, customLocation, Quaternion.identity);
                playerNetworkObject.AssignInputAuthority(player);
                _spawnedPlayers.Add(player, playerNetworkObject);
                
            }
        }
    [Rpc (RpcSources.StateAuthority, RpcTargets.Proxies)]
        void RpcSourcesAll()
        {
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedPlayers.TryGetValue(player, out NetworkObject playerNetworkObject))
            {
                runner.Despawn(playerNetworkObject);
                _spawnedPlayers.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            if (UnityEngine.Input.GetKey(KeyCode.W))
            {
                data.Direction += Vector3.forward;
            }
            if (UnityEngine.Input.GetKey(KeyCode.A))
            {
                data.Direction += Vector3.left;
            }
            if (UnityEngine.Input.GetKey(KeyCode.S))
            {
                data.Direction += Vector3.back;
            }
            if (UnityEngine.Input.GetKey(KeyCode.D))
            {
                data.Direction += Vector3.right;
            }
            data.buttons.Set(NetworkInputData.MOUSEBUTTON0,_isMouseButton0Pressed);
            data.buttons.Set(NetworkInputData.ISJUMP, UnityEngine.Input.GetKey(KeyCode.Space));
            input.Set(data);

        }
        
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){ }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason){ }
        public void OnConnectedToServer(NetworkRunner runner){ }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason){ }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){ }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){ }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message){ }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList){ }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data){ }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){ }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }

        public void OnSceneLoadDone(NetworkRunner runner){ }

        public void OnSceneLoadStart(NetworkRunner runner){ }
        #endregion

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                StartGame(GameMode.AutoHostOrClient);
                _button.transform.parent.gameObject.SetActive(false);
            });
        }

        private void Update()
        {
            _isMouseButton0Pressed = UnityEngine.Input.GetMouseButton(0);
        }

        async void StartGame(GameMode mode)
        {
            // lets fusion know that we will be sending input
            _runner = this.gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true; 

            //create the scene info to send to fusion
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            await _runner.StartGame(new StartGameArgs()
                {
                    GameMode = mode,
                    SessionName = "TestRoom",
                    Scene = scene,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                }
            );

        }

        private void OnGUI()
        {/*
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                {
                    StartGame(GameMode.Host);
                }
                if (GUI.Button(new Rect(0, 40, 200, 40), "Client"))
                {
                    StartGame(GameMode.Client);
                }
            }*/
        }
    }
}
