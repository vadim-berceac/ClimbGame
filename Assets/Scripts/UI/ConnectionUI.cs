using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private ConnectionUISettings settings;
    
    private void Start()
    {
        settings.StartHost.onClick.AddListener(OnStartHost);
        settings.StartServer.onClick.AddListener(OnStartServer);
        settings.StartClient.onClick.AddListener(OnStartClient);
    }

    private void OnStartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void OnStartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    private void OnStartClient()
    {
        var ipAddress = settings.IpField.text;
        ushort.TryParse(settings.PortField.text, out var port);
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.LogError("IP address is empty!");
            return;
        }

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, port);
        NetworkManager.Singleton.StartClient();
    }

    private void OnDisable()
    {
        if (settings.StartHost.onClick != null)
            settings.StartHost.onClick.RemoveListener(OnStartHost);
        if (settings.StartServer.onClick != null)
            settings.StartServer.onClick.RemoveListener(OnStartServer);
        if (settings.StartClient.onClick != null)
            settings.StartClient.onClick.RemoveListener(OnStartClient);
    }
}

[System.Serializable]
public struct ConnectionUISettings
{
    [field: SerializeField] public Button StartHost { get; private set; }
    [field: SerializeField] public Button StartServer { get; private set; }
    [field: SerializeField] public Button StartClient { get; private set; }
    [field: SerializeField] public TMPro.TMP_InputField IpField { get; private set; }
    [field: SerializeField] public TMPro.TMP_InputField PortField { get; private set; }
}