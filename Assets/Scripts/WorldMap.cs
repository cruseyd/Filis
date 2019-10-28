using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : Map
{

    public List<Node> regionBounds;
    public List<Node> worldBounds;
    public List<Vertex> riverStarts;
    public List<Region> regions;

    public int worldRadius = 2;
    public int regionRadius = 5;
    public float maxCompression = 0.0f;
    public float minCompression = 0.0f;
    public float maxRainfall = 0.0f;
    public float minRainfall = 0.0f;
    public float seaLevel = 0.0f;
    public float compressiveForceFalloff = 0.5f;
    public float tensileForceFalloff = 0.5f;
    public float rainFalloff = 0.5f;
    public float elevationSampleRate = 0.1f;
    public float elevationSampleAmplitude = 0.1f;
    public float riverMinSpawnElevation = 0.7f;
    public float riverSpawnRate = 0.1f;
    public float lakeSpawnRate = 0.1f;
    public float erosionModifier = 0.7f;
    public float erosionConstant = 0.15f;

    private bool showTopography = false;

    private int xOffset;
    private int zOffset;

    void Awake()
    {
        graph = new HexGraph(2 * regionRadius * worldRadius + regionRadius);
        regions = new List<Region>();
        regionBounds = new List<Node>();
        worldBounds = new List<Node>();
        riverStarts = new List<Vertex>();

        Camera.main.GetComponent<CameraFollow>().maxSize 
            = (2 * worldRadius + 1) * regionRadius * Mathf.Sqrt(2.0f / 3);
        //Camera.main.orthographicSize  = (2 * worldRadius + 1) * regionRadius * Mathf.Sqrt(2.0f/3);
    }

    void Start()
    {
        Generate();
        SpawnTiles();
        ShowElevation();
    }

    public override void Generate()
    {
        AssignRegions();
        
        FindWorldBoundaries();
        SimulateTectonics();
        ComputeElevation();
        SpawnRivers();
        RandomizeElevation();
        SimulateErosion();
        FindOceans();
        SimulateRainfall();
        SimulateTemperature();
        
    }

    public void AssignRegions()
    {
        Node n0 = graph.Center();
        n0.visited = true;
        n0.distance = 0;
        Region r0 = new Region(n0);
        regions.Add(r0);

        Queue<Node> process = new Queue<Node>();
        process.Enqueue(n0);

        List<HexCoords> offsets = n0.coords.neighbors();
        List<HexCoords> nCoords = n0.coords.neighbors();
        for (int ii = 0; ii < 6; ii++)
        {
            nCoords[ii] = (nCoords[ii] + offsets[(ii + 5) % 6])*regionRadius;
        }

        while (process.Count > 0)
        {
            Node n = process.Dequeue();
            if (n.distance < worldRadius)
            {
                for (int ii = 0; ii < 6; ii++)
                {
                    HexCoords regionCoords = n.coords + nCoords[ii];
                  
                    Node node = graph.nodes[regionCoords];
                    if (node != null && !node.visited)
                    {
                        node.visited = true;
                        node.distance = n.distance + 1;
                        regions.Add(new Region(node));
                        process.Enqueue(node);
                    }
                }
            }
        }

        foreach (Region r in regions)
        {
            graph.resetNodes();
            VisitData args = new VisitData();
            args.region = r;
            r.center.LocalVisit(AddToRegion, args, r.center.coords, regionRadius);
        }
        graph.resetNodes();

        // cull extra nodes
        List<Node> cullNodes =
            new List<Node>();
        foreach (Node node in graph.nodes.Values)
        {
            if (node.region1 == null)
            {
                cullNodes.Add(node);
            }
        }
        foreach (Node node in cullNodes)
        {
            graph.removeNode(node);
        }

        // set world boundaries
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.numNeighbors() < 6)
                {
                    worldBounds.Add(n);
                    if (n.numRegions() == 1)
                    {
                        regionBounds.Add(n);
                    }
                }
            }
        );
        graph.resetNodes();
    }

    public void FindWorldBoundaries()
    {
        graph.Center().borders[0].Visit(
            (e) => {
                if (e.external())
                {
                    e.worldBound = true;
                }
            }
        );
    }

    public override void SpawnTiles()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                HexTile.Spawn(n, this.transform, hexPrefab,
                    Position(n.coords));
            }
        );
    }

    public void SimulateTectonics()
    {
        graph.resetNodes();

        graph.Center().Visit(
            (n) => {
                n.compression = 0.0f;
            }
        );

        foreach (Node node in regionBounds)
        {
            float force = 0.0f;
            Vector2 dir = Position(node.coords - node.region1.center.coords).normalized;
            force += Vector2.Dot(node.region1.tectonicForce, dir);
            if (node.region2 != null)
            {
                dir = Position(node.coords - node.region2.center.coords).normalized;
                force += Vector2.Dot(node.region2.tectonicForce, dir);
                if (node.region3 != null)
                {
                    dir = Position(node.coords - node.region3.center.coords).normalized;
                    force += Vector2.Dot(node.region3.tectonicForce, dir);
                }
                else
                {
                    force *= 2;
                }
            }
            else
            {
                force *= 3;
            }

            graph.resetNodes();
            VisitData data = new VisitData();
            data.value = force;
            data.origin = node.coords;
            node.LocalVisit(
                (n, args) => {
                    HexCoords diff = n.coords - data.origin;
                    if (args.value > 0)
                    {
                        n.compression += args.value
                            * Mathf.Pow(compressiveForceFalloff, Mathf.Max(diff.radius() - 1, 0));
                    }
                    else
                    {
                        n.compression += args.value
                            * Mathf.Pow(tensileForceFalloff, diff.radius());
                    }
                }, data, node.coords, regionRadius + Random.Range(-2, 1));
        }
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                maxCompression = Mathf.Max(n.compression, maxCompression);
                minCompression = Mathf.Min(n.compression, minCompression);
            }
        );
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.compression > 0)
                {
                    n.compression /= maxCompression;
                }
                else
                {
                    n.compression /= (-minCompression);
                }
                n.compression -= seaLevel;
                if (n.compression > 0)
                {
                    n.compression /= (1 - seaLevel);
                }
                else
                {
                    n.compression /= (1 + seaLevel);
                }
                Debug.Assert(n.compression >= -1 && n.compression <= 1);
            }
        );

    }

    public void ComputeElevation()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                n.elevation = n.compression;
            }
        );
    }

    public void RandomizeElevation()
    {
        graph.resetNodes();
        InitSampling();

        graph.Center().Visit(
            (n) => {
                float sample = getSample(Position(n.coords), elevationSampleRate);
                n.elevation += n.elevation * sample * elevationSampleAmplitude;
                //if (n.seaLevel())
                //{
                n.elevation -= Mathf.Abs(sample / 20.0f);
                //}
                n.elevation = Mathf.Min(n.elevation, 1);
                n.elevation = Mathf.Max(n.elevation, -1);
            }
        );

    }

    public void SimulateErosion()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.river())
                {
                    n.elevation *= erosionModifier;
                    n.elevation -= erosionConstant;
                }
            }
        );
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.elevation < 0)
                {
                    foreach (Edge e in n.borders)
                    {
                        if (e != null) { e.river = false; }
                    }
                    foreach (Vertex v in n.corners)
                    {
                        v.riverEnd = false;
                    }
                }
            }
        );
    }

    public void SpawnRivers()
    {
        graph.resetVertices();
        graph.Center().corners[0].Visit(
            (v) => {
                int n = 0;
                for (int ii = 0; ii < 3; ii++)
                {
                    if (v.touches[ii] != null)
                    {
                        v.elevation += v.touches[ii].elevation;
                        n++;
                    }
                }
                v.elevation /= n;

                if (n == 3 && v.elevation > riverMinSpawnElevation)
                {
                    if (Random.value < riverSpawnRate)
                    {
                        for (int ii = 0; ii < 3; ii++)
                        {
                            if (v.neighbors[ii].riverStart) { return; } //no adjacent river seeds
                        }
                        v.riverStart = true;
                        riverStarts.Add(v);
                    }
                }
            }
        );
        foreach (Vertex v in riverStarts)
        {
            SpawnRiver(v);
        }

    }

    public void SpawnRiver(Vertex v)
    {
        bool spawnLake = true;
        Edge edge = v.randomDownhill();
        if (edge != null && !edge.river) { edge.river = true; }
        else
        {
            v.riverStart = false;
            return;
        }
        Vertex next = edge.downhill();

        bool done = false;
        while (!done)
        {
            if (next.coastal() || next.external()) { done = true; }
            if (done)
            {
                next.riverEnd = true;
            }
            else if (Random.value < lakeSpawnRate && spawnLake)
            {
                SpawnLake(next);
                spawnLake = false;
            }
            else
            {
                edge = next.randomDownhill();
                if (edge == null)
                {
                    SpawnLake(next);
                    done = true;
                    next.riverEnd = true;
                }
                else if (edge.river)
                {
                    done = true;
                }
                else
                {
                    edge.river = true;
                    next = edge.downhill();
                }
            }
        }
    }

    public void SpawnLake(Vertex v)
    {
        v.riverEnd = true;
        Node lakeNode = null;
        foreach (Node n in v.touches)
        {
            if (n != null && !n.river()) { lakeNode = n; }
        }
        if (lakeNode == null)
        {
            v.riverEnd = false;
            SpawnRiver(v);
            return;
        }
        lakeNode.water = true;

        bool done = false;
        while (!done)
        {
            if (Random.value < 0.25)
            {
                Vertex vertex = lakeNode.downhillCorner();
                if (vertex != null)
                {
                    foreach (Node n in vertex.touches)
                    {
                        if (n != null && !n.water && !n.river() && n.elevation > 0)
                        {
                            lakeNode = n;
                            lakeNode.water = true;
                            break;
                        }
                        else
                        {
                            done = true;
                        }
                    }
                }
            }
            else
            {
                done = true;
            }
        }

        if (!lakeNode.downhillCorner().seaLevel())
        {
            SpawnRiver(lakeNode.downhillCorner());
        }
    }

    public void FindOceans()
    {

        foreach (Node n in worldBounds)
        {
            if (n.elevation < 0)
            {
                n.ocean = true;
                n.water = true;
            }
        }

        foreach (Node n in worldBounds)
        {
            if (n.elevation < 0)
            {
                graph.resetNodes();
                n.Visit(
                    (node) => {
                        bool flag = false;
                        foreach (Node neighbor in node.neighbors)
                        {
                            if (neighbor != null && neighbor.ocean)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag && node.elevation < 0)
                        {
                            node.ocean = true;
                            node.water = true;
                        }
                        else if (flag && node.water) //deal with "coastal" lakes
                        {
                            node.elevation = -node.elevation;
                            node.ocean = true;
                        }
                    }
                );
            }
        }
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.elevation < 0 && !n.ocean)
                {
                    n.elevation /= 10;
                    n.swamp = true;
                }
            }
        );
    }

    public void SimulateRainfall()
    {
        foreach (Node node in graph.nodes.Values)
        {
            if (node.ocean)
            {
                continue;
            }
            graph.resetNodes();
            float rain = 0.0f;
            if (node.seaLevel())
            {
                rain += 0.5f;
            }
            if (node.river())
            {
                rain += 0.5f;
            }
            if (node.water)
            {
                rain += 1.0f;
            }
            if (node.swamp)
            {
                rain += 1.0f;
            }
            VisitData data = new VisitData();
            data.origin = node.coords;
            data.value = rain;
            node.LocalVisit(
                (n, args) => {
                    int dist = (n.coords - args.origin).radius();
                    n.rainfall += args.value
                        * Mathf.Pow(rainFalloff, Mathf.Max(dist - 1, 0))
                        * (1.0f - n.elevation / 4);
                    if (n.rainfall > maxRainfall)
                    {
                        maxRainfall = n.rainfall;
                    }
                }, data, node.coords, 2 * regionRadius
            );
        }
        foreach (Node n in graph.nodes.Values)
        {
            n.rainfall /= maxRainfall;
        }

    }

    public void SimulateTemperature()
    {
        int worldHeight = (2 * worldRadius + 1) * regionRadius * 2;
        foreach (Node n in graph.nodes.Values)
        {
            float latitude = -(n.coords.a - worldHeight / 2.0f);
            n.temperature = latitude / worldHeight;
            Debug.Assert(n.temperature >= 0);
            Debug.Assert(n.temperature <= 1);
            if (n.elevation > 0)
            {
                n.temperature -= n.elevation / 2;
            }
        }
    }

    public void AddToRegion(Node node, VisitData args)
    {
        if (node.region1 == null)
        {
            node.region1 = args.region;
            return;
        }
        else if (node.region2 == null)
        {
            node.region2 = args.region;
            regionBounds.Add(node);
            return;
        }
        else if (node.region3 == null)
        {
            node.region3 = args.region;
            return;
        }
        else
        {
            print("Tried to add a 4th region to node " + node.name);
            print("When adding region centered at " + args.region.center.name);
        }
    }

    public void ShowCompression()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.compression > 0)
                {
                    n.tile.highlight(new Color(n.compression, 0, 0));
                }
                else
                {
                    n.tile.highlight(new Color(0, 0, -n.compression));
                }
            }
        );
        graph.resetNodes();
        //ShowForces();
    }

    public void ShowElevation()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                float e = n.elevation;
                n.tile.highlight(new Color(1, 1, 1));
                if (e > 0)
                {
                    n.tile.highlight(new Color(0.3f + e, 1 - e, 0));
                }
                else
                {
                    n.tile.highlight(new Color(0, 0.7f + e, 1));
                }
                if (n.water)
                {
                    n.tile.highlight(new Color(0, 0, 1));
                }
                if (n.swamp)
                {
                    n.tile.highlight(new Color(0, 0.1f - n.elevation, 0));
                }
            }
        );
    }

    public void ShowRegions()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (n.numRegions() == 2 || n.numNeighbors() != 6)
                {
                    n.tile.highlight(new Color(0.1f, 0.1f, 0.1f));
                }
                if (n.numRegions() == 3)
                {
                    n.tile.highlight(Color.black);
                }
            }
        );
    }

    public void ShowForces()
    {
        foreach (Region r in regions)
        {
            /*
			Debug.DrawLine(r.center.position,
				r.center.position + r.tectonicForce,
				Color.magenta,9999);
               */
        }
    }

    public void ShowRainfall()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                n.tile.highlight(new Color(
                    Mathf.Pow(1.0f - n.rainfall, 3),
                    Mathf.Pow(1.0f - n.rainfall, 2),
                    Mathf.Pow(n.rainfall, 0.5f)));
                /*
				if (n.rainfall > 0.9f)
				{
					n.tile.highlight(Color.red);
					return;
				} else if (n.rainfall > 0.7f){
					n.tile.highlight(new Color(1, 0.5f, 0));
					return;
				} else if (n.rainfall > 0.5f){
					n.tile.highlight(new Color(1, 1, 0));
					return;
				} else if (n.rainfall > 0.3f){
					n.tile.highlight(new Color(0, 0.5f, 0));
					return;
				} else {
					n.tile.highlight(new Color(0,0,0.5f));
					return;
				}
				*/
            }
        );
    }
    public void ShowTemperature()
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                n.tile.highlight(new Color(n.temperature, 0.3f, 1.0f - n.temperature));
            }
        );
    }

    public void InitSampling()
    {
        xOffset = Random.Range(1, 999999);
        zOffset = Random.Range(1, 999999);
    }

    public float getSample(Vector2 pos, float rate)
    {
        float xCoord = pos.x * elevationSampleRate * 3 + xOffset;
        float zCoord = pos.y * elevationSampleRate * 3 + zOffset;
        float sample = Mathf.PerlinNoise(xCoord, zCoord);
        return sample;
    }

    public void topography()
    {
        showTopography = !showTopography;
        foreach (Node node in graph.nodes.Values)
        {
            Vector2 pos = node.tile.transform.localPosition;
            if (showTopography)
            {
                
                node.tile.transform.localPosition = new Vector2(pos.x,
                pos.y + Mathf.Max(0, node.elevation));
            } else
            {
                node.tile.transform.localPosition = Position(node.coords);
            }
            
        }
    }

    public override HexTile TileAt(HexCoords coords) { return graph.NodeAt(coords)?.tile; }
}
