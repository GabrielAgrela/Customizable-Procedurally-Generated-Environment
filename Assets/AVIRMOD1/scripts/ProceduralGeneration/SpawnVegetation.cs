using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Vegetation
{
    public GameObject prefab;
    public bool spawn;
    public int numObjectsToSpawn;
    public int octaves=4;
    public float persistence=0.5f;
    public float pocketThreshold = 0.4f;
}

public class SpawnVegetation : MonoBehaviour
{
    [HideInInspector]public float SpawnHeight = 500.0f;
    [HideInInspector]public MeshGenerator meshGenerated;
    public GameObject parent;
    public Vegetation[] vegetations;

     

    
    private float PerlinNoiseOctaves(float x, float y, int octaves, float persistence)
{
    float total = 0;
    float frequency = 1;
    float amplitude = 1;
    float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0

    for (int i = 0; i < octaves; i++)
    {
        total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

        maxValue += amplitude;
        amplitude *= persistence;
        frequency *= 2;
    }

    return total / maxValue;
}

    public void SpawnObject()
    {
        meshGenerated = GameObject.Find("Terrain(Clone)").GetComponent<MeshGenerator>();

        foreach (var vegetation in vegetations)
        {
            if (!vegetation.spawn)
                continue;
            float spawnPointX = 0f;
            float spawnPointZ = 0f;
            float offsetX = Random.Range(meshGenerated.minSize, meshGenerated.maxSize);
            float offsetZ = Random.Range(meshGenerated.minSize, meshGenerated.maxSize);

            for (int i = 0; i < vegetation.numObjectsToSpawn; i++)
            {
                spawnPointX = Random.Range(meshGenerated.minSize, meshGenerated.maxSize);
                spawnPointZ = Random.Range(meshGenerated.minSize, meshGenerated.maxSize);
                Vector3 spawnPosition = new Vector3(spawnPointX, SpawnHeight, spawnPointZ);

                float noiseValue = PerlinNoiseOctaves((spawnPosition.x + offsetX) * 0.01f, (spawnPosition.z + offsetZ) * 0.01f, vegetation.octaves, vegetation.persistence);

                if (Random.value < (noiseValue - vegetation.pocketThreshold))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(spawnPosition, Vector3.down, out hit, 500f))
                    {
                        if (hit.transform.gameObject.tag != "water")
                            Instantiate(vegetation.prefab, new Vector3 (hit.point.x, hit.point.y, hit.point.z), Quaternion.Euler(new Vector3(0, Random.Range(-36, 36), 0)), parent.transform);
                    }
                }
            }
        }
    }

}
