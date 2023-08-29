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
    }

    private async void HandleLobbyHeartbeat()
    {
        if (currentLobby is null) return;
        
        float heartbeatTimerMax = 10f;
        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer < 0)
        {
            heartbeatTimer = heartbeatTimerMax;
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
    }

    public async void CreateLobby(TextMeshProUGUI text)
    {
        try
        {
            string lobbyName = text.text;
            int maxPlayers = Random.Range(2,6);
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers);

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

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            Debug.Log("Joined to the lobby! " + lobby.Name + " " + lobby.MaxPlayers);
            Log("Joined to the lobby! " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            Log(e.Message);
        }
    }
    
    // public async void LeaveLobby()
    // {
    //     try
    //     {
    //         await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
    //     }
    //     catch (LobbyServiceException e)
    //     {
    //         Debug.Log(e);
    //     }
    // }
    
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

    private void Log(string message)
    {
        logUGUI.text += "\n" + message;
    }


}
