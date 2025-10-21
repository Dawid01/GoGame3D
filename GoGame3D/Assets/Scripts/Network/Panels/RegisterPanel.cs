using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public enum RegisterStatus{
    OK,
    EMAIL_EXIST,
    NICKNAME_USED
}

public class RegisterPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TextMeshProUGUI emailError;
    
    [SerializeField] private TMP_InputField nicknameField;
    [SerializeField] private TextMeshProUGUI nicknameError;
    
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TextMeshProUGUI passwordError;
    
    [SerializeField] private TMP_InputField confirmPasswordField;
    [SerializeField] private TextMeshProUGUI confirmPasswordError;

    [SerializeField] private LoginPanel loginPanel;

    public void Register()
    {
        string email = emailField.text;
        string nickname = nicknameField.text;
        string password = passwordField.text;
        string confirmPassword = confirmPasswordField.text;
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
        
        if (string.IsNullOrWhiteSpace(nickname))
        {
            isError = true;
            nicknameError.gameObject.SetActive(true);
            nicknameError.text = "Nickname field is empty!";
        }
        else
        {
            nicknameError.gameObject.SetActive(false);

        }

        if (string.IsNullOrWhiteSpace(password))
        {
            isError = true;
            passwordError.gameObject.SetActive(true);
            passwordError.text = "Password field is empty!";
        }else
        {
            passwordError.gameObject.SetActive(false);
        }
        
        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            isError = true;
            confirmPasswordError.gameObject.SetActive(true);
            confirmPasswordError.text = "Confirm password field is empty!";
            
        }else if (!password.Equals(confirmPassword))
        {
            isError = true;
            confirmPasswordError.gameObject.SetActive(true);
            confirmPasswordError.text = "Confirm password does not match";
        }
        else
        {
            confirmPasswordError.gameObject.SetActive(false);
        }

        if(isError) return;
        _ = RegisterTask(email, nickname, password);
    }

    private async Task RegisterTask(string email, string nickname, string password)
    {
        RegisterRequest registerRequest = new RegisterRequest(email, nickname, password);
        
        UIMgr.Instance.ShowLoadingPanel();
        await ClientAPI.CallPost<RegisterResponse, RegisterRequest>(
            "/auth/register",
            registerRequest,
            OnSuccessfull: (response, httpResponse) => {

                for (int i = 0; i < response.RegisterStatuses.Count; i++)
                {
                    RegisterStatus status = response.RegisterStatuses[i];
                    
                    switch (status)
                    {
                        case RegisterStatus.EMAIL_EXIST:
                            emailError.gameObject.SetActive(true);
                            emailError.text = "Email is used!";
                            break;
                        case RegisterStatus.NICKNAME_USED:
                            nicknameError.gameObject.SetActive(true);
                            nicknameError.text = "Nickname is used!";
                            break;
                        case RegisterStatus.OK:
                            UIMgr.Instance.ActiveElement("LoginPanel");
                            emailField.text = "";
                            nicknameField.text = "";
                            passwordField.text = "";
                            confirmPasswordField.text = "";
                            loginPanel.SetEmail(email);
                            break;
                    }
                }
                UIMgr.Instance.HideLoadingPanel();

            },
            OnFailure: () => {
                Debug.LogError("Error");
                UIMgr.Instance.HideLoadingPanel();
            }
        );
    }
    
    public class RegisterRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("nickname")]
        public String Nickname { get; set; }
        
        [JsonProperty("password")]
        public string Password { get; set; }

        public RegisterRequest(string email, string nickname, string password)
        {
            Email = email;
            Nickname = nickname;
            Password = password;
        }
    }
    
    public class RegisterResponse{
        public List<RegisterStatus> RegisterStatuses { get; set; }

        public RegisterResponse(List<RegisterStatus> registerStatuses) {
            RegisterStatuses = registerStatuses;
        }
    }
    
}
