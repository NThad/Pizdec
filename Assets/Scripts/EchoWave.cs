using System.Collections.Generic;
using UnityEngine;

public class EchoWave : MonoBehaviour
{
    private float maxRadius;
    private float duration;
    private float currentRadius;
    private LineRenderer line;

    private const int Points = 96;
    private const float RaycastHeight = 100f;
    private const float SurfaceOffset = 0.08f;
    private const float TargetCheckRadius = 0.75f;

    private readonly HashSet<EchoTarget> detectedTargets = new HashSet<EchoTarget>();

    public void Initialize(float radius, float waveDuration)
    {
        maxRadius = radius;
        duration = Mathf.Max(0.01f, waveDuration);
        currentRadius = 0f;

        line = gameObject.AddComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = Points;
        line.widthMultiplier = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));

        UpdateWave();
    }

    private void Update()
    {
        currentRadius += maxRadius / duration * Time.deltaTime;
        currentRadius = Mathf.Min(currentRadius, maxRadius);

        UpdateWave();

        if (currentRadius >= maxRadius)
            Destroy(gameObject);
    }

    private void UpdateWave()
    {
        if (line == null)
            return;

        Vector3 origin = transform.position;

        for (int i = 0; i < Points; i++)
        {
            float angle = i * Mathf.PI * 2f / Points;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 surfacePoint = GetSurfacePoint(origin, direction * currentRadius);

            line.SetPosition(i, surfacePoint + Vector3.up * SurfaceOffset);
            CheckForTargets(surfacePoint);
        }
    }

    private Vector3 GetSurfacePoint(Vector3 origin, Vector3 offset)
    {
        Vector3 rayStart = new Vector3(
            origin.x + offset.x,
            origin.y + RaycastHeight,
            origin.z + offset.z
        );

        RaycastHit[] hits = Physics.RaycastAll(
            rayStart,
            Vector3.down,
            RaycastHeight * 2f
        );

        float closestSurfaceY = float.NegativeInfinity;
        bool foundSurface = false;

        foreach (RaycastHit hit in hits)
        {
            EchoObstacle obstacle = hit.collider.GetComponentInParent<EchoObstacle>();

            if (obstacle == null || !obstacle.affectsEcho)
                continue;

            if (!foundSurface || hit.point.y > closestSurfaceY)
            {
                closestSurfaceY = hit.point.y;
                foundSurface = true;
            }
        }

        if (foundSurface)
        {
            return new Vector3(
                origin.x + offset.x,
                closestSurfaceY,
                origin.z + offset.z
            );
        }

        return new Vector3(
            origin.x + offset.x,
            origin.y,
            origin.z + offset.z
        );
    }

    private void CheckForTargets(Vector3 surfacePoint)
    {
        Collider[] colliders = Physics.OverlapSphere(surfacePoint, TargetCheckRadius);

        foreach (Collider collider in colliders)
        {
            EchoTarget target = collider.GetComponentInParent<EchoTarget>();

            if (target == null || detectedTargets.Contains(target))
                continue;

            detectedTargets.Add(target);
            target.OnEchoDetected();
        }
    }
}
