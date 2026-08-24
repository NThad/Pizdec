using UnityEngine;
using System.Collections.Generic;

public class GraphBuilder : MonoBehaviour
{
    public List<Vector3> nodes = new List<Vector3>();
    public List<List<int>> adjacencyList = new List<List<int>>();

    // Результат работы Прима
    public List<(int from, int to)> mstEdges = new List<(int, int)>(); // Минимальное остовное дерево

    void Start()
    {
        BuildGraph();
        RunPrimAlgorithm();
    }

    public void BuildGraph()
    {
        InitializeAdjacencyList();
        ConnectNearbyNodes();
        Debug.Log("Граф построен! Узлов: " + nodes.Count);
    }

    private void InitializeAdjacencyList()
    {
        adjacencyList.Clear();
        for (int i = 0; i < nodes.Count; i++)
        {
            adjacencyList.Add(new List<int>());
        }
    }

    private void ConnectNearbyNodes()
    {
        float maxDist = 90f;

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                float dist = Vector3.Distance(nodes[i], nodes[j]);
                if (dist < maxDist && dist > 5f)
                {
                    adjacencyList[i].Add(j);
                    adjacencyList[j].Add(i);
                }
            }
        }
    }

    // === АЛГОРИТМ ПРИМА ===
    public void RunPrimAlgorithm()
    {
        mstEdges.Clear();

        int n = nodes.Count;
        bool[] inMST = new bool[n];           // Вошёл ли узел в дерево
        float[] minWeight = new float[n];     // Минимальный вес до узла
        int[] parent = new int[n];            // Родитель узла в дереве

        Debug.Log("Пример позиции узла 0: " + nodes[0]);
        Debug.Log("Количество рёбер в MST: " + mstEdges.Count);

        for (int i = 0; i < n; i++)
        {
            minWeight[i] = float.MaxValue;
        }

        // Начинаем с узла 0 (Избушка)
        minWeight[0] = 0;
        parent[0] = -1;

        for (int count = 0; count < n - 1; count++)
        {
            // Находим узел с минимальным весом, который ещё не в MST
            int u = -1;
            float min = float.MaxValue;

            for (int v = 0; v < n; v++)
            {
                if (!inMST[v] && minWeight[v] < min)
                {
                    min = minWeight[v];
                    u = v;
                }
            }

            if (u == -1) break; // Граф несвязный

            inMST[u] = true;

            // Обновляем соседей
            for (int v = 0; v < adjacencyList[u].Count; v++)
            {
                int neighbor = adjacencyList[u][v];

                float weight = Vector3.Distance(nodes[u], nodes[neighbor]);

                if (!inMST[neighbor] && weight < minWeight[neighbor])
                {
                    minWeight[neighbor] = weight;
                    parent[neighbor] = u;
                }
            }
        }

        // Собираем рёбра MST
        for (int i = 1; i < n; i++)
        {
            if (parent[i] != -1)
            {
                mstEdges.Add((parent[i], i));
            }
        }

        Debug.Log("Алгоритм Прима завершён. Рёбер в MST: " + mstEdges.Count);
    }
}