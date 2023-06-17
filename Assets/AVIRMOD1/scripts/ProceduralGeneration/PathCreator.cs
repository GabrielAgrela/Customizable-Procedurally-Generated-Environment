using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathCreator : MonoBehaviour
{

    [Header("General fields")]
    public GameObject finalDoor;

    public GameObject anchor;

    [Header("Agent and navigation fields")]
    private List<Vector3> lastPositions = new List<Vector3>();
    public Vector3 pointPosition;
    public Vector3 pointPositionLate;
    public NavMeshAgent myNavMeshAgent;
    public GameObject destination;

    [Header("Milestones and environment objects")]
    public GameObject grass;
    public GameObject rock;
    public GameObject bush;
    public GameObject grassParent;
    public GameObject rockParent;
    public GameObject bushParent;

    [Header("State and timing variables")]
    public int x = 0;
    public int milestoneIncrementCounter;
    public bool finished = false;
    public float TimeT;
    public float TimeTotal;
    private float anchorTimer = 0f;
    

    // Destroy grass objects upon collision to keep the path clear
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("grass"))
        {
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("FinishPrimary"))
        {
            OnFinishPrimary(collision);
        }
    }

    private void OnFinishPrimary(Collision collision)
    {
        finished = true;
        Destroy(collision.gameObject);
        StartCoroutine(FinalPathPreparations(1));
        TimeTotal = 0;
    }

    public void DrawPath()
    {
        myNavMeshAgent.SetDestination(destination.transform.position);
        if (myNavMeshAgent.path.corners.Length < 2)
        {
            return;
        }
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform foundChild = FindChildRecursive(child, targetName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null;
    }

    // Clean up the scene after completion and spawn the final door
    private IEnumerator FinalPathPreparations(int secs)
    {
        yield return new WaitForSeconds(secs);

        CleanUpScene();
        SpawnFinalDoor();
        GameObject.Find("Terrain(Clone)").GetComponent<MeshGenerator>().TerrainFinishes();
        gameObject.SetActive(false);
    }

    private void CleanUpScene()
    {
        foreach (GameObject grass in GameObject.FindGameObjectsWithTag("grass"))
        {
            Destroy(grass.GetComponent<Collider>());
            Destroy(grass.GetComponent<Rigidbody>());
        }
        //GameObject.Find("BridgeRock").transform.position = new Vector3(GameObject.Find("BridgeRock").transform.position.x, GameObject.Find("BridgeRock").transform.position.y + 1.0f, GameObject.Find("BridgeRock").transform.position.z);
    }

    private void SpawnFinalDoor()
    {
        GameObject finalDoorTemp = Instantiate(finalDoor, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
        finalDoorTemp.GetComponent<DoorColliderDetection>().SpawnPos = new Vector3(-50f, 15f, -35f);
    }


    private void FixedUpdate()
    {
        // Update the total time elapsed
        UpdateTime();

        // Check if the agent should draw the path and update accordingly
        if (ShouldDrawPath())
        {
            UpdatePath();
        }
    }

    private void UpdateTime()
    {
        TimeTotal += Time.deltaTime;
    }

    // Determine if the agent should draw the path based on its navigation mesh and completion status
    private bool ShouldDrawPath()
    {
        return myNavMeshAgent.hasPath && !finished;
    }

    // Update the path and related objects based on the agent's current state

    public int counterTest;
    private void UpdatePath()
    {
        TimeT += Time.deltaTime;
        anchorTimer += Time.deltaTime;
        var randomRotation = GetRandomRotation();

        UpdateLineRenderer();
        float AngleBetweenPathmakerPositions = UpdateAgentPosition();

        // Instantiate environment objects along the path
        PlaceEnvironmentObjects(randomRotation, AngleBetweenPathmakerPositions);
    }

    // Generate a random rotation for instantiated objects
    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
    }

    // Update the line renderer to account for the agent's new position
    private void UpdateLineRenderer()
    {
        DrawPath();
    }

    // Update the agent's position and calculate the angle between its current and previous position
    private float UpdateAgentPosition()
    {
        pointPosition = transform.position;
        float AngleBetweenPathmakerPositions = UpdateAngleBetweenPositions();
        pointPositionLate = pointPosition;

        // Add the current position to the list
        lastPositions.Add(pointPosition);

        // If the list has more than 10 positions, remove the oldest one
        if (lastPositions.Count > 10)
        {
            lastPositions.RemoveAt(0);
        }

        x++;
        milestoneIncrementCounter++;

        return AngleBetweenPathmakerPositions;
    }

    public bool CheckIfPathIsValid()
    {
        NavMeshPath path = new NavMeshPath();
        if (myNavMeshAgent.CalculatePath(destination.transform.position, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
        }
        return false;
    }

    // Calculate the angle between the agent's current position and its previous position
    private float UpdateAngleBetweenPositions()
    {
        // If there are not enough positions in the list, return 0
        if (lastPositions.Count < 10)
        {
            return 0;
        }

        Vector3 positionFrom10IterationsBefore = lastPositions[0];

        return Mathf.Atan2(positionFrom10IterationsBefore.z - pointPosition.z, positionFrom10IterationsBefore.x - pointPosition.x) * 180 / Mathf.PI;
    }

    // Instantiate grass, rocks, and bushes along the path based on random probability
    private void PlaceEnvironmentObjects(Quaternion randomRotation, float AngleBetweenPathmakerPositions)
    {
        int randm = Random.Range(0, 41);

        // Instantiate anchor every 2 seconds
        if (anchorTimer >= 2f)
        {
            Instantiate(anchor, transform.position, Quaternion.Euler(new Vector3(0, -AngleBetweenPathmakerPositions, 0)));
            anchorTimer = 0f;
        }
        if (randm <= 30)
        {
            bool isPositive = randm <= 15;
            //Instantiate(grass, CalculateNewPosition(isPositive, AngleBetweenPathmakerPositions, 8.0f, 8.5f), randomRotation, grassParent.transform);
        }
        else if (randm >= 37)
        {
            GameObject objectToInstantiate = (randm == 38 || randm == 37) ? bush : rock;
            bool isPositive = randm == 38 || randm == 40;
            //Instantiate(objectToInstantiate, CalculateNewPosition(isPositive, AngleBetweenPathmakerPositions, 8.0f, 8.5f), randomRotation, (randm == 38 || randm == 37) ? bushParent.transform : rockParent.transform);
        }
    }

    // Calculate the new position for an instantiated object based on the angle between the agent's positions
    private Vector3 CalculateNewPosition(bool isPositive, float AngleBetweenPathmakerPositions, float minRange, float maxRange)
    {
        float randomRange = Random.Range(minRange, maxRange) / Mathf.Cos((AngleBetweenPathmakerPositions * Mathf.PI) / 180);
        float zPosition = transform.position.z + (isPositive ? randomRange : -randomRange);
        return new Vector3(transform.position.x, transform.position.y + 10, zPosition);
    }
}

