using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{   
    public GraphBuilder graphBuilder;
    [Header("Префабы")]
    public GameObject hutPrefab;
    public GameObject[] ritualPrefabs;

    [Header("Настройки карты")]
    public float mapSize = 500f;
    public int numberOfRitualPlaces = 4;
    public int numberOfRandomNodes = 40;        // Промежуточные узлы

    [Header("Ограничения расстояний")]
    public float minDistanceBetweenImportant = 130f; // Между Избушкой и ритуалами
    public float minDistanceBetweenNodes = 35f;     // Между всеми узлами

    private List<Vector3> nodes = new List<Vector3>(); // Все узлы
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        GenerateNodes();
    }

    public void GenerateNodes()
    {
        ClearOldLevel();

        // 1. Спавним Избушку
        Vector3 hutPos = GetRandomPosition();
        GameObject hut = Instantiate(hutPrefab, hutPos, Quaternion.identity);
        spawnedObjects.Add(hut);
        nodes.Add(hutPos);

        // 2. Спавним 4 ритуальных места
        for (int i = 0; i < numberOfRitualPlaces; i++)
        {
            Vector3 ritualPos = GetValidPosition(nodes, minDistanceBetweenImportant);
            GameObject ritual = Instantiate(ritualPrefabs[i % ritualPrefabs.Length], ritualPos, Quaternion.identity);
            spawnedObjects.Add(ritual);
            nodes.Add(ritualPos);
        }

        // 3. Спавним случайные промежуточные узлы
        for (int i = 0; i < numberOfRandomNodes; i++)
        {
            Vector3 nodePos = GetValidPosition(nodes, minDistanceBetweenNodes);
            nodes.Add(nodePos);
            // Можно спавнить визуальный маркер для отладки
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = nodePos;
            marker.transform.localScale = new Vector3(2, 2, 2);
        }
        if (graphBuilder != null)
        {
            graphBuilder.nodes.Clear();
            graphBuilder.nodes.AddRange(nodes);

            graphBuilder.BuildGraph();           // Сначала строим граф
            graphBuilder.RunPrimAlgorithm();     // Потом Прима

            // Потом можно вызвать PathGenerator
        }       
    }       

    // Получить случайную позицию в пределах карты
    private Vector3 GetRandomPosition()
    {
        float half = mapSize * 0.45f; // отступ от края
        return new Vector3(Random.Range(-half, half), 0, Random.Range(-half, half));
    }

    // Получить позицию, которая достаточно далеко от уже существующих
    private Vector3 GetValidPosition(List<Vector3> existing, float minDistance)
    {
        Vector3 pos;
        int attempts = 0;

        do
        {
            pos = GetRandomPosition();
            attempts++;
        } 
        while (attempts < 100 && IsTooClose(pos, existing, minDistance));

        return pos;
    }

    private bool IsTooClose(Vector3 pos, List<Vector3> existing, float minDistance)
    {
        foreach (var p in existing)
        {
            if (Vector3.Distance(pos, p) < minDistance)
                return true;
        }
        return false;
    }

    private void ClearOldLevel()
    {
        foreach (var obj in spawnedObjects)
            if (obj != null) Destroy(obj);
        spawnedObjects.Clear();
        nodes.Clear();
    }
}