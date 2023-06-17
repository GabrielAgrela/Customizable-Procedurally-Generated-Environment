using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Vegetation
{
    public GameObject prefab;
    public bool spawn;
    public int numObjectsToSpawn;
}

public class SpawnVegetation : MonoBehaviour
{
    public float SpawnHeight = 500.0f;
    public GameObject parent;
    public Vegetation[] vegetations;

    public MeshGenerator meshGenerated;

    public void SpawnObject()
    {
        meshGenerated = GameObject.Find("Terrain(Clone)").GetComponent<MeshGenerator>();
        foreach (var vegetation in vegetations)
        {
            float spawnPointX = 0f;
            float spawnPointZ = 0f;
            float pocketThreshold = 0.6f; // Adjust this value to control density
            float offsetX = Random.Range(meshGenerated.minSize, meshGenerated.maxSize);
            float offsetZ = Random.Range(meshGenerated.minSize, meshGenerated.maxSize);
            for (int i = 0; i < vegetation.numObjectsToSpawn; i++)
            {
                spawnPointX = Random.Range(meshGenerated.minSize + (150 * transform.GetComponent<MeshGenerator>().tileX), meshGenerated.maxSize + (150 * transform.GetComponent<MeshGenerator>().tileX));
                spawnPointZ = Random.Range(meshGenerated.minSize + (150 * transform.GetComponent<MeshGenerator>().tileZ), meshGenerated.maxSize + (150 * transform.GetComponent<MeshGenerator>().tileZ));
                Vector3 spawnPosition = new Vector3(spawnPointX, SpawnHeight, spawnPointZ);

                // Use Perlin noise to create pockets
                float noiseValue = Mathf.PerlinNoise((spawnPosition.x + offsetX) * 0.01f, (spawnPosition.z + offsetZ) * 0.01f);

                if (noiseValue > pocketThreshold)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(spawnPosition, Vector3.down, out hit, 500f))
                    {
                        if (hit.transform.gameObject.tag != "water")
                            Instantiate(vegetation.prefab, new Vector3 (hit.point.x, hit.point.y,hit.point.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), parent.transform);
                    }
                }
            }
        }
        
    }
}
