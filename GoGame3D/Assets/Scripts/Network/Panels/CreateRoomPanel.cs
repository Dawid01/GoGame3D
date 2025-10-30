using System;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class CreateRoomPanel : MonoBehaviour
{

    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TextMeshProUGUI nameError;
    [SerializeField] private TextMeshProUGUI characterLimit;

    private void Awake()
    {
        Clear();
        nameField.onValueChanged.AddListener((value) =>
        {
            characterLimit.text = $"{value.Length}/{nameField.characterLimit}";
        });
       
    }

    public void CreateRoom()
    {
        if(!ClientAPI.IsLogged) return;
        string name = nameField.text;
        bool isError = false;

        if (string.IsNullOrWhiteSpace(name))
        {
            isError = true;
            nameError.gameObject.SetActive(true);
            nameError.text = "Name field is empty!";
        }
        
        if(isError) return;
        RoomsMgr.Instance.CreateRoom(name);
    }

    private void Clear()
    {
        nameField.text = "";
        nameError.text = "";
        characterLimit.text = $"0/{nameField.characterLimit}";
    }
    
    private void OnDisable()
    {
        Clear();
    }
    
}
