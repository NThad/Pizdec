using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathGenerator : MonoBehaviour
{
    public GraphBuilder graphBuilder;
    public GameObject pathSegmentPrefab;

    [Header("Настройки троп")]
    public int segmentsPerEdge = 12;
    public float pathDeviation = 18f;

    private List<GameObject> pathSegments = new List<GameObject>();

    void Start()
    {
        if (graphBuilder != null)
        {
            StartCoroutine(GenerateAfterDelay());

        }
    }

    public void GeneratePathsFromMST()
    {
        ClearOldPaths();

        Debug.Log("Начинаем генерацию троп. Рёбер MST: " + graphBuilder.mstEdges.Count);

        foreach (var edge in graphBuilder.mstEdges)
        {
            Vector3 startPos = graphBuilder.nodes[edge.from];
            Vector3 endPos = graphBuilder.nodes[edge.to];

            Debug.Log($"Строим тропу от {startPos} к {endPos}");

            CreateWindingPath(startPos, endPos);
        }
    }
    private void CreateWindingPath(Vector3 start, Vector3 end)
    {
        Vector3 current = start;

        for (int i = 0; i < segmentsPerEdge; i++)
        {
            float t = (i + 1f) / segmentsPerEdge;
            Vector3 target = Vector3.Lerp(start, end, t);

            // Извилистость
            float wave = Mathf.Sin(i * 0.8f) * pathDeviation;
            Vector3 perpendicular = new Vector3(- (end - start).normalized.z, 0, (end - start).normalized.x);

            Vector3 nextPos = target + perpendicular * wave;

            // Спавним сегмент
            GameObject segment = Instantiate(pathSegmentPrefab, current, Quaternion.identity);
            pathSegments.Add(segment);

            // Поворачиваем и размещаем
            segment.transform.LookAt(nextPos);

            current = nextPos;
        }
    }

    private void ClearOldPaths()
    {
        foreach (var segment in pathSegments)
            if (segment != null) Destroy(segment);
        pathSegments.Clear();
    }

    private IEnumerator GenerateAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // небольшая задержка

        if (graphBuilder != null)
        {
            GeneratePathsFromMST();
        }
    }

}
