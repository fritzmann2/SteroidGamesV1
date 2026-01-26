using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        // 1. Verbindung zu den Unity Services herstellen
        await UnityServices.InitializeAsync();

        // 2. Anonym anmelden (Nötig, um Relay zu nutzen)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // HOST: Erstellt ein Relay und gibt den Join Code zurück
    public async Task<string> CreateRelay()
    {
        try
        {
            // Reserviere Platz für 3 Spieler (Host + 3 Clients = 4 Total)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            // Hol den Join Code
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Richte den NetworkManager so ein, dass er Relay benutzt
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Starte den Host (wie früher, aber jetzt über Relay)
            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    // CLIENT: Tritt einem Relay per Code bei
    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            // Versuche dem Relay beizutreten
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Transport einrichten
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Client starten
            NetworkManager.Singleton.StartClient();
            
            // Wenn wir bis hier kommen, hat alles geklappt!
            return true; 
        }
        catch (System.Exception e)
        {
            Debug.LogError("Fehler beim Beitreten: " + e);
            // Wenn ein Fehler passiert (z.B. falscher Code), geben wir 'false' zurück
            return false; 
        }
    }
}