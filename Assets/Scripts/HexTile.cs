using UnityEngine;
using System.Collections;
using TMPro;

public class HexTile : MonoBehaviour {

	
    public HexCoords coords { get { return node.coords; } }
	
    public HexTerrain terrain { get { return _terrain; } }
    private Node _node;
    public Node node { get { return _node; } }
    public float elevation { get { return _node.elevation; } }

    // rendering / interaction
    public Transform occupant = null;
    [SerializeField] private HexTerrain _terrain;
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private Renderer _telegraph;   
    public static float outlineWidth = 0.1f;
    private RaycastHit hit;

    public bool Walkable(UnitData unit) { return (occupant == null); }
    public bool Passable(UnitData unit) { return true; }
    public bool selectable { get { return node.selectable; } set { node.selectable = value; } }

	public void Start() {
        //mainRenderer.sortingOrder = -coords.a;
        //_telegraph.sortingOrder = -coords.a;
        mainRenderer.material.color = terrain.color;
        _text.text = Mathf.Floor(node.elevation*10).ToString();
        //_text.GetComponent<MeshRenderer>().sortingOrder = -coords.a;
	}
    
	public static void Spawn(Node node, Transform parent,
		GameObject tilePrefab, Vector2 spawnLocation)
    {
		GameObject hexObj = Instantiate(tilePrefab);
		hexObj.transform.parent = parent;
        hexObj.transform.localPosition = spawnLocation;
        HexTile tile = hexObj.GetComponent<HexTile>();
		node.tile = tile;
        tile._node = node;
	}

	public HexTile parent()
	{
		return node.parent.tile;
	}

	public void highlight(Color color) {
        _telegraph.gameObject.SetActive(true);
        _telegraph.material.color = color;
	}

    public void resetColor ()
    {
        _telegraph.gameObject.SetActive(false);
    }

	public void outline(Color color) {
		StartCoroutine (animateBorder (0.1f));
	}

    
	IEnumerator animateBorder(float targetWidth)
	{
		float currentWidth = mainRenderer.material.GetFloat ("_OutlineWidth");
		while (currentWidth < targetWidth){
			currentWidth += 0.01f;
			mainRenderer.material.SetFloat ("_OutlineWidth", currentWidth);
			yield return null;
		}
	}

}
