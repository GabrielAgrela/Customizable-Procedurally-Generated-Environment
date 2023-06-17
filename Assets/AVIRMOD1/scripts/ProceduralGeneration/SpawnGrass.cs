using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnGrass : MonoBehaviour
{
    public float XMin = -300.0f;
    public float XMax = 500.0f;
    public float ZMin = -200.0f;
    public float ZMax = 550.0f;
    public float SpawnHeight = 115.0f;
    public GameObject parent;
    public GameObject[] tree;
    public GameObject[] placedGrass;
    public int numObjectsToSpawn=0;


    public void SpawnObject(int chosen)
    {
        float spawnPointX = 0f;
        float spawnPointZ = 0f;
        placedGrass = new GameObject[numObjectsToSpawn];
        float pocketThreshold = 0.4f; // Adjust this value to control density
        float offsetX = Random.Range(-200, 500);
        float offsetZ = Random.Range(-200, 500);
        for (int i = 0; i < numObjectsToSpawn; i++)
        {
            spawnPointX = Random.Range(XMin + (255 * transform.GetComponent<MeshGenerator>().tileX), XMax + (255 * transform.GetComponent<MeshGenerator>().tileX));
            spawnPointZ = Random.Range(ZMin + (255 * transform.GetComponent<MeshGenerator>().tileZ), ZMax + (255 * transform.GetComponent<MeshGenerator>().tileZ));
            Vector3 spawnPosition = new Vector3(spawnPointX, SpawnHeight, spawnPointZ);

            // Use Perlin noise to create pockets
            float noiseValue = Mathf.PerlinNoise((spawnPosition.x + offsetX) * 0.01f, (spawnPosition.z + offsetZ) * 0.01f);

            if (noiseValue > pocketThreshold)
            {
                RaycastHit hit;
                if (Physics.Raycast(spawnPosition, Vector3.down, out hit))
                {
                    if (hit.transform.gameObject.tag != "water")
                        placedGrass[i] = Instantiate(tree[chosen], new Vector3 (hit.point.x, hit.point.y+2.0f,hit.point.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), parent.transform);
                }
            }
        }
    }
}
