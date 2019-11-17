using UnityEngine;
using System.Collections;
using TMPro;

public class HexTile : MonoBehaviour {

	
    public HexCoords coords { get { return node.coords; } }
	
    
    private Node _node;
    public Node node { get { return _node; } }
    public float elevation { get { return _node.elevation; } }

    // rendering / interaction
    public Transform occupant = null;
    public HexTerrain terrain {
        get { return _terrain; }
        set
        {
            _terrain = value;
            _tileRenderer.sprite = value.tileSprite;
            _fgRenderer.sprite = value.foreground;
            _bgRenderer.sprite = value.background;
        }
    }
    private HexTerrain _terrain;
    [SerializeField] private SpriteRenderer _tileRenderer;
    [SerializeField] private SpriteRenderer _fgRenderer;
    [SerializeField] private SpriteRenderer _bgRenderer;
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private Renderer _telegraph;   
    public static float outlineWidth = 0.1f;
    private RaycastHit hit;

    public bool Walkable(UnitData unit) { return (occupant == null); }
    public bool Passable(UnitData unit) { return true; }
    public bool selectable { get { return node.selectable; } set { node.selectable = value; } }

	public void Start() {
        mainRenderer.material.color = terrain.color;
        _text.text = FieldMap.current.GetHeight(coords).ToString();
	}
    
	public static void Spawn(Node node, Transform parent,
		GameObject tilePrefab, HexTerrain terrain)
    {
		GameObject hexObj = Instantiate(tilePrefab);
		hexObj.transform.parent = parent;
        hexObj.transform.localPosition = Map.Position(node.coords);
        HexTile tile = hexObj.GetComponent<HexTile>();
        tile.terrain = terrain;
		node.tile = tile;
        tile._node = node;
	}

	public HexTile parent()
	{
		return node.parent.tile;
	}

	public void highlight(Color color) {
        darken(0);
        _telegraph.gameObject.SetActive(true);
        _telegraph.material.color = color;
    }

    public void darken(float v)
    {
        Color shade = new Color(1.0f - v, 1.0f - v, 1.0f - v);
        mainRenderer.material.SetColor("_Color", shade);
        _tileRenderer?.material.SetColor("_Color", shade);
        _fgRenderer?.material.SetColor("_Color", shade);
        _bgRenderer?.material.SetColor("_Color", shade);
    }

    public void resetColor ()
    {
        _telegraph.gameObject.SetActive(false);
        darken(0);
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
