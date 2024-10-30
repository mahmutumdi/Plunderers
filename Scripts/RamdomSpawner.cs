using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;

public class RandomSpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] prefabsToSpawn;
    public Vector3 areaSize;
    public float avoidRadius = 1.0f;
    public int targetSpawnCount = 25;
    public Color gizmoColor = new Color(0, 1, 0, 0.3f);

    private Bounds spawnBounds;
    private Collider[] collidersBuffer = new Collider[10];
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isSpawning = true;

    private void Start()
    {
        spawnBounds = new Bounds(transform.position, areaSize);
        
        // Only spawn objects if this client is the master client
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < targetSpawnCount; i++)
            {
                SpawnRandomObject();
            }
        }
    }

    private void Update()
    {
        if (!isSpawning || !PhotonNetwork.IsMasterClient) return;

        spawnedObjects.RemoveAll(item => item == null);
        while (spawnedObjects.Count < targetSpawnCount)
        {
            SpawnRandomObject();
        }
    }

    private void SpawnRandomObject()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;

        while (!validPosition && attempts < 10)
        {
            spawnPosition = new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                0f,
                Random.Range(-areaSize.z / 2, areaSize.z / 2)
            ) + transform.position;

            if (spawnBounds.Contains(spawnPosition) && IsPositionValid(spawnPosition))
            {
                validPosition = true;
            }
            attempts++;
        }

        if (validPosition)
        {
            GameObject prefabToSpawn = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
            GameObject newObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", prefabToSpawn.name), spawnPosition, Quaternion.identity);
            
            newObject.transform.SetParent(transform);
            spawnedObjects.Add(newObject);
        }
        else
        {
            Debug.Log("No valid spawn position found after 10 attempts.");
        }
    }

    private bool IsPositionValid(Vector3 position)
    {
        int hits = Physics.OverlapSphereNonAlloc(position, avoidRadius, collidersBuffer);
        return hits == 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, areaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }

    public override void OnLeftRoom()
    {
        isSpawning = false;
        ClearSpawnedObjects();
    }

    private void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) PhotonNetwork.Destroy(obj);
        }
        spawnedObjects.Clear();
    }
}
