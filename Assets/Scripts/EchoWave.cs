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
    private const float DetourAngleStep = 2f;
    private const float MaxDetourAngle = 120f;

    private readonly Dictionary<EchoTarget, Vector3> detectedTargets = new Dictionary<EchoTarget, Vector3>();

    public void Initialize(float radius, float waveDuration)
    {
        maxRadius = Mathf.Max(0f, radius);
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
        detectedTargets.Clear();

        for (int i = 0; i < Points; i++)
        {
            float angle = i * Mathf.PI * 2f / Points;
            float resolvedAngle = FindFreeAngle(origin, angle, currentRadius);
            Vector3 direction = new Vector3(Mathf.Cos(resolvedAngle), 0f, Mathf.Sin(resolvedAngle));
            Vector3 surfacePoint = GetSurfacePoint(origin, direction * currentRadius);

            line.SetPosition(i, surfacePoint + Vector3.up * SurfaceOffset);
            CheckForTargets(surfacePoint, origin);
        }

        foreach (KeyValuePair<EchoTarget, Vector3> hit in detectedTargets)
            hit.Key.OnEchoHit(hit.Value, Vector3.zero, currentRadius);
    }

    private float FindFreeAngle(Vector3 origin, float desiredAngle, float radius)
    {
        if (radius <= 0.01f)
            return desiredAngle;

        if (IsPathClear(origin, GetHorizontalPoint(origin, desiredAngle, radius)))
            return desiredAngle;

        int steps = Mathf.CeilToInt(MaxDetourAngle / DetourAngleStep);

        for (int step = 1; step <= steps; step++)
        {
            float offset = step * DetourAngleStep * Mathf.Deg2Rad;

            float left = desiredAngle - offset;
            if (IsPathClear(origin, GetHorizontalPoint(origin, left, radius)))
                return left;

            float right = desiredAngle + offset;
            if (IsPathClear(origin, GetHorizontalPoint(origin, right, radius)))
                return right;
        }

        return desiredAngle;
    }

    private Vector3 GetHorizontalPoint(Vector3 origin, float angle, float radius)
    {
        return origin + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private bool IsPathClear(Vector3 origin, Vector3 destination)
    {
        Vector3 start = origin + Vector3.up * 0.25f;
        Vector3 end = destination + Vector3.up * 0.25f;
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return true;

        RaycastHit[] hits = Physics.RaycastAll(start, direction.normalized, distance);

        foreach (RaycastHit hit in hits)
        {
            EchoObstacle obstacle = hit.collider.GetComponentInParent<EchoObstacle>();
            if (obstacle != null && obstacle.blocksEcho)
                return false;
        }

        return true;
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

    private void CheckForTargets(Vector3 surfacePoint, Vector3 origin)
    {
        // The target check uses a small physical overlap around the wave ring,
        // but the target itself must also be inside the current wave radius.
        // This prevents a target from being activated outside maxRadius.
        Vector3 horizontalOffset = new Vector3(
            surfacePoint.x - origin.x,
            0f,
            surfacePoint.z - origin.z
        );

        float distanceFromOrigin = horizontalOffset.magnitude;

        if (distanceFromOrigin > maxRadius + TargetCheckRadius)
            return;

        Collider[] colliders = Physics.OverlapSphere(surfacePoint, TargetCheckRadius);

        foreach (Collider collider in colliders)
        {
            EchoTarget target = collider.GetComponentInParent<EchoTarget>();
            if (target == null)
                continue;

            Vector3 targetPosition = target.transform.position;
            Vector3 targetOffset = new Vector3(
                targetPosition.x - origin.x,
                0f,
                targetPosition.z - origin.z
            );

            // Do not activate targets whose center is outside the actual
            // maximum reach of this echo pulse.
            if (targetOffset.magnitude > maxRadius + TargetCheckRadius)
                continue;

            detectedTargets[target] = surfacePoint;
        }
    }
}