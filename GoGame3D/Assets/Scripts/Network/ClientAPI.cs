using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ClientAPI : MonoBehaviour
{

    private static readonly string BaseURL = "localhost:8080/GoGame";
    private static readonly HttpClient Client = new HttpClient();

    public static async Task CallGet<T>(string call, Action<T, HttpResponseMessage> OnSuccessfull = null, Action OnFailure = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(BaseURL + call, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync();
            T result = JsonConvert.DeserializeObject<T>(responseBody);
            OnSuccessfull?.Invoke(result, response);
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
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            T result = JsonConvert.DeserializeObject<T>(responseBody);

            OnSuccessfull?.Invoke(result, response);
        }
        catch (HttpRequestException e)
        {
            Debug.LogError(e);
            OnFailure?.Invoke();
        }
    }
}
