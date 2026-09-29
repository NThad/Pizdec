using System.Collections.Generic;
using UnityEngine;

public class EchoTarget : MonoBehaviour
{
    [Header("Sound Interaction")]
    [Range(0f, 1f)]
    public float absorption = 0.5f;

    [Range(0f, 1f)]
    public float reflection = 0.5f;

    public bool blocksSound = true;

    [Header("Echo Highlight")]
    public Color highlightColor = Color.white;
    [Min(0f)] public float outlineWidth = 0.025f;
    [Min(0f)] public float highlightDuration = 1.25f;
    [Min(0f)] public float fadeDuration = 0.75f;

    private Renderer[] renderers;
    private readonly List<Material> outlineMaterials = new();
    private readonly List<Material> originalMaterials = new();
    private float highlightTime;
    private bool highlighted;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                    continue;

                originalMaterials.Add(material);
            }
        }
    }

    private void Update()
    {
        if (!highlighted)
            return;

        highlightTime -= Time.deltaTime;

        if (highlightTime <= 0f)
        {
            highlighted = false;
            return;
        }
    }

    public float GetReflection()
    {
        return reflection;
    }

    public float GetAbsorption()
    {
        return absorption;
    }

    public void OnEchoDetected()
    {
        highlighted = true;
        highlightTime = highlightDuration + fadeDuration;

        Debug.Log($"ECHO detected: {gameObject.name}");
    }

    // Reserved for the progressive wave-driven outline implementation.
    // The contact point will be supplied by EchoWave in the next step.
    public void OnEchoHit(Vector3 contactPoint, Vector3 waveDirection)
    {
        OnEchoDetected();
    }
}
