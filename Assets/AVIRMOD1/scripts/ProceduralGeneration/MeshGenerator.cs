using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    
    
    // Mesh and its components
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private Color[] colors;
    int[] triangles;
    Gradient gradient;

    // Mesh Size (in vertices)
    int xSize = 150;
    int zSize = 150;

    // idfk
    private float offSet;

    
    [HideInInspector] public Vector3 finishPosition;

    [HideInInspector] public bool agentReady = false; 
    // Terrain Bounds
    [HideInInspector]
    public int minSize = 0;
    [HideInInspector]
    public int maxSize = 0;

    [Tooltip("Save the whole generation into a prefab (will load for a couple of minutes at the end)")]
    public bool saveGeneration = false;

    // Terrain Customization
    [Header("Terrain Color Customization")]

    // Define the gradient color spectrum for the terrain
    [SerializeField] private Color shoreColor;
    [SerializeField] private float shoreTime = 0.0f;
    [SerializeField] private Color fieldColor;
    [SerializeField] private float fieldTime = 0.1f;
    [SerializeField] private Color hillColor;
    [SerializeField] private float hillTime = 0.4f;
    [SerializeField] private Color PeakColor;
    [SerializeField] private float peakTime = 0.8f;

    [Header("Terrain Generation Customization")]
    [Tooltip("Terrain Granularity")]
    public float granularity = 0.014f;

    [Tooltip("Terrain Height")]
    public float Height = 29.56f;

    [Tooltip("Number of layers of noise to create a Pseudo-Fractal effect")]
    public int octaves = 9; 

    [Tooltip("Controls the amplitude of each octave. Lower values lead to smoother terrain, while higher values make the terrain more rugged or bumpy.")]
    public float persistence = 0.47f;

    [Tooltip("Affects the frequency of each octave. Higher values lead to more small-scale details in the terrain, like a craggy mountainside.")]
    public float lacunarity = 2.13f;

    [Tooltip("Changes offsets' value to deterministically generate a new terrain")]
    public int seed = 1;
    public bool randomSeed;

    [Tooltip("Spawnable Bridges")]
    public GameObject[] Bridges;

    // Path Customization
    [Header("_____________________________________________________________")]
    [Header("Path Customization")]

    [SerializeField] private Color pathGradientInside;
    [SerializeField] private Color pathGradientMiddle;
    [SerializeField] private Color pathGradientOutside;

    [Tooltip("How many waypoints to generate")]
    public int numberOfWaypoints;
    [Tooltip("Straight distance between inicial pathmaker position and final waypoint")]
    public int directDistance;
    [Tooltip("How much of a deviation from the straight path is allowed on the waypoints")]
    public float lateralDeviation;
    public GameObject waypointPrefab;
    [Tooltip("Waypoints spawned in between the inicial and final waypoints")]
    public List<GameObject> waypoints = new List<GameObject>();

    public int numMiniPaths;
    private int numMiniPathsFinished = 0;

    // Terrain Helpers
    [Header("_____________________________________________________________")]
    [Header("Path and Helpers")]

    
    public GameObject Scout;
    public GameObject PathDestination;
    public GameObject PathMaker;
    public GameObject Finish;


    void Start()
    {
        if (randomSeed)
        {
            seed = Random.Range(0, 1000000);
        }
        
        MakeGradient();
        ColdStart();
    }

    public void CleanGeneration()
    {
        GameObject Temporary = GameObject.Find("Temporary");
        GameObject TrailPaintersParent = GameObject.Find("TrailPainters");
        // Destroy all the children gameobjects of the temporary gameobject
        foreach (Transform temporaryChild in Temporary.transform)
        {
            foreach (Transform child in temporaryChild)
            {
                Destroy(child.gameObject);
            }
        }
        waypoints.Clear();
        PathMaker.GetComponent<PathCreator>().pathCoordinates.Clear();
        foreach (Transform child in TrailPaintersParent.transform)
        {
            child.gameObject.GetComponent<RayCastHitVertex>().hitTriangles.Clear();
        }
    }

    public void MakeGradient()
    {
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;

        // Populate the color keys at the relative times
        colorKey = new GradientColorKey[4];
        colorKey[0].color = shoreColor;
        colorKey[0].time = shoreTime;
        colorKey[1].color = fieldColor;
        colorKey[1].time = fieldTime;
        colorKey[2].color = hillColor;
        colorKey[2].time = hillTime;
        colorKey[3].color = PeakColor;
        colorKey[3].time = peakTime;

        // Populate the alpha keys at the relative times of 0 and 1
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient = new Gradient();
        gradient.SetKeys(colorKey, alphaKey);
    }
    public void ColdStart()
    {
        CleanGeneration();
        print("Cleanead Generation");
        UpdateMap();
        print("Mesh generated");
        CleanFinish();
        print("Painters Cleaned");
        //SpawnRaycasts();
        PlacePathMarks();
        print("Paths Placed");
        GenerateWaypoints();
        transform.parent.transform.parent.gameObject.GetComponent<NavigationBaker>().buildNavMesh();
        // clear PathMaker.GetComponent<NavMeshAgent>() destination
    }
    public void UpdateMap()
    {
        seedToOffsets();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }
    // extra layer of calibration of the pseudorandom terrain
    void seedToOffsets()
    {
        offSet=seed/2;
    }
    // Creates the mesh for the terrain
    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colors = new Color[vertices.Length];
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                // Calculate the distance to the closest edge
                float edgeDistance = Mathf.Min(x, xSize - x, z, zSize - z) / (float)xSize;
                float edgeFactor =  Mathf.SmoothStep(0f, 1f, edgeDistance);
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int o = 0; o < octaves; o++)
                {
                    var temp = Mathf.PerlinNoise((offSet + x * granularity) * frequency, (offSet + z * granularity) * frequency);
                    noiseHeight += temp * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                float y = Mathf.Pow(noiseHeight, 2f) * Height * edgeFactor;
                // Record the minimum and maximum height
                minHeight = Mathf.Min(minHeight, y);
                maxHeight = Mathf.Max(maxHeight, y);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        // Assign colors based on height
        for (int i = 0; i < vertices.Length; i++)
        {
            float normalizedHeight = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
            colors[i] = gradient.Evaluate(normalizedHeight);
        }
        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }
        uvs = new Vector2[vertices. Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x/ xSize, (float) z / zSize);
                    i++;
            }
        }
        mesh.colors = colors;
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        PrintMeshBounds();
    }
    void PrintMeshBounds()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        Vector3 min = vertices[0];
        Vector3 max = vertices[0];

        for (int i = 1; i < vertices.Length; i++)
        {
            min = Vector3.Min(min, vertices[i]);
            max = Vector3.Max(max, vertices[i]);
        }
        // To transform the coordinates to world space:
        min = transform.TransformPoint(min);
        max = transform.TransformPoint(max);
        minSize = Mathf.RoundToInt(min[0]);
        maxSize = Mathf.RoundToInt(max[0]);
    }

    public void CleanFinish()
    {
        GameObject[] beacons;
        beacons = GameObject.FindGameObjectsWithTag("FinishPrimary");
        foreach (GameObject beacon in beacons)
        {
            Destroy(beacon);
        }
    }

    public void PlacePathMarks()
    {
        PathMaker.GetComponent<NavMeshAgent>().enabled = false;
        while(true)
        {
            directDistance--;
            PlacePathBeginning();
            PlacePathEnd();
            GameObject MarksParent = GameObject.Find("Marks");
            if (Vector3.Distance(PathMaker.transform.position, finishPosition) > directDistance)
            {
                PathDestination.transform.position = finishPosition;
                Instantiate(Finish, finishPosition, Quaternion.identity, MarksParent.transform);
                PathMaker.GetComponent<NavMeshAgent>().enabled = true;
                return;
            }
        }

    }
    public void PlacePathBeginning()
    {
        while(true)
        {
            GameObject ScoutPlaced;
            ScoutPlaced = Instantiate (Scout, new Vector3(500, 300, 500), Quaternion.identity);


            int x = Random.Range(maxSize, minSize);
            int z = Random.Range(maxSize, minSize);

            ScoutPlaced.transform.position = new Vector3(x, 300f, z);
            ScoutPlaced.GetComponent<ScoutTerrain>().checkIsInWater();

            if (ScoutPlaced.GetComponent<ScoutTerrain>().isInWater == false)
            {
                bool isSuitablePlace = CheckSurroundingArea(x, z, ScoutPlaced);
                if (isSuitablePlace)
                {
                    PathMaker.transform.position = new Vector3(x, ScoutPlaced.GetComponent<ScoutTerrain>().yPos, z);
                    Destroy(ScoutPlaced);
                    return;
                }
            }
            Destroy(ScoutPlaced);
        }
    }   
    public void PlacePathEnd()
    {
        GameObject ScoutPlaced;
        
        ScoutPlaced = Instantiate (Scout, new Vector3(minSize, 100, minSize), Quaternion.identity);
        
        while(true)
        {
            int x = Random.Range(maxSize, minSize);
            int z = Random.Range(maxSize, minSize);

            ScoutPlaced.transform.position = new Vector3(x, 300, z);
            ScoutPlaced.GetComponent<ScoutTerrain>().checkIsInWater();

            if (ScoutPlaced.GetComponent<ScoutTerrain>().isInWater == false)
            {
                bool isSuitablePlace = CheckSurroundingArea(x, z, ScoutPlaced);
                if (isSuitablePlace)
                {
                    // Proposed position for the finish
                    finishPosition = new Vector3(x, ScoutPlaced.GetComponent<ScoutTerrain>().yPos, z);
                    Destroy(ScoutPlaced);
                    return;
                    // Check if the distance between PathMaker and Finish is more than 100
                    
                }
            }
            Destroy(ScoutPlaced);
        }
        
    }
    private bool CheckSurroundingArea(int x, int z, GameObject scout)
    {
        // Array of four directions: right, left, forward, and backward
        Vector3[] directions = new Vector3[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        foreach (var direction in directions)
        {
            // Check land within 10 units
            scout.transform.position = new Vector3(x + direction.x * 10, 400f, z + direction.z * 10);
            scout.GetComponent<ScoutTerrain>().checkIsInWater();
            if (scout.GetComponent<ScoutTerrain>().isInWater == false)
            {
                // Check water within 20 units
                scout.transform.position = new Vector3(x + direction.x * 20, 400f, z + direction.z * 20);
                scout.GetComponent<ScoutTerrain>().checkIsInWater();
                if (scout.GetComponent<ScoutTerrain>().isInWater == true)
                {
                    return true; // Suitable position found
                }
            }
        }
        return false; // No suitable position found
    }
    void GenerateWaypoints()
    {
        GameObject waypointsParent= GameObject.Find("Waypoints");
        Vector3 startPosition = PathMaker.transform.position;
        Vector3 destinationPosition = PathDestination.transform.position;
        float maxTerrainHeight = 500.0f; // Set this to the maximum possible height of your terrain features

        Vector3 direction = (destinationPosition - startPosition).normalized;
        Vector3 rightVector = Vector3.Cross(direction, Vector3.up);

        for (int i = 0; i < numberOfWaypoints; i++)
        {
            float t = (float)(i + 1) / (numberOfWaypoints + 1); // Calculate interpolation factor

            // Interpolate a position along the line from start to destination
            Vector3 waypoint = Vector3.Lerp(startPosition, destinationPosition, t);

            // Add some lateral deviation to make the path less straight
            float lateralOffset = Random.Range(-lateralDeviation, lateralDeviation);
            waypoint += lateralOffset * rightVector;

            // Cast a ray downwards from the waypoint
            RaycastHit hit;
            if (Physics.Raycast(waypoint + Vector3.up * maxTerrainHeight, Vector3.down, out hit, maxTerrainHeight * 2))
            {
                // If the ray hit the ground, move the waypoint to the hit point
                waypoint = hit.point;
            }

                // Instantiate a cube or another prefab at the waypoint for debugging
                waypoints.Add(Instantiate(waypointPrefab, waypoint, Quaternion.identity, waypointsParent.transform));

                // Add the waypoint
            // waypoints.Add(waypoint);
        }

        // Add the final destination as the last waypoint
        waypoints.Add(PathDestination);

        // Instantiate a cube or another prefab at the final destination for debugging
        Instantiate(waypointPrefab, destinationPosition, Quaternion.identity, waypointsParent.transform);
    }
    // look for a suitable place to place the bridge
    // each bridge has scouts on the edges, telling me if it's hitting water or land,
    // if the middle scouts are hitting water, and the extremities hitting land, that means its a suitable place to place the bridge
    // each loop makes the bridge: longer, rotates it, changes y pos, changes z pos.
    public void SpawnRaycasts()
    {
        foreach (GameObject bridge in Bridges)
        {
            bool isBridgePlaced = false;

            for (int widthIndex = 0; widthIndex <= 10; widthIndex++)
            {
                if (isBridgePlaced) continue;

                bridge.transform.localScale -= new Vector3(0, 0, 1);

                for (int yRotation = 0; yRotation <= 360; yRotation += 20)
                {
                    if (isBridgePlaced) continue;

                    bridge.transform.Rotate(0.0f, (float)yRotation, 0.0f, Space.Self);

                    for (float xPos = maxSize; xPos > minSize; xPos -= 10)
                    {
                        if (isBridgePlaced) continue;

                        for (float zPos = maxSize; zPos > minSize; zPos -= 10)
                        {
                            bridge.transform.position = new Vector3(xPos, 200f, zPos);

                            if (!bridge.GetComponent<BridgeMain>().checkPlacable()) continue;

                            bridge.transform.position = new Vector3(xPos, bridge.GetComponent<BridgeMain>().YPosCheck.GetComponent<ScoutTerrain>().yPos - 1.5f, zPos);
                            Debug.Log("Bridge place at "+ widthIndex + " " + yRotation + " " + xPos + " " + zPos);
                            isBridgePlaced = true;
                            break;
                        }
                    }
                }
            }

            if (!isBridgePlaced)
            {
                bridge.SetActive(false);
                Debug.Log("Bridge did not place");
            }
        }
    }
    public void SaveMeshAsAsset()
    {
        #if UNITY_EDITOR
        string resourcesPath = "Assets/Resources/";
        string path = "Mesh" + seed + ".asset";
        AssetDatabase.CreateAsset(mesh, resourcesPath + path);
        AssetDatabase.SaveAssets();
        print("saved");
        mesh = Resources.Load<Mesh>("Mesh"+seed);
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        print("loaded");
        GetComponent<MeshCollider>().sharedMesh = mesh;
        #endif
    }

    public void PaintHitTriangles(GameObject painter, bool isMiniPath=false)
    {
        // Check if we have a mesh to paint
        if (mesh == null)
        {
            Debug.LogWarning("No mesh set to paint!");
            return;
        }

        // Check if mesh has colors array, if not, initialize it
        Color[] colors = mesh.colors;
        if (colors.Length == 0)
        {
            colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
        }

        // Brown color
        

        List<Triangle> hitTriangles = painter.GetComponent<RayCastHitVertex>().hitTriangles;
        HashSet<Triangle> hitTrianglesSet = new HashSet<Triangle>(hitTriangles);
        HashSet<Triangle> firstSurroundingTrianglesSet = new HashSet<Triangle>();
        HashSet<Triangle> secondSurroundingTrianglesSet = new HashSet<Triangle>();

        // Get all triangles
        List<Triangle> allTriangles = new List<Triangle>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            allTriangles.Add(new Triangle(triangles[i], triangles[i + 1], triangles[i + 2]));
        }

        // Get first surrounding triangles
        foreach (Triangle triangle in allTriangles)
        {
            if (!hitTrianglesSet.Contains(triangle))
            {
                foreach (Triangle hitTriangle in hitTriangles)
                {
                    // Check if triangle and hitTriangle share an edge
                    if (TrianglesShareEdge(triangle, hitTriangle))
                    {
                        // Add to the first surrounding triangles set
                        firstSurroundingTrianglesSet.Add(triangle);

                        // Skip to the next triangle as we've already added this one
                        break;
                    }
                }
            }
        }

        if (!isMiniPath)
        {
            // Get second surrounding triangles
            foreach (Triangle triangle in allTriangles)
            {
                if (!hitTrianglesSet.Contains(triangle) && !firstSurroundingTrianglesSet.Contains(triangle))
                {
                    foreach (Triangle surroundingTriangle in firstSurroundingTrianglesSet)
                    {
                        // Check if triangle and surroundingTriangle share an edge
                        if (TrianglesShareEdge(triangle, surroundingTriangle))
                        {
                            // Add to the second surrounding triangles set
                            secondSurroundingTrianglesSet.Add(triangle);

                            // Skip to the next triangle as we've already added this one
                            break;
                        }
                    }
                }
            }

            // Color second surrounding triangles even lighter brown
            foreach (Triangle triangle in secondSurroundingTrianglesSet)
            {
                colors[triangle.index1] = pathGradientOutside;
                colors[triangle.index2] = pathGradientOutside;
                colors[triangle.index3] = pathGradientOutside;
            }
        }
        

        // Color first surrounding triangles lighter brown
        foreach (Triangle triangle in firstSurroundingTrianglesSet)
        {
            colors[triangle.index1] = pathGradientMiddle;
            colors[triangle.index2] = pathGradientMiddle;
            colors[triangle.index3] = pathGradientMiddle;
        }

        // Then color the hit triangles brown
        foreach (Triangle triangle in hitTriangles)
        {
            colors[triangle.index1] = pathGradientInside;
            colors[triangle.index2] = pathGradientInside;
            colors[triangle.index3] = pathGradientInside;
        }

        // Assign the new colors to the mesh
        mesh.colors = colors;
    }
    private bool TrianglesShareEdge(Triangle triangle1, Triangle triangle2)
    {
        // Count the vertices that triangle1 and triangle2 have in common
        int commonVertices = 0;
        if (triangle1.index1 == triangle2.index1 || triangle1.index1 == triangle2.index2 || triangle1.index1 == triangle2.index3) commonVertices++;
        if (triangle1.index2 == triangle2.index1 || triangle1.index2 == triangle2.index2 || triangle1.index2 == triangle2.index3) commonVertices++;
        if (triangle1.index3 == triangle2.index1 || triangle1.index3 == triangle2.index2 || triangle1.index3 == triangle2.index3) commonVertices++;

        // If they have two vertices in common, they share an edge
        return commonVertices == 2;
    }

    public void TerrainFinishes()
    {
        GameObject[] painters;
        painters = GameObject.FindGameObjectsWithTag("PathPainter");
        foreach (GameObject painter in painters)
        {
            GameObject.Find("Terrain(Clone)").GetComponent<MeshGenerator>().PaintHitTriangles(painter);
        }
       SaveMeshAsAsset();

        #if UNITY_EDITOR
            SaveGeneration();
        #endif

        PathMaker.SetActive(false);
    }
    #if UNITY_EDITOR
    public void SaveGeneration()
    {
        if (saveGeneration)
        {
            // find gameobject terrain
            GameObject terrain = GameObject.Find("Terrain(Clone)");
            CreatePrefabFromGameObject("Assets/Generations/"+seed+".prefab");
        }
        
    }
    #endif
    #if UNITY_EDITOR
    private void CreatePrefabFromGameObject(string path)
    {
        GameObject parentPrefab = new GameObject("ParentPrefab");

        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
        foreach(GameObject obj in allObjects)
        {
            if (obj.transform.parent == null) // avoid attaching child objects directly to the new parent
            {
                obj.transform.SetParent(parentPrefab.transform);
            }
        }
        
        PrefabUtility.SaveAsPrefabAsset(parentPrefab, path);
    }
    #endif

    public void NotifyFinish(GameObject painter)
    {
        
        numMiniPathsFinished++;
        PaintHitTriangles(painter,true);
        if (numMiniPathsFinished == numMiniPaths)
        {
            TerrainFinishes();
        }
    }
}