using UnityEngine;

public class OilLampPickup : Interactable
{
    public override void Interact()
    {
        PlayerOilLamp lamp =
            FindObjectOfType<PlayerOilLamp>();

        if (lamp != null)
        {
            lamp.PickupLamp();
            Destroy(gameObject);
        }
    }
}