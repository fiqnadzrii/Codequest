using UnityEngine;

public class PlayerSpawnLocator : MonoBehaviour
{
    void Start()
    {
        // 1. Check if we have a pending spawn location from the SceneTeleporter
        if (!string.IsNullOrEmpty(SceneTeleporter.PendingSpawnPointName))
        {
            // 2. Find the spawn point object by name in the new scene
            GameObject spawnPoint = GameObject.Find(SceneTeleporter.PendingSpawnPointName);
            
            if (spawnPoint != null)
            {
                Debug.Log($"[SpawnLocator] Teleporting Player to '{spawnPoint.name}'");

                // --- CRITICAL FOR CHARACTER CONTROLLERS ---
                // If you use a CharacterController, you MUST disable it briefly to move 
                // the transform directly, otherwise the CharacterController overrides it.
                CharacterController cc = GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // 3. Move the Player
                transform.position = spawnPoint.transform.position;
                transform.rotation = spawnPoint.transform.rotation;

                // Re-enable CharacterController
                if (cc != null) cc.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[SpawnLocator] Could not find Spawn Point named '{SceneTeleporter.PendingSpawnPointName}' in this scene!");
            }
            
            // 4. Clear the memory so if we reload this scene normally, we don't teleport randomly
            SceneTeleporter.PendingSpawnPointName = null; 
        }
    }
}