using System;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class NetworkMgr : Singleton<NetworkMgr>
{
    public WebSocket websocket;
    private static readonly string ServerUrl = "ws://localhost:8080/ws/game";
    public PlayerData currentPlayer;
    
    async public void Connect()
    {
        
        websocket = new WebSocket(ServerUrl);
        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");

        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed! Close code: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            HandleIncomingMessage(msg);
            
        };
        await websocket.Connect();
        _ = KeepAliveLoop();
    }
    
    async Task KeepAliveLoop()
    {
        while (currentPlayer != null)
        {
            if (websocket.State == WebSocketState.Open)
                await websocket.SendText("ping");
            await Task.Delay(5000);
        }
    }
    
    private void Update()
    {
        if(websocket == null) return;
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }
    
    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
        
    }
    
    public void SendWebSocketMessage<T>(GameMessage<T> message)
    {
        if(websocket != null && websocket.State == WebSocketState.Open)
        {
            string jsonMessage = JsonUtility.ToJson(message);
            websocket.SendText(jsonMessage);
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
                var msg = JsonUtility.FromJson<GameMessage<GameMessageBase>>(json);
                currentPlayer = new PlayerData(msg.sessionId, ClientAPI.LoggedUser);
                InitializePlayer();
                break;
            default:
                break;
        }
        
    }


    private void InitializePlayer()
    {
        GameMessage<PlayerData> msg = new GameMessage<PlayerData>();
        msg.type = "INIT_PLAYER";
        msg.sessionId = currentPlayer.sessionId;
        msg.data = currentPlayer;
        SendWebSocketMessage(msg);
    }

    public void CreateRoom()
    {
        
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
    public T data;
    public string roomId;
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



