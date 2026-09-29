using UnityEngine;

public class SoundDistanceTester : MonoBehaviour
{
    public SoundSource soundSource;
    public Transform player;

    void Update()
    {
        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= soundSource.smallRadius)
        {
            Debug.Log("SMALL");
        }
        else if (distance <= soundSource.largeRadius)
        {
            Debug.Log("LARGE");
        }
        else
        {
            Debug.Log("OUTSIDE");
        }
    }
}