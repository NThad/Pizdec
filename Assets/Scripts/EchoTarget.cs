using UnityEngine;

public class EchoTarget : MonoBehaviour
{
    [Header("Sound Interaction")]
    [Range(0f, 1f)]
    public float absorption = 0.5f;

    [Range(0f, 1f)]
    public float reflection = 0.5f;

    public bool blocksSound = true;

    public float GetReflection()
    {
        return reflection;
    }

    public float GetAbsorption()
    {
        return absorption;
    }
}
