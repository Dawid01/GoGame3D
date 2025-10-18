using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TextMeshProUGUI emailError;
    
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TextMeshProUGUI passwordError;

    public void Login()
    {
        String email = emailField.text;
        String password = passwordField.text;
        bool isError = false;

        if (string.IsNullOrWhiteSpace(email))
        {
            isError = true;
            emailError.gameObject.SetActive(true);
            emailError.text = "Email field is empty!";
        }
        else if (!email.Contains("@"))
        {
            isError = true;
            emailError.gameObject.SetActive(true);
            emailError.text = "Invalid email format!";
        }
        else
        {
            emailError.gameObject.SetActive(false);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            isError = true;
            passwordError.gameObject.SetActive(true);
            passwordError.text = "Password field is empty!";
        }
        else
        {
            passwordError.gameObject.SetActive(false);
        }
        if(isError) return;

        _ = LoginTask(email, password);
    }

    private async Task LoginTask(string email, string password)
    {
        LoginRequest loginRequest = new LoginRequest(email, password);
        await ClientAPI.CallPost<string, LoginRequest>(
            "/auth/login",
            loginRequest,
            OnSuccessfull: (responseText, httpResponse) => {
                
                Debug.Log("Login successfu: " + responseText);
                UIMgr.Instance.ActiveElement("Menu Panel");
                ClientAPI.PlayerLoged(loginRequest);
                emailField.text = "";
                passwordField.text = "";

            },
            OnFailure: () => {
                Debug.LogError("Błąd logowania!");
            }
        );

    }
}

    
[Serializable]
public class LoginRequest
{
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }

    public LoginRequest(string email, string password)
    {
        Email = email;
        Password = password;
    }
        
}

public class RegisterRequest
{
    public string Email { get; set; }
    public String Nickname { get; set; }
    public string Password { get; set; }

    public RegisterRequest(string email, string nickname, string password)
    {
        Email = email;
        Nickname = nickname;
        Password = password;
    }
}
