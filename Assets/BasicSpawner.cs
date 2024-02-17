using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public Transform spawnTransform;
    public Camera mainCamera;
    public Material[] playerMaterials; // TODO change to colors
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    // Events
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
            networkPlayerObject.GetComponent<Player>().SetInitialPosition(spawnTransform.position);
            
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);

            AssignPlayerColor(networkPlayerObject);
        }
    }

    private void AssignPlayerColor(NetworkObject player)
    {
        if (_spawnedCharacters.Count > playerMaterials.Length)
        {
            Debug.LogError($"Spawned Players({_spawnedCharacters.Count}) > Player materials({_spawnedCharacters.Count})");
        }

        player.GetComponent<Player>().playerColor_r = (byte)Mathf.RoundToInt(playerMaterials[_spawnedCharacters.Count-1].color.r * 255f);
        player.GetComponent<Player>().playerColor_g = (byte)Mathf.RoundToInt(playerMaterials[_spawnedCharacters.Count-1].color.g * 255f);
        player.GetComponent<Player>().playerColor_b = (byte)Mathf.RoundToInt(playerMaterials[_spawnedCharacters.Count-1].color.b * 255f);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
        var data = new NetworkInputData();
        Vector3 point = FindMousePosition();
        if (point != Vector3.zero)
        {
            Debug.Log($"point is {point}");
            data.position = point;
        }

        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
    
    private NetworkRunner _runner;

    async void StartGame(GameMode mode)
    {
        rayLayerMask = LayerMask.GetMask(new[] { "Ground"});
        mainCamera = Camera.main;
        
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }

            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    private Vector3 screenPos;
    private Ray ray;
    public float rayRange = 100f;
    public int rayLayerMask;
    private Vector3 FindMousePosition()
    {
        screenPos = Input.mousePosition;
        ray = mainCamera.ScreenPointToRay(screenPos);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            rayRange,
            rayLayerMask
        );
        if (hits.Length > 0)
        {
            RaycastHit[] sortedHits;
            if (hits.Length > 1)
            {
                sortedHits = hits;
                // sortedHits = new RaycastHit[hits.Length];
                // sortedHits = hits.OrderBy(item => (item.transform.position - transform.position).sqrMagnitude)
                    // .ToArray();    
            }
            else
            {
                sortedHits = hits;
            }
            
            foreach (RaycastHit hit in sortedHits)
            {
                string hitLayerName = LayerMask.LayerToName(hit.transform.gameObject.layer);

                if (hitLayerName == "Ground")
                {
                    if (hitGround(hit))
                    {
                        return hit.point;
                    }
                }
            }
        }

        return Vector3.zero;
    }

    private bool hitGround(RaycastHit hit)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("click M1");
            // if (!EventSystem.current.IsPointerOverGameObject())
            // {
                return true;
            // }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("click M2");
            return true;
        }
        return false;
    }
}
