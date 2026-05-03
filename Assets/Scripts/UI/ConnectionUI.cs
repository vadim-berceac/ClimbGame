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
        ConnectionService.Connect(new ConnectionData(ConnectionType.Host,
            null, GetPort(settings.PortField.text)));
    }
    
    private void OnStartServer()
    {
        ConnectionService.Connect(new ConnectionData(ConnectionType.Server,
            null, GetPort(settings.PortField.text)));
    }

    private void OnStartClient()
    {
        ConnectionService.Connect(new ConnectionData(ConnectionType.Client,
            settings.IpField.text, GetPort(settings.PortField.text)));
    }

    private static ushort GetPort(string port)
    {
        return ushort.Parse(port);
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