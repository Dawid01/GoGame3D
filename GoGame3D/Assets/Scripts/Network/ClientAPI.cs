using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ClientAPI : MonoBehaviour
{

    private static readonly string BaseURL = "http://localhost:8080";
    private static readonly HttpClient Client = new HttpClient();

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
    
    public static void PlayerLoged(LoginRequest loginRequest)
    {
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(loginRequest.Email + ":" + loginRequest.Password));
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        
    }
}
