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
        _ = CreateRoomTask(name);
    }
    
    private async Task CreateRoomTask(string name)
    {
        CreateRoomRequest createRoomRequest = new CreateRoomRequest(NetworkMgr.Instance.currentPlayer.sessionId, name);
        UIMgr.Instance.ShowLoadingPanel();
        await ClientAPI.CallPost<string, CreateRoomRequest>(
            "/rooms/create",
            createRoomRequest,
            OnSuccessfull: (user, httpResponse) =>
            {
                if (httpResponse.IsSuccessStatusCode)
                {
                    nameField.text = "";
                }

                UIMgr.Instance.HideLoadingPanel();

            },
            OnFailure: () => {
                UIMgr.Instance.HideLoadingPanel();
            }
        );

    }
}
