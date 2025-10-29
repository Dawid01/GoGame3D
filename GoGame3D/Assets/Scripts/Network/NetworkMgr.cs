using System;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class NetworkMgr : Singleton<NetworkMgr>
{
    public WebSocket Websocket;
    private static string ServerUrl => $"ws://localhost:8080/ws/game?token={ClientAPI.AuthStorage.AccessToken}";

    public PlayerData currentPlayer;
    public Room currentRoom;

    public static Action<PlayerData> onPlayerJoin;
    public static Action<PlayerData> onPlayerLeave;


    async public void Connect()
    {
        
        Websocket = new WebSocket(ServerUrl);

        Websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");

        };

        Websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            Logout();
        };

        Websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed! Close code: " + e);
            Logout();
        };

        Websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            HandleIncomingMessage(msg);
            
        };
        await Websocket.Connect();
        _ = KeepAliveLoop();
    }
    
    async Task KeepAliveLoop()
    {
        while (currentPlayer != null)
        {
            if (Websocket.State == WebSocketState.Open)
                await Websocket.SendText("ping");
            await Task.Delay(5000);
        }
    }
    
    private void Update()
    {
        if(Websocket == null) return;
#if !UNITY_WEBGL || UNITY_EDITOR
        Websocket.DispatchMessageQueue();
#endif
    }
    
    private async void OnApplicationQuit()
    {
        if (Websocket != null && Websocket.State == WebSocketState.Open)
        {
            await Websocket.Close();
        }
        
    }
    
    public void SendWebSocketMessage<T>(GameMessage<T> message)
    {
        if(Websocket != null && Websocket.State == WebSocketState.Open)
        {
            string jsonMessage = JsonUtility.ToJson(message);
            Websocket.SendText(jsonMessage);
        }
    }
    
    
    public void HandleIncomingMessage(string json)
    {
        
        if (json.StartsWith("Ack: "))
        {
            json = json.Substring(5);
        }
        
        GameMessageBase baseMsg = JsonUtility.FromJson<GameMessageBase>(json);

        switch (baseMsg.type)
        {
            case "ASSIGN_ID":
                var assignIdMsg = JsonUtility.FromJson<GameMessageBase>(json);
                currentPlayer = new PlayerData(assignIdMsg.sessionId, ClientAPI.LoggedUser);
                break;
            case "JOIN_ROOM":
                var joinRoomMsg = JsonUtility.FromJson<GameMessage<PlayerData>>(json);
                onPlayerJoin?.Invoke(joinRoomMsg.data);
                break;
            case "LEAVE_ROOM":
                var leaveRoomMsg = JsonUtility.FromJson<GameMessage<PlayerData>>(json);
                onPlayerLeave?.Invoke(leaveRoomMsg.data);
                break;
            default:
                break;
        }
        
    }
    
    public void Logout()
    {
        ClientAPI.Logout();
        Websocket.Close();
        currentPlayer = null;
        currentRoom = null;
        UIMgr.Instance.ActiveElement("LoginPanel");
    }
}



[Serializable]
public class GameMessageBase
{
    public string type;
    public string sessionId;

}

[Serializable]
public class GameMessage<T> : GameMessageBase
{
    public string roomId;
    public T data;

}

[Serializable]
public class PlayerData
{
    public string sessionId;
    public User user;
    public string currentRoomId;

    public PlayerData(string sessionId, User user)
    {
        this.sessionId = sessionId;
        this.user = user;
    }
}



