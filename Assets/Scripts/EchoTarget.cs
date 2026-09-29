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

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private bool highlighted;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
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
        if (highlighted)
            return;

        highlighted = true;

        foreach (Renderer renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", highlightColor);
            propertyBlock.SetColor("_BaseColor", highlightColor);
            renderer.SetPropertyBlock(propertyBlock);
        }

        Debug.Log($"ECHO detected: {gameObject.name}");
    }
}
