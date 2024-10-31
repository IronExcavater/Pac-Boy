using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float cherrySpeed = 2f;
    [SerializeField] private float spawnDistance = 2f;
    
    private Vector3 levelCenter;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        var map = GameManager.LevelTilemap();
        levelCenter = map.cellBounds.center;
        StartCoroutine(SpawnCherry());
    }

    private IEnumerator SpawnCherry()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            var spawnPosition = RandomSpawnPosition();
            var cherry = Instantiate(cherryPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(MoveCherry(cherry, spawnPosition));
        }
    }

    private IEnumerator MoveCherry(GameObject cherry, Vector3 spawnPosition)
    {
        var endPosition = EndPosition(spawnPosition);
        AnimationManager.AddTween(cherry.transform, endPosition,
            Vector3.Distance(spawnPosition, endPosition) / cherrySpeed,
            AnimationManager.Easing.Linear);
        yield return new WaitUntil(() => cherry == null || !AnimationManager.TargetExists(cherry.transform));
        Destroy(cherry);
    }

    private Vector3 RandomSpawnPosition()
    {
        var side = Random.Range(0, 4);
        var camHeight = mainCamera.orthographicSize;
        var camWidth = camHeight * mainCamera.aspect;

        return side switch
        {
            0 => new Vector3(Random.Range(-camWidth, camWidth), camHeight + spawnDistance), // Top
            1 => new Vector3(Random.Range(-camWidth, camWidth), -camHeight - spawnDistance), // Bottom
            2 => new Vector3(-camWidth - spawnDistance, Random.Range(-camHeight, camHeight)), // Left
            3 => new Vector3(camWidth + spawnDistance, Random.Range(-camHeight, camHeight)), // Right
            _ => Vector3.zero
        };
    }

    private Vector3 EndPosition(Vector3 start)
    {
        // Calculate an end position on the opposite side of the screen
        var direction = (levelCenter - start).normalized;
        return levelCenter + direction * (Vector3.Distance(start, levelCenter) + spawnDistance * 2f);
    }
}
