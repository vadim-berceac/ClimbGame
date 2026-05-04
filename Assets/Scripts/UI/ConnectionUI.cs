using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private ConnectionUISettings settings;
    [SerializeField] private string firstSceneName;
    
    private void Start()
    {
        settings.StartHost.onClick.AddListener(OnStartHost);
        settings.StartServer.onClick.AddListener(OnStartServer);
        settings.StartClient.onClick.AddListener(OnStartClient);
    }

    private void OnStartHost()
    {
        if (ConnectionService.Connect(new ConnectionData(ConnectionType.LocalHost,
                null, GetPort(settings.PortField.text))))
        {
            ConnectionService.LoadNetworkScene(firstSceneName);
        }
    }
    
    private void OnStartServer()
    {
        if (ConnectionService.Connect(new ConnectionData(ConnectionType.LocalServer,
                null, GetPort(settings.PortField.text))))
        {
            ConnectionService.LoadNetworkScene(firstSceneName);
        }
    }

    private void OnStartClient()
    {
        ConnectionService.Connect(new ConnectionData(ConnectionType.LocalClient,
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