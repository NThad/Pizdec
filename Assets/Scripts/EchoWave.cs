using UnityEngine;

public class EchoWave : MonoBehaviour
{
    private float maxRadius;
    private float duration;
    private float currentRadius;
    private LineRenderer line;

    private const int Points = 64;

    public void Initialize(float radius, float waveDuration)
    {
        maxRadius = radius;
        duration = Mathf.Max(0.01f, waveDuration);
        currentRadius = 0f;

        line = gameObject.AddComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = false;
        line.positionCount = Points;
        line.widthMultiplier = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));

        UpdateCircle();
    }

    private void Update()
    {
        currentRadius += maxRadius / duration * Time.deltaTime;
        currentRadius = Mathf.Min(currentRadius, maxRadius);

        UpdateCircle();

        if (currentRadius >= maxRadius)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateCircle()
    {
        if (line == null)
            return;

        for (int i = 0; i < Points; i++)
        {
            float angle = i * Mathf.PI * 2f / Points;
            float x = Mathf.Cos(angle) * currentRadius;
            float z = Mathf.Sin(angle) * currentRadius;
            line.SetPosition(i, new Vector3(x, 0.05f, z));
        }
    }
}
