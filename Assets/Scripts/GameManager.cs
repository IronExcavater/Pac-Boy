using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }
    
    [Header("References:")]
    [SerializeField] private Tilemap _map;
    [SerializeField] private TileBase groundTile;
    
    private List<Character> _characters = new();
    private List<Item> _items = new();

    [Header("Attributes:")]
    [SerializeField] private float characterSpeed;

    private Vector3Int _playerPos;
    private Character.Direction _playerFacing;
    private float _scatterTimer;

    public enum Mode
    {
        Chase,
        Scatter,
        Scared
    }
    
    private void Awake()
    {
        if (Game == null)
        {
            Game = this;
            DontDestroyOnLoad(Game);
        }
        else Destroy(this);
    }

    private void Start()
    {
        _map = GameObject.Find("Level01").GetComponent<Tilemap>();
        TriggerMode(Mode.Chase);
    }

    public static void RegisterCharacter(Character character)
    {
        if (!Game._characters.Contains(character)) Game._characters.Add(character);
    }

    public static void UnregisterCharacter(Character character)
    {
        Game._characters.Remove(character);
    }

    public static void RegisterItem(Item item)
    {
        if (!Game._items.Contains(item)) Game._items.Add(item);
    }

    public static void UnregisterItem(Item item)
    {
        Game._items.Remove(item);
    }

    public static Tilemap LevelTilemap() { return Game._map; }

    public static float CharacterSpeed() { return Game.characterSpeed; }

    public static TileBase GroundTile() { return Game.groundTile; }

    public static Vector3Int PlayerPosition { get; set; }

    public static Character.Direction PlayerFacing { get; set; }

    private static void TriggerMode(Mode mode)
    {
        foreach (var character in Game._characters) character.ChangeMode(mode);

        switch (mode)
        {
            case Mode.Chase or Mode.Scatter:
                AudioManager.PlayMusicLoop(AudioManager.Audio.musicNormal);
                break;
            case Mode.Scared:
                AudioManager.PlayMusicImmediate(AudioManager.Audio.musicScared);
                Game.StartCoroutine(TriggerMode(Mode.Chase, 9f));
                break;
        }
    }

    private static IEnumerator TriggerMode(Mode mode, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        TriggerMode(mode);
    }
}
