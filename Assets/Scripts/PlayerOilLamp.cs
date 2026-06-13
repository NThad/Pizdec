using UnityEngine;

public class PlayerOilLamp : MonoBehaviour
{
    public GameObject oilLamp;

    public float maxOil = 100f;
    public float oil = 100f;

    public float oilDrain = 5f;

    private bool hasLamp = false;
    private bool lampOn = false;

    void Update()
    {
        if (!hasLamp)
            return;

        // включение / выключение
        if (Input.GetKeyDown(KeyCode.F))
        {
            lampOn = !lampOn;

            oilLamp.SetActive(lampOn);
        }

        // расход масла
        if (lampOn)
        {
            oil -= oilDrain * Time.deltaTime;

            if (oil <= 0f)
            {
                oil = 0f;
                lampOn = false;
                oilLamp.SetActive(false);

                Debug.Log("Масло закончилось");
            }
        }

        // чтобы не уходило в минус/переполнение
        oil = Mathf.Clamp(oil, 0f, maxOil);
    }

    public void PickupLamp()
    {
        hasLamp = true;
        Debug.Log("Светильник подобран");
    }
}