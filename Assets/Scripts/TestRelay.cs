using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logUGUI;

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
    

    public async void CreateRelay()
    {
        try
        {
            int maxPlayers = 4;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Created relay! Join code is " + relayJoinCode);
            Log("Created relay! Join code is " + relayJoinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            Log(e.Message);
        }
        
        
    }
    
    public async void JoinRelay(TextMeshProUGUI text)
    {
        try
        {
            string joinCode = text.text;
            await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("Joined to the relay with " + joinCode);
            Log("Joined to the relay with " + joinCode);
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
