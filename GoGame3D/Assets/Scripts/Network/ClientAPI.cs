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

    public static class AuthStorage
    {
        public static string AccessToken { get; set; }
        public static string RefreshToken { get; set; }
    }

    public static async Task CallGet<T>(string call, Action<T, HttpResponseMessage> OnSuccess = null, Action OnFailure = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();
            using HttpResponseMessage response = await Client.GetAsync(BaseURL + call, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                T result = JsonConvert.DeserializeObject<T>(responseBody);
                OnSuccess?.Invoke(result, response);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                bool refreshed = await RefreshAccessToken();
                if (refreshed)
                {
                    await CallGet<T>(call, OnSuccess, OnFailure, cancellationToken);
                }
                else
                {
                    OnFailure?.Invoke();
                }
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

    
    public static async Task CallPost<T, U>(string call, U data, Action<T, HttpResponseMessage> OnSuccess = null, Action OnFailure = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();

            string json = JsonConvert.SerializeObject(data);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await Client.PostAsync(BaseURL + call, content, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                T result = typeof(T) == typeof(string)
                    ? (T)(object)responseBody
                    : JsonConvert.DeserializeObject<T>(responseBody);

                OnSuccess?.Invoke(result, response);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                bool refreshed = await RefreshAccessToken();
                if (refreshed)
                {
                    await CallPost<T, U>(call, data, OnSuccess, OnFailure, cancellationToken);
                }
                else
                {
                    OnFailure?.Invoke();
                }
            }

        }
        catch (HttpRequestException e)
        {
            Debug.LogError(e);
            OnFailure?.Invoke();
        }
    }
    
    public static async Task<bool> RefreshAccessToken()
    {
        if (string.IsNullOrEmpty(AuthStorage.RefreshToken))
            return false;

        var data = new { refreshToken = AuthStorage.RefreshToken };
        bool success = false;

        await CallPost<AuthTokens, object>(
            "/auth/refresh",
            data,
            (authTokens, response) =>
            {
                if (authTokens != null && !string.IsNullOrEmpty(authTokens.accessToken))
                {
                    AuthStorage.AccessToken = authTokens.accessToken;
                    AuthStorage.RefreshToken = authTokens.refreshToken;
                    LoggedUser = authTokens.user;
                    success = true;
                }
            },
            OnFailure: () =>
            {
                Logout(); // refresh się nie powiódł → wyloguj
                success = false;
            }
        );

        return success;
    }


    private static void AddAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(AuthStorage.AccessToken))
        {
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthStorage.AccessToken);
        }
    }

    public static void PlayerLogged(User user, string accessToken, string refreshToken)
    {
        IsLogged = true;
        LoggedUser = user;
        AuthStorage.AccessToken = accessToken;
        AuthStorage.RefreshToken = refreshToken;
    }

    public static void Logout()
    {
        LoggedUser = null;
        IsLogged = false;
        AuthStorage.AccessToken = null;
        AuthStorage.RefreshToken = null;
        Client.DefaultRequestHeaders.Authorization = null;
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

    [Serializable]
    public class AuthTokens
    {
        public string accessToken;
        public string refreshToken;
        public User user;
    }
}
