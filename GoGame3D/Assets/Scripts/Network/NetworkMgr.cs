using System;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class NetworkMgr : Singleton<NetworkMgr>
{
    public WebSocket websocket;
    private static readonly string ServerUrl = "ws://localhost:8080/GoGame/ws/game";

    
    async public void Connect()
    {
        
        websocket = new WebSocket(ServerUrl);
        websocket.OnOpen += async () =>
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
        while (true)
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
            case "EXAMPLE":
                var msg = JsonUtility.FromJson<GameMessage<long>>(json);
                break;
            default:
                break;
        }
    }
    
}

[Serializable]
public class GameMessageBase
{
    public string playerId;
    public string type;
    public string roomId;

}

[Serializable]
public class GameMessage<T> : GameMessageBase
{
    public T data;
}
