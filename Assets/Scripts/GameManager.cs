using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }
    
    [Header("References:")]
    [SerializeField] private TileBase groundTile;
    
    private Tilemap _map;
    private Dictionary<string, Character> _characters = new();
    private List<Item> _items = new();

    private HashSet<string> _logIdentifiers = new();

    [Header("Attributes:")]
    [SerializeField] private float characterSpeed;

    [SerializeField] private Mode mode;
    [SerializeField] private float scatterTimer;

    public enum Mode
    {
        Chase,
        Scatter,
        Scared
    }
    
    private void Awake()
    {
        Game = this;
        _map = GameObject.Find("Level01").GetComponent<Tilemap>();
        _map.CompressBounds();
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographicSize = _map.cellBounds.size.y / 2f;
        cam.transform.position = _map.cellBounds.center - new Vector3(0.5f, 0.5f, 10);
    }

    private void Start()
    {
        GameMode = Mode.Scatter;
    }

    public static void RegisterCharacter(string identifier, Character character)
    {
        if (Game._characters.TryAdd(identifier, character)) return;
        Debug.LogWarning("Character with identifier " + identifier + " is already registered.");
    }

    public static void UnregisterCharacter(string identifier)
    {
        Game._characters.Remove(identifier);
    }
    
    public static Character GetCharacter(string identifier)
    {
        if (Game._characters.TryGetValue(identifier, out var character)) return character;
        LogWarning("No character found with identifier: " + identifier, "No " + identifier);
        return null;
    }

    public static void ClearCharacters()
    {
        foreach (var character in Game._characters)
            Destroy(character.Value.gameObject);
    }

    public static void RegisterItem(Item item)
    {
        if (!Game._items.Contains(item)) Game._items.Add(item);
    }

    public static void UnregisterItem(Item item)
    {
        Game._items.Remove(item);
    }

    public static void ClearItems()
    {
        foreach (var item in Game._items)
            Destroy(item.gameObject);
    }

    public static Tilemap LevelTilemap() { return Game._map; }

    public static float CharacterSpeed() { return Game.characterSpeed; }

    public static TileBase GroundTile() { return Game.groundTile; }

    public static Mode GameMode
    {
        get => Game.mode;
        private set
        {
            if (value == Game.mode) return;
            Game.mode = value;
            TriggerMode();
        }
    }

    private static void TriggerMode()
    {
        foreach (var character in Game._characters.Values) character.TriggerMode();

        switch (GameMode)
        {
            case Mode.Chase or Mode.Scatter:
                AudioManager.PlayMusicLoop(AudioManager.Audio.musicNormal);
                break;
            case Mode.Scared:
                AudioManager.PlayMusicImmediate(AudioManager.Audio.musicScared);
                Game.StartCoroutine(QueueMode(Mode.Chase, 9f));
                break;
        }
    }

    private static IEnumerator QueueMode(Mode mode, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        GameMode = mode;
    }

    public static void LogWarning(string message, string logIdentifier)
    {
        if (Game._logIdentifiers.Contains(logIdentifier)) return;
        
        Debug.LogWarning(message);
        Game._logIdentifiers.Add(logIdentifier);
    }
}
