<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <title>Customizable Procedurally Generated Environment Documentation</title>
    <style>
        body {
            font-family: Arial, sans-serif;
        }
        h1, h2 {
            color: #333;
        }
    </style>
</head>

<body>
    <h1>Customizable Procedurally Generated Environment</h1>
    <p>Watch our small demo <a href="https://youtu.be/niXPKbuWB7I">here</a>.</p>

    <h2>Mesh Generator</h2>
    <ul>
        <li><strong>Terrain Coloration:</strong> Attribute a color and a time to define when and what color that part of the mesh will be, in that normalized height. Note that the color is additive, not absolute.</li>
        <li>
            <strong>Terrain Generation Customization:</strong>
            <ul>
                <li><strong>Granularity:</strong> The higher the value, the more granular and "bumpy" the terrain will be.</li>
                <li><strong>Height:</strong> The average height of the terrain.</li>
                <li><strong>Octaves:</strong> The number of "layers" of Perlin noise. More layers equal more detailed terrain.</li>
                <li><strong>Persistence:</strong> Higher values result in the terrain being more affected by the octaves. Lower values generally yield smoother terrain.</li>
                <li><strong>Lacunarity:</strong> Affects the frequency of each octave. Higher values lead to more detailed but potentially "spiky" terrain.</li>
                <li><strong>Seed:</strong> The seed used to generate the terrain. Altering this will result in different terrain.</li>
                <li><strong>Random Seed:</strong> If selected, the seed will be randomized with each generation.</li>
            </ul>
        </li>
        <li>
            <strong>Path Customization:</strong>
            <ul>
                <li><strong>Path Gradients:</strong> Define colors for smoother transitions between the path and the terrain triangles' color.</li>
                <li><strong>Number of Waypoints:</strong> Randomly spawn between the start and end points. More waypoints make the path more "interesting".</li>
                <li><strong>Lateral Deviation:</strong> The higher the value, the further away the waypoint can potentially spawn laterally from the direct path.</li>
                <li><strong>Direct Distance:</strong> In the path generation process, the algorithm will look for spots in the shore for both start and end points. When an iteration is reached where the distance between the two points is less than the direct distance, the path will be generated.</li>
                <li><strong>Number of Minipaths:</strong> Once the path is finished, a number of minipaths will be generated. Mini paths start at a random point in the path and end at another random point on the map.</li>
            </ul>
        </li>
    </ul>

    <h2>Spawn Vegetation</h2>
    <p>Create new vegetation by clicking on the plus icon, and define the following:</p>
    <ul>
        <li><strong>Prefab:</strong> The prefab to spawn. It should have the same components as the examples.</li>
        <li><strong>Spawn:</strong> Determine whether to spawn it or not.</li>
        <li><strong>Num Objects:</strong> The number of objects to spawn (serves as an indicator of density, rather than a strict count).</li>
        <li><strong>Octaves:</strong> The number of "layers" of Perlin noise.</li>
        <li><strong>Persistence:</strong> The higher the value, the more the terrain will be affected by the octaves.</li>
        <li><strong>Pocket Threshold:</strong> The higher the value, the more likely the object will spawn in a pocket.</li>
    </ul>

    <h2>Path Creator</h2>
    <ul>
        <li><strong>Item Drop Rate:</strong> Determine how frequently an item will spawn.</li>
        <li><strong>Item to Drop:</strong> Specify which, if any, item to drop.</li>
    </ul>
</body>

</html>
