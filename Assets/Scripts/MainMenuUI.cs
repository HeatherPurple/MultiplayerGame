using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button joinLobbyBtn;
    [SerializeField] private Button listLobbiesBtn;
    [SerializeField] private Button startGameBtn;

    [SerializeField] private TextMeshProUGUI createdLobbyNameTextField;
    [SerializeField] private TextMeshProUGUI joinedLobbyNameTextField;

    private void Awake()
    {
        createLobbyBtn.onClick.AddListener(() => CreateLobby(createdLobbyNameTextField));
        joinLobbyBtn.onClick.AddListener(() => JoinLobby(joinedLobbyNameTextField));
        listLobbiesBtn.onClick.AddListener(ListLobbies);
        startGameBtn.onClick.AddListener(StartGame);
    }

    private void CreateLobby(TextMeshProUGUI textField)
    {
        LobbyManager.Singleton.CreateLobby(textField.text);
    }

    private void JoinLobby(TextMeshProUGUI textField)
    {
        LobbyManager.Singleton.JoinLobbyByName(textField.text);
    }

    private void StartGame()
    {
        LobbyManager.Singleton.StartGame();
    }

    private void ListLobbies()
    {
        
    }

    private void OnDisable()
    {
        createLobbyBtn.onClick.RemoveListener(() => CreateLobby(createdLobbyNameTextField));
        joinLobbyBtn.onClick.RemoveListener(() => JoinLobby(joinedLobbyNameTextField));
        listLobbiesBtn.onClick.RemoveListener(ListLobbies);
        startGameBtn.onClick.RemoveListener(StartGame);
    }
}
