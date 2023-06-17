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

    private Mesh mesh;

    private Vector3[] vertices;
    private Vector2[] uvs;
    private Color[] colors; // Add this line
    int[] triangles;

    public int octaves = 4; 
    public float persistence = 0.5f; 
    public float lacunarity = 2.0f; 

    public int minSize = 0;
    public int maxSize = 0;


    int xSize = 150;
    int zSize = 150;

    float noiseRange=3.57f;

    [Header("Tile Coordinates")]
    public int tileX=0;
    public int tileZ=0;


    [Header("Axis Noise Density")]
    public float XModifier = 0.014f;
    public float ZModifier = 0.014f;

    [Header("Noise Amplitude")]
    public float Height = .3f;

    [Header("Axis Noise Offsets")]
    public float offSetX = .3f;
    public float offSetZ = .3f;

    [Header("Randomizes offsets' value")]
    public int seed = 0;

    [Header("Spawnable Objects")]
    public GameObject[] Bridges;

    [Header("Path and Helpers")]
    public GameObject Scout;
    public GameObject PathDestination;
    public GameObject PathMaker;
    public GameObject Finish;

    Gradient gradient;

    private bool lockBeg = false;

    void Start()
    {
        Color brown = new Color(0.65f, 0.16f, 0.16f);
        Color yellow = Color.yellow;

        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;

        // Populate the color keys at the relative height of 0, 0.25, 0.5, and 1
        colorKey = new GradientColorKey[4];
        colorKey[0].color = yellow;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.white;
        colorKey[1].time = 0.1f;
        colorKey[2].color = brown;
        colorKey[2].time = 0.4f;
        colorKey[3].color = Color.black;
        colorKey[3].time = .8f;

        // Populate the alpha keys at the relative height of 0 and 1
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient = new Gradient();
        gradient.SetKeys(colorKey, alphaKey);



        ColdStart();
    }

    public void ColdStart()
    {
        lockBeg=false;
        UpdateMap();
        print("Mesh generated");
        CleanFinish();
        print("Painters Cleaned");
        // Spawn bridges
        //SpawnRaycasts();
        
        //StartCoroutine(SpawnRaycasts());
        // Ducktape where I spawn grass and place markers for the pathmaker agent
        StartCoroutine(putGrass());
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
    // makes it so i can "slide" through the "noise map" of the pseudorandom function
    void seedToOffsets()
    {
        offSetX=seed/2;
        offSetZ=seed/2;
        offSetX=offSetX+tileX*noiseRange;
        offSetZ=offSetZ+tileZ*noiseRange;
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
                float edgeFactor = Mathf.SmoothStep(0f, 1.5f, edgeDistance);
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int o = 0; o < octaves; o++)
                {
                    var temp = Mathf.PerlinNoise((offSetX + x * XModifier) * frequency, (offSetZ + z * ZModifier) * frequency);
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
    mesh.colors = colors; // Make sure to assign colors after vertices
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();
    
    mesh.RecalculateBounds();
    MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
    meshCollider.sharedMesh = mesh;
    PrintMeshBounds();
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


    IEnumerator putGrass()
    {
        // spawn type of grass 3
        
        yield return new WaitForSeconds(3);

        //THE ducktape
        FitaCola();
    }

    // after defining the path markers, the navmesh can be build. so the agent can create the path
    public void FitaCola()
    {
        PlacePathBeginning();
        print("Paths Placed");
        //StartCoroutine(WaitMainPath());
        //PlacePathEnd();
        
        transform.parent.transform.parent.gameObject.GetComponent<NavigationBaker>().buildNavMesh();
        
        
    }

    // start searching from the other corner of the scene for land using my scout (scout is a cube with a vertical downraycasts that tell me if it's hitting water or terrain)
    // if he finds terrain, place the agent there.
    int edgeOffset = 5;



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
    

    public void PaintHitTriangles(GameObject painter)
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
    Color brown = new Color(0.64f, 0.16f, 0.16f);
    Color lighterBrown = new Color(0.78f, 0.35f, 0.35f); // Lighter shade of brown
    Color evenLighterBrown = new Color(0.86f, 0.58f, 0.58f); // Even lighter shade of brown

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
        colors[triangle.index1] = evenLighterBrown;
        colors[triangle.index2] = evenLighterBrown;
        colors[triangle.index3] = evenLighterBrown;
    }

    // Color first surrounding triangles lighter brown
    foreach (Triangle triangle in firstSurroundingTrianglesSet)
    {
        colors[triangle.index1] = lighterBrown;
        colors[triangle.index2] = lighterBrown;
        colors[triangle.index3] = lighterBrown;
    }

    // Then color the hit triangles brown
    foreach (Triangle triangle in hitTriangles)
    {
        colors[triangle.index1] = brown;
        colors[triangle.index2] = brown;
        colors[triangle.index3] = brown;
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




    public void CleanFinish()
    {
        GameObject[] beacons;
        beacons = GameObject.FindGameObjectsWithTag("FinishPrimary");
        foreach (GameObject beacon in beacons)
        {
            Destroy(beacon);
        }

        
        /*foreach (GameObject painter in painters)
        {
            StartCoroutine(RemoveAndAddComponent(painter));
        }*/
    }

    IEnumerator RemoveAndAddComponent(GameObject painter)
    {
        RayCastHitVertex component = painter.GetComponent<RayCastHitVertex>();

        if (component != null)
        {
            // Destroy the component
            Destroy(component);
        }

        // Wait until next frame to add the component back
        yield return new WaitForSeconds(5);

        RayCastHitVertex newComponent = painter.AddComponent<RayCastHitVertex>();

        // Set the firstTime and mesh to default
        newComponent.firstTime = true;
        newComponent.mesh = null;
        newComponent.mask = 1 << LayerMask.NameToLayer("Terrain");

    }
    
    IEnumerator WaitMainPath()
    {
        yield return new WaitForSeconds(15);
    }

    public void PlacePathBeginning()
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
            if (isSuitablePlace && lockBeg==false)
            {
                
                PathMaker.transform.position = new Vector3(x, ScoutPlaced.GetComponent<ScoutTerrain>().yPos, z);
                Destroy(ScoutPlaced);
                PlacePathEnd();
                return;
            }
        }
            Destroy(ScoutPlaced);
            PlacePathBeginning();

        
}

    // start searching from one corner of the scene for land using my scout (scout is a cube with a vertical downraycasts that tell me if it's hitting water or terrain)
    // if he finds terrain, place marker there.
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
                ScoutPlaced.transform.position = new Vector3(x, 300, z);
                ScoutPlaced.GetComponent<ScoutTerrain>().checkIsInWater();
                // Proposed position for the finish
                Vector3 finishPosition = new Vector3(x, ScoutPlaced.GetComponent<ScoutTerrain>().yPos, z);
                // Check if the distance between PathMaker and Finish is more than 100
                if (Vector3.Distance(PathMaker.transform.position, finishPosition) > 1000)
                {
                    lockBeg = true;
                    PathDestination.transform.position = finishPosition;
                    Instantiate(Finish, finishPosition, Quaternion.identity);
                    return;
                }
                else
                {
                    Destroy(ScoutPlaced);
                    PlacePathBeginning();
                    return;
                }
            }
        }
    }
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
}