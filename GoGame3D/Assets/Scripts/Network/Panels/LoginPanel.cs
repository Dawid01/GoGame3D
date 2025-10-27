using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TextMeshProUGUI emailError;
    
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TextMeshProUGUI passwordError;

    [SerializeField] private Toggle rememberMeToggle;

    private void OnEnable()
    {
        emailError.text = "";
        passwordError.text = "";
        string email = PlayerPrefs.GetString("RememberEmail", "");
        rememberMeToggle.isOn = email.Length > 0;
        emailField.text = email;
    }

    public void Login()
    {
        string email = emailField.text;
        string password = passwordField.text;
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

        _ = LoginTask(email, password, rememberMeToggle.isOn);
    }

    private async Task LoginTask(string email, string password, bool remember)
    {
        LoginRequest loginRequest = new LoginRequest(email, password);
        UIMgr.Instance.ShowLoadingPanel();

        await ClientAPI.CallPost<ClientAPI.AuthTokens, LoginRequest>(
            "/auth/login",
            loginRequest,
            (authTokens, httpResponse) => 
            {
                if (authTokens == null || string.IsNullOrEmpty(authTokens.accessToken))
                {
                    Debug.LogError("Login failed: no access token returned");
                    UIMgr.Instance.HideLoadingPanel();
                    return;
                }

                ClientAPI.PlayerLogged(authTokens.userDTO, authTokens.accessToken, authTokens.refreshToken);

                UIMgr.Instance.ActiveElement("Menu Panel");
                NetworkMgr.Instance.Connect();
                UIMgr.Instance.IsLoggedInitialize();
                emailField.text = "";
                passwordField.text = "";
                PlayerPrefs.SetString("RememberEmail", remember ? email : "");
                UIMgr.Instance.HideLoadingPanel();
            },
            OnFailure: () => 
            {
                emailError.gameObject.SetActive(true);
                passwordError.gameObject.SetActive(true);
                emailError.text = "Email or password may be incorrect.";
                passwordError.text = "Email or password may be incorrect.";
                UIMgr.Instance.HideLoadingPanel();
            }
        );
    }


    public void SetEmail(string email)
    {
        emailField.text = email;
        passwordField.text = "";
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

