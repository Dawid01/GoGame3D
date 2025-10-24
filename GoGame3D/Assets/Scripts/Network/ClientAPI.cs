using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClientAPI : MonoBehaviour
{

    private static readonly string BaseURL = "http://localhost:8080";
    private static readonly HttpClient Client = new HttpClient();
    public static bool IsLogged { get; private set; }
    public static User LoggedUser { get; private set; }

    public static async Task CallGet<T>(string call, Action<T, HttpResponseMessage> OnSuccessfull = null, Action OnFailure = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(BaseURL + call, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                T result = JsonConvert.DeserializeObject<T>(responseBody);
                OnSuccessfull?.Invoke(result, response);
            }
            else
            {
                Debug.LogError("CallGet failed: " + response.StatusCode);
                OnFailure?.Invoke();
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError(e);
            OnFailure?.Invoke();
        }
        
    }
    
    public static async Task CallPost<T, U>(string call, U data, Action<T, HttpResponseMessage> OnSuccessfull = null, Action OnFailure = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await Client.PostAsync(BaseURL + call, content, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(string))
                {
                    OnSuccessfull?.Invoke((T)(object)responseBody, response);
                }
                else
                {
                    T result = JsonConvert.DeserializeObject<T>(responseBody);
                    OnSuccessfull?.Invoke(result, response);
                }
            }
            else
            {
                Debug.LogError("CallPost failed: " + response.StatusCode);
                OnFailure?.Invoke();
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError(e);
            OnFailure?.Invoke();
        }
    }
    
    public static void PlayerLoged(LoginRequest loginRequest, User user)
    {
        IsLogged = true;
        LoggedUser = user;
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(loginRequest.Email + ":" + loginRequest.Password));
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        
    }
    
    public static async Task LoadImageAsync(string url, Image img)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URL is blank!");
            return;
        }

        using var www = UnityWebRequestTexture.GetTexture(url);
        var operation = www.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"ERROR: {www.error}");
            return;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(www);
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        if (img != null)
            img.sprite = sprite;
        else
            Debug.LogWarning("Image is null!");
    }

    public static void Logout()
    {
        LoggedUser = null;
        IsLogged = false;
        Client.DefaultRequestHeaders.Authorization = null;
    }
}
