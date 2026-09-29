using UnityEngine;

public class EchoTarget : MonoBehaviour
{
    [Header("Sound Interaction")]
    [Range(0f, 1f)]
    public float absorption = 0.5f;

    [Range(0f, 1f)]
    public float reflection = 0.5f;

    public bool blocksSound = true;

    [Header("Echo Outline")]
    public Color outlineColor = Color.white;
    [Min(0f)] public float outlineWidth = 0.025f;
    [Min(0f)] public float revealSpeed = 6f;
    [Min(0f)] public float fadeSpeed = 2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private float highlight;
    private bool highlighted;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int Color = Shader.PropertyToID("_Color");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (!highlighted)
            return;

        highlight = Mathf.MoveTowards(highlight, 0f, fadeSpeed * Time.deltaTime);
        ApplyHighlight(highlight);

        if (highlight <= 0f)
            highlighted = false;
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
        highlight = 1f;
        ApplyHighlight(highlight);
        Debug.Log($"ECHO detected: {gameObject.name}");
    }

    public void OnEchoHit(Vector3 contactPoint, Vector3 waveDirection)
    {
        // Contact data is kept in the API so EchoWave can drive a spatial
        // outline later without changing the detection contract again.
        OnEchoDetected();
    }

    private void ApplyHighlight(float intensity)
    {
        Color color = outlineColor * intensity;

        foreach (Renderer renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColor, color);
            propertyBlock.SetColor(Color, color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
