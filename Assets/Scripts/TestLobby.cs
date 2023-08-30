using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TestLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logUGUI;
    
    private Lobby currentLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float heartbeatTimerMax = 10f;
    private float lobbyUpdateTimerMax = 1.1f;
    private bool IsInGame;

    private const string KeyStartGame = "StartGameCode";

    [SerializeField] private TestRelay testRelay;
    
    
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in with ID:" + AuthenticationService.Instance.PlayerId);
            Log("Signed in with ID:" + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (currentLobby is null) return;
        if (!IsLobbyHost()) return;

            heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer < 0)
        {
            heartbeatTimer = heartbeatTimerMax;
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
    }
    
    private async void HandleLobbyPollForUpdates()
    {
        if (IsInGame) return;
        if (currentLobby is null) return;
        
        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0)
        {
            lobbyUpdateTimer = lobbyUpdateTimerMax;
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
        }

        if (currentLobby.Data[KeyStartGame].Value != "0")
        {
            if (!IsLobbyHost())
            {
                testRelay.JoinRelayFromLobby(currentLobby.Data[KeyStartGame].Value);
                IsInGame = true;
            }
        }
    }

    public async void CreateLobby(TextMeshProUGUI text)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { KeyStartGame, new DataObject(DataObject.VisibilityOptions.Member,"0") }
                }
            };
            
            string lobbyName = text.text;
            int maxPlayers = Random.Range(2,6);
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,options);

            currentLobby = lobby;
        
            Debug.Log("Created lobby! " + lobby.Name + " " + lobby.MaxPlayers);
            Log("Created lobby! " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            Log(e.Message);
        }
        
        
    }
    
    public async void JoinLobbyByName(TextMeshProUGUI text)
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 1,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, text.text, QueryFilter.OpOptions.EQ),
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            if (queryResponse.Results.Count == 0)
            {
                Debug.Log("There's no lobby with that name!");
                Log("There's no lobby with that name!");
                return;
            }

            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            Debug.Log("Joined to the lobby! " + currentLobby.Name + " " + currentLobby.MaxPlayers);
            Log("Joined to the lobby! " + currentLobby.Name + " " + currentLobby.MaxPlayers);
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            Log(e.Message);
        }
    }

    public async void StartGame()
    {
        if(!IsLobbyHost()) return;
        
        string joinCode = await testRelay.CreateRelayFromLobby();
        currentLobby = await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions()
        {
            Data = new Dictionary<string, DataObject>()
            {
                { KeyStartGame, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
            }
        });
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            var counter = 1;

            foreach (var lobby in queryResponse.Results)
            {
                Debug.Log(counter +". " + lobby.Name + " " + lobby.Players.Count + "/" + lobby.MaxPlayers);
                Log(counter +". " + lobby.Name + " " + lobby.Players.Count + "/" + lobby.MaxPlayers);
                counter++;
            }
            if (counter == 1)
            {
               Debug.Log("No lobbies found!"); 
               Log("No lobbies found!"); 
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            Log(e.Message);
        }
    }

    private bool IsLobbyHost()
    {
        if (currentLobby is null) return false;

        return currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private void Log(string message)
    {
        logUGUI.text += "\n" + message;
    }


}
