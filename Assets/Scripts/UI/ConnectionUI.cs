using System.Net;
using System.Net.Sockets;
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
        var port = ushort.Parse(settings.PortField.text);
        Debug.Log($"[HOST] Запуск хоста на порте {port}");

        try
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", (ushort)port);
            NetworkManager.Singleton.StartHost();
            var ip = GetLocalIPv4();
            Debug.Log($"[HOST] Хост запущен {ip}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HOST] Ошибка: {ex.Message}");
        }
    }

    private void OnStartClient()
    {
        var ipAddress = settings.IpField.text;
        var port = ushort.Parse(settings.PortField.text);
    
        Debug.Log($"[CLIENT] Попытка подключиться к: {ipAddress}:{port}");
    
        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.LogError("[CLIENT] IP пусто!");
            return;
        }

        try
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            Debug.Log($"[CLIENT] UnityTransport найден: {transport != null}");
        
            transport.SetConnectionData(ipAddress, (ushort)port);
            Debug.Log("[CLIENT] SetConnectionData выполнен");
        
            NetworkManager.Singleton.StartClient();
            Debug.Log("[CLIENT] StartClient вызван");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CLIENT] Ошибка: {ex.Message}");
        }
    }

    private void OnStartServer()
    {
        var port = ushort.Parse(settings.PortField.text);
        Debug.Log($"[SERVER] Запуск сервера на порте {port}");
        try
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", (ushort)port);
            NetworkManager.Singleton.StartServer();
            var ip = GetLocalIPv4();
            Debug.Log($"[SERVER] Сервер запущен {ip}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SERVER] Ошибка: {ex.Message}");
        }
    }

    private static string GetLocalIPv4()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "No local IPv4 address found";
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