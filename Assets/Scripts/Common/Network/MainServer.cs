#nullable enable

using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using UnityEngine;

public class MainServer : SingletonMonoBehaviour<MainServer>
{
    public static NetworkRunner? ActiveRunner;

    [SerializeField] private GameObject networkRunnerPrefab;
    [SerializeField] private Recorder recorder;
    private NetworkRunner? networkRunner;
    private FusionVoiceClient? voiceClient;
    private bool connecting = false;

    private void Start()
    {
        StartServer(null);
    }

    public async void StartVoiceServer(Photon.Realtime.AuthenticationValues? authentication)
    {
        await Task.Delay(1000);
        voiceClient!.Client.AuthValues = authentication;
        if (!voiceClient.ConnectAndJoinRoom())
        {
            await Task.Delay(5000);
            if (!Application.isPlaying) return;
            StartVoiceServer(null);
        }
    }

    public async Task StartServer(AuthenticationValues? authentication)
    {
        if (connecting) return;

        networkRunner = Instantiate(networkRunnerPrefab).GetComponent<NetworkRunner>();
        voiceClient = networkRunner.GetComponent<FusionVoiceClient>();
        voiceClient.PrimaryRecorder = recorder;

        connecting = true;
        var result = await networkRunner!.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>(),
            AuthValues = authentication
        });

        connecting = false;
        if (result.Ok)
        {
            ActiveRunner = networkRunner;
#if !UNITY_EDITOR
            StartVoiceServer(null);
#endif
            await Task.Delay(1000);
        }
        else
        {
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }
}
