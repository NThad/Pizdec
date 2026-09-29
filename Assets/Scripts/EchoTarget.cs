using System.Collections.Generic;
using UnityEngine;

public class EchoTarget : MonoBehaviour
{
    [Header("Sound Interaction")]
    [Range(0f, 1f)] public float absorption = 0.5f;
    [Range(0f, 1f)] public float reflection = 0.5f;
    public bool blocksSound = true;

    [Header("Echo Outline")]
    public Color outlineColor = Color.white;
    [Min(0.001f)] public float outlineWidth = 0.025f;
    [Min(0f)] public float glowHoldTime = 1.5f;
    [Min(0f)] public float fadeSpeed = 2f;
    [Min(0.01f)] public float revealBand = 1.25f;

    private Renderer[] renderers;
    private Material outlineMaterial;
    private Material invisibleMaterial;
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private float revealRadius;
    private float targetRadius;
    private float fadeRadius;
    private float fadeOpacity = 0f;
    private bool revealComplete;
    private bool highlighted;
    private bool echoVisionActive;
    private float glowTimer;
    private Vector3 echoPoint;

    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int EchoPoint = Shader.PropertyToID("_EchoPoint");
    private static readonly int EchoRadius = Shader.PropertyToID("_EchoRadius");
    private static readonly int EchoBand = Shader.PropertyToID("_EchoBand");
    private static readonly int EchoFadeRadius = Shader.PropertyToID("_EchoFadeRadius");
    private static readonly int EchoFadeBand = Shader.PropertyToID("_EchoFadeBand");
    private static readonly int EchoOpacity = Shader.PropertyToID("_EchoOpacity");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        Shader shader = Shader.Find("Pizdec/EchoOutline");
        if (shader == null)
        {
            Debug.LogError($"EchoTarget on {gameObject.name}: Pizdec/EchoOutline shader was not found.");
            return;
        }

        Shader invisibleShader = Shader.Find("Pizdec/EchoInvisible");
        if (invisibleShader == null)
        {
            Debug.LogError($"EchoTarget on {gameObject.name}: Pizdec/EchoInvisible shader was not found.");
            return;
        }

        outlineMaterial = new Material(shader)
        {
            name = $"{gameObject.name} Echo Outline"
        };

        invisibleMaterial = new Material(invisibleShader)
        {
            name = $"{gameObject.name} Echo Invisible"
        };

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            originalMaterials[renderer] = materials;
            Material[] extended = new Material[materials.Length + 1];

            for (int i = 0; i < materials.Length; i++)
                extended[i] = materials[i];

            extended[materials.Length] = outlineMaterial;
            renderer.sharedMaterials = extended;
        }

        outlineMaterial.SetFloat(EchoOpacity, 0f);
    }

    private void Update()
    {
        if (!highlighted || outlineMaterial == null)
            return;

        if (!revealComplete)
        {
            if (revealRadius >= targetRadius)
            {
                revealRadius = targetRadius;
                revealComplete = true;
                glowTimer = glowHoldTime;
                fadeRadius = 0f;
            }
        }
        else
        {
            if (glowTimer > 0f)
            {
                glowTimer = Mathf.Max(0f, glowTimer - Time.deltaTime);
                ApplyOutline();
                return;
            }

            fadeRadius = Mathf.MoveTowards(fadeRadius, targetRadius, fadeSpeed * Time.deltaTime);
            fadeOpacity = 1f - Mathf.Clamp01(fadeRadius / Mathf.Max(targetRadius, 0.001f));

            ApplyOutline();

            if (fadeRadius >= targetRadius)
            {
                highlighted = false;
                revealComplete = false;
                revealRadius = 0f;
                targetRadius = 0f;
                fadeRadius = 0f;
                fadeOpacity = 0f;
                glowTimer = 0f;

                // Force the material fully invisible after the last fade frame.
                outlineMaterial.SetFloat(EchoOpacity, 0f);
                outlineMaterial.SetFloat(EchoRadius, 0f);
                outlineMaterial.SetFloat(EchoFadeRadius, 0f);
            }

            return;
        }

        ApplyOutline();
    }

    public void SetEchoVisionActive(bool active)
    {
        echoVisionActive = active;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (active)
            {
                Material[] invisible = new Material[originalMaterials[renderer].Length + 1];

                for (int i = 0; i < originalMaterials[renderer].Length; i++)
                    invisible[i] = invisibleMaterial;

                invisible[invisible.Length - 1] = outlineMaterial;
                renderer.sharedMaterials = invisible;
            }
            else if (originalMaterials.TryGetValue(renderer, out Material[] original))
            {
                Material[] restored = new Material[original.Length + 1];

                for (int i = 0; i < original.Length; i++)
                    restored[i] = original[i];

                restored[original.Length] = outlineMaterial;
                renderer.sharedMaterials = restored;
            }
        }

        if (!active)
        {
            highlighted = false;
            revealComplete = false;
            revealRadius = 0f;
            targetRadius = 0f;
            fadeRadius = 0f;
            fadeOpacity = 0f;
            glowTimer = 0f;

            if (outlineMaterial != null)
            {
                outlineMaterial.SetFloat(EchoRadius, 0f);
                outlineMaterial.SetFloat(EchoFadeRadius, 0f);
                outlineMaterial.SetFloat(EchoFadeBand, revealBand);
                outlineMaterial.SetFloat(EchoOpacity, 0f);
            }
        }
    }

    public float GetReflection() => reflection;
    public float GetAbsorption() => absorption;

    public void OnEchoDetected()
    {
        OnEchoHit(transform.position, Vector3.zero, 0f);
    }

    public void OnEchoHit(Vector3 contactPoint, Vector3 waveDirection, float waveRadius)
    {
        if (!highlighted)
        {
            echoPoint = contactPoint;
            revealRadius = waveRadius;
            targetRadius = GetMaxDistanceFromPoint(echoPoint);
            fadeRadius = 0f;
            glowTimer = 0f;
            revealComplete = false;
            highlighted = true;
            fadeOpacity = 1f;
        }

        if (revealComplete)
            return;

        revealRadius = Mathf.Min(targetRadius, Mathf.Max(revealRadius, waveRadius));

        if (revealRadius >= targetRadius)
        {
            revealRadius = targetRadius;
            revealComplete = true;
            glowTimer = glowHoldTime;
            fadeRadius = 0f;
            fadeOpacity = 1f;
        }

        ApplyOutline();
    }

    private float GetMaxDistanceFromPoint(Vector3 point)
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                bounds.Encapsulate(renderer.bounds);
        }

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        float maxDistance = 0f;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        float maxDistance = 0f;

        foreach (Vector3 corner in corners)
            maxDistance = Mathf.Max(maxDistance, Vector3.Distance(point, corner));

        return maxDistance;
    }

    private void ApplyOutline()
    {
        if (outlineMaterial == null)
            return;

        outlineMaterial.SetColor(OutlineColor, outlineColor);
        outlineMaterial.SetFloat(OutlineWidth, outlineWidth);
        outlineMaterial.SetVector(EchoPoint, echoPoint);
        outlineMaterial.SetFloat(EchoRadius, revealRadius);
        outlineMaterial.SetFloat(EchoBand, revealBand);
        outlineMaterial.SetFloat(EchoFadeRadius, fadeRadius);
        outlineMaterial.SetFloat(EchoFadeBand, revealBand);
        outlineMaterial.SetFloat(EchoOpacity, fadeOpacity);
    }

    private void OnDestroy()
    {
        if (outlineMaterial != null)
            Destroy(outlineMaterial);

        if (invisibleMaterial != null)
            Destroy(invisibleMaterial);
    }
}