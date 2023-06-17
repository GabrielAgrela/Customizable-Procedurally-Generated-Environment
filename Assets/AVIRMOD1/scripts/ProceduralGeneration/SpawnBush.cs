using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnBush : MonoBehaviour
{
    public GameObject parent;
    public GameObject[] tree;
    public GameObject[] placedTrees;
    public int numObjectsToSpawn=0;
    public bool destroy = true;
        public TMP_Text m_TextComponent;
    // Start is called before the first frame update
    void Start()
    {
    m_TextComponent.text = numObjectsToSpawn+"";
    //SpawnObject(0);
    //SpawnObject(1);
        
    }

    public void ToggleDestroy()
    {
        for (int i = 0; i< placedTrees.Length; i++)
        {
            Destroy(placedTrees[i]);
        }
    }

    public void SpawnObject(int chosen)
    {
        float spawnPointX =0f; 
        float spawnPointZ =0f; 
        placedTrees = new GameObject[numObjectsToSpawn];
        for (int i = 0; i< numObjectsToSpawn; i++)
        {
            spawnPointX=Random.Range(-300.0f+(255 * transform.GetComponent<MeshGenerator>().tileX), 500.0f+(255 * transform.GetComponent<MeshGenerator>().tileX));
            spawnPointZ=Random.Range(-200.0f+(255 * transform.GetComponent<MeshGenerator>().tileZ), 550.0f+(255 * transform.GetComponent<MeshGenerator>().tileZ));
            placedTrees[i]=Instantiate (tree[chosen], new Vector3(spawnPointX, 115, spawnPointZ), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)),parent.transform);
        }
    }

    public void AddCount()
    {
        numObjectsToSpawn = numObjectsToSpawn + 10;
        m_TextComponent.text = numObjectsToSpawn+"";
    }

    public void SubCount()
    {
        numObjectsToSpawn = numObjectsToSpawn - 10;
        m_TextComponent.text = numObjectsToSpawn+"";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
