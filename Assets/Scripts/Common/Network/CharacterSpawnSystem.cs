using Fusion;
using UnityEngine;

public class CharacterSpawnSystem : SingletonMonoBehaviour<CharacterSpawnSystem>
{
    [SerializeField] private Vector3 initialSpawnPosition;
    [SerializeField] private GameObject characterPrefab;

    public GameObject myCharacter;

    public void SpawnPlayer(PlayerRef player)
    {
        if (player == MainServer.ActiveRunner.LocalPlayer && myCharacter == null)
        {
            var currentCharacter = MainServer.ActiveRunner.Spawn(characterPrefab, initialSpawnPosition, Quaternion.identity, player);
            MainServer.ActiveRunner.SetPlayerObject(player, currentCharacter);

            myCharacter = currentCharacter.gameObject;
        }
    }

    public void DespawnPlayer(PlayerRef player)
    {
        if (player == MainServer.ActiveRunner.LocalPlayer)
        {
            var playerObject = MainServer.ActiveRunner.GetPlayerObject(player);
            
            if (playerObject != null)
            {
                MainServer.ActiveRunner.Despawn(playerObject);
            }
        }
    }
}