using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RoomListScrollView : MonoBehaviour
{

    [SerializeField] private RoomItem roomItem;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject emptyInfo;
    [SerializeField] private Button reloadbutton;
    [SerializeField] private LoadingPanel loadingPanel;

    void Awake()
    {
        reloadbutton.onClick.AddListener(() =>
        {
            _ = LoadRoomsTask();
        });
    }

    private void OnEnable()
    {
        _ = LoadRoomsTask();
    }


    void Update()
    {
        
    }

    private void Clear()
    {
        foreach (Transform item in content)
        {
            Destroy(item.gameObject);
        }
    }

    public void LoadRooms(List<Room> rooms)
    {
        Clear();
        foreach (var room in rooms)
        { 
            RoomItem item = Instantiate(roomItem, content);
            item.Initialize(room);
        }
        emptyInfo.SetActive(rooms.Count == 0);
    }
    
    private async Task LoadRoomsTask()
    {
        emptyInfo.SetActive(false);
        loadingPanel.gameObject.SetActive(true);
        loadingPanel.Initialize();
        await ClientAPI.CallGet<List<Room>>(
            "/rooms",
            (rooms, response) => {
                loadingPanel.gameObject.SetActive(false);
                Debug.Log("ROOMS LOAD SUCC");
                LoadRooms(rooms);
            },
            OnFailure: () => {
                Debug.Log("ROOMS LOAD FAIL");
                loadingPanel.gameObject.SetActive(false);
            }
        );


    }
    
    
}

[SerializeField]
public class RoomsWrapper
{
    [JsonProperty("rooms")]
    private List<Room> Rooms { get; set; }
}
