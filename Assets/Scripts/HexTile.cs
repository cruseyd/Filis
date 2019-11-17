using UnityEngine;
using System.Collections;
using TMPro;

public class HexTile : MonoBehaviour {
	
    public HexCoords coords { get { return node.coords; } }
    
    private Node _node;
    public Node node { get { return _node; } }
    public float elevation {
        get { return _node.elevation; }
    }

    // rendering / interaction
    public Transform occupant = null;
    public HexTerrain terrain {
        get { return _terrain; }
        set
        {
            _terrain = value;
            _topSprite.GetComponent<SpriteRenderer>().sprite = value.tileSprite;
            _fgSprite.GetComponent<SpriteRenderer>().sprite = value.foreground;
            _bgSprite.GetComponent<SpriteRenderer>().sprite = value.background;
        }
    }
    private HexTerrain _terrain;
    [SerializeField] private float _shadowLevel = 0;
    [SerializeField] private GameObject _topSprite;
    [SerializeField] private GameObject _fgSprite;
    [SerializeField] private GameObject _bgSprite;
    [SerializeField] private GameObject _mainSprite;
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private Renderer _telegraph;   
    public static float outlineWidth = 0.1f;
    private RaycastHit hit;

    public bool Walkable(UnitData unit) { return (occupant == null); }
    public bool Passable(UnitData unit) { return true; }
    public bool selectable { get { return node.selectable; } set { node.selectable = value; } }

	public void Start() {
        _mainSprite.GetComponent<SpriteRenderer>().material.color = terrain.color;
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

	public void highlight(Color color) {
        darken(0);
        _telegraph.gameObject.SetActive(true);
        _telegraph.material.color = color;
    }

    public void darken(float v)
    {
        Color shade = new Color(1.0f - v, 1.0f - v, 1.0f - v);
        tint(shade);
    }

    public void tint(Color c)
    {
        _mainSprite.GetComponent<SpriteRenderer>().material.SetColor("_Color", c);
        _topSprite?.GetComponent<SpriteRenderer>().material.SetColor("_Color", c);
        _fgSprite?.GetComponent<SpriteRenderer>().material.SetColor("_Color", c);
        _bgSprite?.GetComponent<SpriteRenderer>().material.SetColor("_Color", c);
    }

    public void resetColor ()
    {
        _telegraph.gameObject.SetActive(false);
        darken(0);
    }

    public void SetHeight(float h)
    {
        _mainSprite.GetComponent<RenderHeight>().SetHeight(h);
        _topSprite?.GetComponent<RenderHeight>().SetHeight(h);
        _fgSprite?.GetComponent<RenderHeight>().SetHeight(h);
        _bgSprite?.GetComponent<RenderHeight>().SetHeight(h);
    }
}
