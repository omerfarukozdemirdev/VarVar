using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField lobbyRoomCapInputField;
    [SerializeField] private Toggle isPrivateToggle;

    private bool isPrivate=false;
    private int roomCapacity = 4;
    private string roomName;
    
    private void Start()
    {
        string playerName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, "Unknown");
        roomName = playerName + "'s Room";
    }

    public void OnCreateLobbyButtonClicked()
    {
        createButton.interactable = false;
        closeButton.interactable = false;
        CreateLobby();
    }

    private async void CreateLobby()
    {
        await HostSingleton.Instance.HostManager.StartHostAsync(roomName, roomCapacity, isPrivate);
    }

    public void OnToggleValueChanged(bool toggleValue)
    {
        isPrivate = toggleValue;
    }

    public void OnRoomCapacityValueChanged(string value)
    {
        int parsedValue = int.TryParse(value, out int number) ? number : 4;
        int clampedCapacity = Mathf.Clamp(parsedValue, 4, 6);
        roomCapacity = clampedCapacity;
        lobbyRoomCapInputField.text = roomCapacity.ToString();
    }

    public void OnRoomNameValueChanged(string value)
    {
        roomName = value;
    }
}
