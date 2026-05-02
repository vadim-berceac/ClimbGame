using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private ConnectionUISettings settings;
    
    private void Start()
    {
        settings.StartHost.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        settings.StartServer.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        settings.StartClient.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }

    private void OnDisable()
    {
        settings.StartHost.onClick.RemoveListener(() => NetworkManager.Singleton.StartHost());
        settings.StartServer.onClick.RemoveListener(() => NetworkManager.Singleton.StartServer());
        settings.StartClient.onClick.RemoveListener(() => NetworkManager.Singleton.StartClient());
    }
}

[System.Serializable]
public struct ConnectionUISettings
{
    [field: SerializeField] public Button StartHost { get; private set; }
    [field: SerializeField] public Button StartServer { get; private set; }
    [field: SerializeField] public Button StartClient { get; private set; }
    [field: SerializeField] public TMPro.TMP_InputField IpField { get; private set; }
}
