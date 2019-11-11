using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldMap : Map
{
    public static FieldMap current;
    public int radius = 7;
    public int numHeightLevels = 10;
    [SerializeField]
    private float xOffset;
    [SerializeField]
    private float yOffset;
    [SerializeField]
    private float elevationSampleRate = 1.0f;

    public void Awake()
    {
        if (Map.current != null) { Destroy(this.gameObject); }
        else { Map.current = this; current = this; }

        xOffset = Random.Range(0, 999999);
        yOffset = Random.Range(0, 999999);
        
        graph = new HexGraph(radius);
        Generate();
        SpawnTiles();
        //setTopography(true);
        Camera.main.GetComponent<CameraFollow>().maxSize = (radius + 1) * Mathf.Sqrt(2.0f / 3);
    }

    public override void Generate()
    {
        float maxSample = 0.0f;
        float minSample = 1.0f;
        graph.resetNodes();
        graph.Center().Visit(
            (n) =>
            {
                n.elevation = getSample(Position(n.coords), elevationSampleRate);
                maxSample = Mathf.Max(maxSample, n.elevation);
                minSample = Mathf.Min(minSample, n.elevation);
            }
        );
        // normalization
        graph.resetNodes();
        graph.Center().Visit(
            (n) =>
            {
                n.elevation -= minSample;
                n.elevation /= (maxSample - minSample);
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

    public void showSelectable()
    {
        foreach (Node node in graph.nodes.Values)
        {
            if (!node.selectable)
            {
                node.tile.highlight(Color.black);
            }
        }
    }

    public float getSample(Vector2 pos, float rate)
    {
        float xCoord = pos.x * elevationSampleRate * 3 + xOffset;
        float zCoord = pos.y * elevationSampleRate * 3 + yOffset;
        float sample = Mathf.PerlinNoise(xCoord, zCoord);
        return sample;
    }

    public void setTopography(bool toggle)
    {
        graph.resetNodes();
        graph.Center().Visit(
            (n) => {
                if (toggle)
                {
                    Vector2 spawnLocation = Position(n.coords);
                    Vector2 offset = new Vector2(0, Mathf.Floor(n.elevation*numHeightLevels)*0.15f);
                    n.tile.transform.localPosition = spawnLocation + offset;

                } else
                {
                    n.tile.transform.localPosition = Position(n.coords);
                }
            }
        );
    }

    public override HexTile TileAt(HexCoords coords) { return graph.NodeAt(coords)?.tile; }

    public int GetHeight(HexCoords coords)
    {
        return (int)Mathf.Floor(TileAt(coords).node.elevation*numHeightLevels);
    }
}
