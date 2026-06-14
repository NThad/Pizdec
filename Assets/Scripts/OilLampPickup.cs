using UnityEngine;

public class OilLampPickup : Interactable
{
    public override void Interact()
    {
        Debug.Log("Светильник подобран!");

        PlayerOilLamp lamp =
            FindObjectOfType<PlayerOilLamp>();

        Debug.Log("Найден PlayerOilLamp: " + lamp);

        if (lamp != null)
        {
            lamp.PickupLamp();
            Destroy(gameObject);
        }
    }
}