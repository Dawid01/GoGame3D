using System;
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
        if (email.Length == 0)
        {
            isError = true;
            emailError.gameObject.SetActive(true);
            emailError.text = "Email field is Empty!";
        }

        if (!email.Contains("@") && !isError)
        {
            isError = true;
            emailError.gameObject.SetActive(true);
            emailError.text = "It's not email!";
        }

        if (password.Length == 0)
        {
            isError = true;
            passwordField.gameObject.SetActive(true);
            passwordError.text = "Password field is Empty!";
        }
    }
}
