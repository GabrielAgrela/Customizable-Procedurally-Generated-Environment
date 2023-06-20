using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniPathCreator : MonoBehaviour
{

    [HideInInspector]public MeshGenerator meshGenerator;
    [HideInInspector]private List<Vector3> lastPositions = new List<Vector3>();
    [HideInInspector]public Vector3 pointPosition;
    [HideInInspector]public NavMeshAgent myNavMeshAgent;
    [HideInInspector]public GameObject ItemsDroppedParent;
    private GameObject placedDestination;
    public List<Vector3> pathCoordinates = new List<Vector3>();

    public float itemToDropRate;
    private float itemToDropTimer=0f;
    private float saveCoordinateTimer = 0f;
    public GameObject itemToDrop;
    public GameObject Scout;
    public GameObject Destination;

    public int minPathDistance;
    public int maxPathDistance;

    
    public void Start()
    {
        meshGenerator = GameObject.Find("Terrain(Clone)").GetComponent<MeshGenerator>();
        myNavMeshAgent = GetComponent<NavMeshAgent>();
        ItemsDroppedParent = GameObject.Find("ItemsDropped");
        PlaceDestination();
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
            Destroy(collision.gameObject);
            StartCoroutine(FinalPathPreparations(1));
            
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
        
        
        meshGenerator.NotifyFinish(transform.GetChild(0).transform.GetChild(0).gameObject);
        gameObject.SetActive(false);
    }
    
    // Update the path and related objects based on the agent's current state
    private void FixedUpdate()
    {       
        itemToDropTimer += Time.deltaTime;
        saveCoordinateTimer += Time.deltaTime;
        if (saveCoordinateTimer >= 2f && meshGenerator.agentReady)
        {
            pathCoordinates.Add(transform.position);
            saveCoordinateTimer = 0f;
        }
        // Instantiate environment objects along the path
        DropItem(GetAgentAngle());
        /*if (myNavMeshAgent.remainingDistance <= 5f)
        {
            StartCoroutine(FinalPathPreparations(1));
        }*/
    }

    public void DrawPath()
    {
        myNavMeshAgent.SetDestination(placedDestination.transform.position);
    }
    public bool CheckIfPathIsValid()
    {
        NavMeshPath path = new NavMeshPath();
        if (myNavMeshAgent.CalculatePath(Destination.transform.position, path))
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
        if (itemToDropTimer >= itemToDropRate && meshGenerator.agentReady)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.Euler(new Vector3(0, -AngleBetweenPathmakerPositions, 0)), ItemsDroppedParent.transform);
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

    public void PlaceDestination()
{
    GameObject ScoutPlaced;
    var pathMaker = GameObject.Find("PathMaker");
    if (pathMaker == null)
    {
        Debug.LogError("No object named PathMaker found");
        return;
    }

    var pathCreator = pathMaker.GetComponent<PathCreator>();
    if (pathCreator == null)
    {
        Debug.LogError("No PathCreator component found in PathMaker");
        return;
    }
    placedDestination = Instantiate(Destination, new Vector3(0,0,0), Quaternion.identity);

    List<Vector3> pathCoordinates = pathCreator.pathCoordinates;
    int maxIterations = 1000;
    int currentIteration = 0;

    while (currentIteration++ < maxIterations)
    {
        int x = Random.Range(meshGenerator.minSize, meshGenerator.maxSize);
        int z = Random.Range(meshGenerator.minSize, meshGenerator.maxSize);

        ScoutPlaced = Instantiate(Scout, new Vector3(x, 300, z), Quaternion.identity);
        ScoutPlaced.GetComponent<ScoutTerrain>().checkIsInWater();

        if (ScoutPlaced.GetComponent<ScoutTerrain>().isInWater == false)
        {
            Vector3 proposedPosition = new Vector3(x, ScoutPlaced.GetComponent<ScoutTerrain>().yPos, z);
            float closestDistance = float.MaxValue;
            Vector3 closestCoordinate = Vector3.zero;

            foreach (var coordinate in pathCoordinates)
            {
                float distance = Vector3.Distance(proposedPosition, coordinate);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCoordinate = coordinate;
                }
            }

            bool validPath = false;
            NavMeshPath path = new NavMeshPath();
            minPathDistance++;
            maxPathDistance--;
            if (closestDistance > minPathDistance && closestDistance< maxPathDistance)
            {
                if (myNavMeshAgent.CalculatePath(closestCoordinate, path))
                {
                    validPath = true;
                }
            }

            if (validPath)
            {
                GetComponent<NavMeshAgent>().enabled = false;
                transform.position = closestCoordinate;
                placedDestination.transform.position = proposedPosition;
                GetComponent<NavMeshAgent>().enabled = true;
                print("closestDistance: " + closestDistance+ "currentIteration " + currentIteration);
                print("Calculatepath "+myNavMeshAgent.CalculatePath(closestCoordinate, path));
                Destroy(ScoutPlaced);
                DrawPath();
                return;
            }
        }
        Destroy(ScoutPlaced);
    }
    Debug.LogWarning("Failed to place destination after " + maxIterations + " attempts");
}




}

