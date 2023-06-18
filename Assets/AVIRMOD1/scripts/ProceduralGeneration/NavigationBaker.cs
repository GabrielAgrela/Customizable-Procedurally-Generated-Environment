using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationBaker : MonoBehaviour {

    public NavMeshSurface[] surfaces;
    public Transform[] objectsToRotate;
    public GameObject PathMaker;
    public GameObject[] PathMakersSec;
    public GameObject TerrainMesh;

    // builds a mesh in wish the agent can walk and create its path later.
    public void buildNavMesh()
    {
        for (int i = 0; i < surfaces.Length; i++) 
        {
            surfaces [i].BuildNavMesh ();    
        }
        StartCoroutine(CheckIfGenerationValid());
    }
    IEnumerator CheckIfGenerationValid()
    {
        yield return new WaitForSeconds(4);
        if (PathMaker.GetComponent<PathCreator>().CheckIfFinalPathIsValid()) 
        {
            GameObject.Find("Terrain(Clone)").GetComponent<SpawnVegetation>().SpawnObject();
        }
        else
        {
            print("Path is not valid, trying again...");
            TerrainMesh.GetComponent<MeshGenerator>().seed += 124;
            TerrainMesh.GetComponent<MeshGenerator>().ColdStart();
        }
    }

}