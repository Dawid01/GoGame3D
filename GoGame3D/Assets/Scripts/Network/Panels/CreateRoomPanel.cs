using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class CreateRoomPanel : MonoBehaviour
{

    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TextMeshProUGUI nameError;
    
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
    
}
