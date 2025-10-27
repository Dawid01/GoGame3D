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
                var msg = JsonUtility.FromJson<GameMessageBase>(json);
                currentPlayer = new PlayerData(msg.sessionId, ClientAPI.LoggedUser);
                //InitializePlayer();
                break;
            default:
                break;
        }
        
    }


    // private void InitializePlayer()
    // {
    //     // GameMessage<PlayerData> msg = new GameMessage<PlayerData>();
    //     // msg.type = "PLAYER_INIT";
    //     // msg.sessionId = currentPlayer.sessionId;
    //     // msg.data = currentPlayer;
    //     GameMessage<User> msg = new GameMessage<User>();
    //     msg.type = "PLAYER_INIT";
    //     msg.sessionId = currentPlayer.sessionId;
    //     msg.data = currentPlayer.user;
    //     SendWebSocketMessage(msg);
    // }

    public void CreateRoom()
    {
        
    }

    public void Logout()
    {
        ClientAPI.Logout();
        Websocket.Close();
        currentPlayer = null;
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

    public PlayerData(string sessionId, User user)
    {
        this.sessionId = sessionId;
        this.user = user;
    }
}



