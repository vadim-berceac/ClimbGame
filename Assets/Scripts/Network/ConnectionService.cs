using System;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

public enum ConnectionType
{
    Host,
    Server,
    Client
}

public readonly struct ConnectionData
{
    public readonly ConnectionType Type;
    public readonly string ConnectedTo;
    public readonly ushort Port;

    public ConnectionData(ConnectionType type, string connectedTo, ushort port)
    {
        Type = type;
        ConnectedTo = connectedTo;
        Port = port;
    }
    
    public bool Equals(ConnectionData other)
    {
        return Type == other.Type &&
               string.Equals(ConnectedTo, other.ConnectedTo, StringComparison.Ordinal) &&
               Port == other.Port;
    }
    
    public override string ToString() => $"{Type}:{ConnectedTo}:{Port}";
}

public static class ConnectionService
{
    public static ConnectionData CurrentConnection { get; private set; }
    
    public static event Action<ConnectionData> ConnectionChanged;
    
    private const string DefaultIp = "0.0.0.0";
    
    [Inject] private static UnityTransport _unityTransport;

    public static void Connect(ConnectionData connectionData)
    {
        if(connectionData.Equals(CurrentConnection)) return;
        
        switch (connectionData.Type)
        {
            case ConnectionType.Host:
                StartHost(connectionData);
                break;
            
            case ConnectionType.Server:
                StartServer(connectionData);
                break;
            
            case ConnectionType.Client:
                StartClient(connectionData);
                break;
        }
        
        ConnectionChanged?.Invoke(CurrentConnection);
        Debug.Log(CurrentConnection.ToString());
    }

    private static void StartHost(ConnectionData connectionData)
    {
        CurrentConnection = new ConnectionData(connectionData.Type, GetLocalIPv4ForDisplay(), connectionData.Port);
        
        StartTransport(DefaultIp, CurrentConnection.Port, NetworkManager.Singleton.StartHost);
    }

    private static void StartServer(ConnectionData connectionData)
    {
        CurrentConnection = new ConnectionData(connectionData.Type, GetLocalIPv4ForDisplay(), connectionData.Port);
        
        StartTransport(DefaultIp, CurrentConnection.Port, NetworkManager.Singleton.StartServer);
    }

    private static void StartClient(ConnectionData connectionData)
    {
        CurrentConnection = connectionData;
        
        StartTransport(CurrentConnection.ConnectedTo, CurrentConnection.Port, NetworkManager.Singleton.StartClient);
    }

    private static bool StartTransport(string ip, ushort port, Func<bool> startAction)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);
    
        return startAction?.Invoke() ?? false;
    }
    
    private static string GetLocalIPv4ForDisplay()
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
}
