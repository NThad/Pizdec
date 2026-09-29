using UnityEngine;

public enum SoundType
{
    Footstep,
    Run,
    Scream,
    Other
}

public enum SoundMode
{
    Normal,
    Echo
}

public class SoundSource : MonoBehaviour
{
    [Header("Sound")]
    public SoundType soundType = SoundType.Other;

    public SoundMode soundMode = SoundMode.Normal;

    [Min(0f)]
    public float soundPower = 1f;

    [Header("Radius")]
    [Min(0f)]
    public float smallRadius = 5f;

    [Min(0f)]
    public float largeRadius = 15f;

    [Header("Optional")]
    public AudioSource audioSource;

    public float GetRadius()
    {
        return largeRadius;
    }

    public bool IsInsideSmallRadius(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= smallRadius;
    }

    public bool IsInsideLargeRadius(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= largeRadius;
    }

    public float GetIntensityAtDistance(float distance)
    {
        if (distance > largeRadius)
            return 0f;

        if (distance <= smallRadius)
            return soundPower;

        float t = Mathf.InverseLerp(largeRadius, smallRadius, distance);
        return soundPower * t;
    }

    public void EmitEcho()
    {
        if (soundMode != SoundMode.Echo)
            return;

        Debug.Log($"ECHO emitted from {gameObject.name}. Radius: {largeRadius}, Power: {soundPower}");
    }
}
