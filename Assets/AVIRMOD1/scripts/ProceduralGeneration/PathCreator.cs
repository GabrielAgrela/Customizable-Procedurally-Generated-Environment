using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathCreator : MonoBehaviour
{

    [HideInInspector]public MeshGenerator meshGenerator;
    [HideInInspector]private List<Vector3> lastPositions = new List<Vector3>();
    [HideInInspector]public Vector3 pointPosition;
    [HideInInspector]public NavMeshAgent myNavMeshAgent;
    private float itemToDropTimer = 0f;
    public GameObject itemToDrop;
    public void Start()
    {
        meshGenerator = GameObject.Find("Terrain(Clone)").GetComponent<MeshGenerator>();
        myNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Destroy grass objects upon collision to keep the path clear
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("grass"))
        {
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("finish"))
        {
            OnFinish(collision);
        }
    }

    private void OnFinish(Collision collision)
    {
        Destroy(collision.gameObject);
        StartCoroutine(FinalPathPreparations(1));
    }
    // Clean up the scene after completion and spawn the final door
    private IEnumerator FinalPathPreparations(int secs)
    {
        yield return new WaitForSeconds(secs);

        CleanUpScene();
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
    
    // Update the path and related objects based on the agent's current state
    private void FixedUpdate()
    {
        // If the agent has reached a waypoint
        if (!myNavMeshAgent.pathPending && myNavMeshAgent.remainingDistance < 0.5f && meshGenerator.agentReady)
            GoToNextWaypoint();
        
        itemToDropTimer += Time.deltaTime;

        // Instantiate environment objects along the path
        DropItem(GetAgentAngle());
    }

    public void GoToNextWaypoint()
    {
        if (meshGenerator.waypoints.Count == 0) 
        return;
        myNavMeshAgent.SetDestination(meshGenerator.waypoints[0].transform.position);
        if (!CheckIfPathIsValid())
        {
            meshGenerator.waypoints.RemoveAt(0);
            GoToNextWaypoint();
        }
        else
        {
            meshGenerator.waypoints.RemoveAt(0);
        }
    }
    public bool CheckIfPathIsValid()
    {
        NavMeshPath path = new NavMeshPath();
        if (myNavMeshAgent.CalculatePath(meshGenerator.waypoints[0].transform.position, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
        }
        return false;
    }

    // Instantiate grass, rocks, and bushes along the path based on random probability
    private void DropItem(float AngleBetweenPathmakerPositions)
    {
        // Instantiate itemToDrop every 2 seconds
        if (itemToDropTimer >= 2f && meshGenerator.agentReady)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.Euler(new Vector3(0, -AngleBetweenPathmakerPositions, 0)));
            itemToDropTimer = 0f;
        }
    }
    // Update the agent's position and calculate the angle between its current and previous position
    private float GetAgentAngle()
    {
        pointPosition = transform.position;
        float AngleBetweenPathmakerPositions = UpdateAngleBetweenPositions();

        // Add the current position to the list
        lastPositions.Add(pointPosition);

        // If the list has more than 10 positions, remove the oldest one
        if (lastPositions.Count > 10)
        {
            lastPositions.RemoveAt(0);
        }
        return AngleBetweenPathmakerPositions;
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

    public bool CheckIfFinalPathIsValid()
    {
        NavMeshPath path = new NavMeshPath();
        if (myNavMeshAgent.CalculatePath(meshGenerator.waypoints[meshGenerator.waypoints.Count - 1].transform.position, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
        }
        return false;
    }
}

