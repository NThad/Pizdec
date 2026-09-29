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
    [Min(0.01f)] public float revealSpeed = 8f;
    [Min(0f)] public float fadeSpeed = 2f;
    [Min(0.01f)] public float revealBand = 1.25f;

    private Renderer[] renderers;
    private Material outlineMaterial;
    private float revealRadius;
    private float targetRadius;
    private float outlineFade;
    private bool highlighted;
    private Vector3 echoPoint;

    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int EchoPoint = Shader.PropertyToID("_EchoPoint");
    private static readonly int EchoRadius = Shader.PropertyToID("_EchoRadius");
    private static readonly int EchoBand = Shader.PropertyToID("_EchoBand");
    private static readonly int EchoFade = Shader.PropertyToID("_EchoFade");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        Shader shader = Shader.Find("Pizdec/EchoOutline");
        if (shader == null)
        {
            Debug.LogError($"EchoTarget on {gameObject.name}: Pizdec/EchoOutline shader was not found.");
            return;
        }

        outlineMaterial = new Material(shader)
        {
            name = $"{gameObject.name} Echo Outline"
        };

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            Material[] extended = new Material[materials.Length + 1];

            for (int i = 0; i < materials.Length; i++)
                extended[i] = materials[i];

            extended[materials.Length] = outlineMaterial;
            renderer.sharedMaterials = extended;
        }
    }

    private void Update()
    {
        if (!highlighted || outlineMaterial == null)
            return;

        revealRadius = Mathf.MoveTowards(revealRadius, targetRadius, revealSpeed * Time.deltaTime);
        outlineFade = Mathf.MoveTowards(outlineFade, 0f, fadeSpeed * Time.deltaTime);

        ApplyOutline();

        if (outlineFade <= 0f)
            highlighted = false;
    }

    public float GetReflection() => reflection;
    public float GetAbsorption() => absorption;

    public void OnEchoDetected()
    {
        OnEchoHit(transform.position, Vector3.zero, 0f);
    }

    public void OnEchoHit(Vector3 contactPoint, Vector3 waveDirection, float waveRadius)
    {
        echoPoint = contactPoint;
        targetRadius = Mathf.Max(targetRadius, waveRadius);
        revealRadius = Mathf.Min(revealRadius, targetRadius);
        outlineFade = 1f;
        highlighted = true;

        ApplyOutline();
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
        outlineMaterial.SetFloat(EchoFade, outlineFade);
    }

    private void OnDestroy()
    {
        if (outlineMaterial != null)
            Destroy(outlineMaterial);
    }
}