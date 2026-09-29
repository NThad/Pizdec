using UnityEngine;

public class EchoVisionMode : MonoBehaviour
{
    public static bool IsActive { get; private set; }

    [Header("Echo Vision")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Q;
    [SerializeField] private bool startEnabled = false;

    private void Awake()
    {
        IsActive = startEnabled;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetActive(!IsActive);
    }

    public void SetActive(bool active)
    {
        IsActive = active;

        EchoTarget[] targets = FindObjectsByType<EchoTarget>(FindObjectsSortMode.None);
        foreach (EchoTarget target in targets)
            target.SetEchoVisionActive(active);
    }

    private void OnDestroy()
    {
        if (FindFirstObjectByType<EchoVisionMode>() == this)
            IsActive = false;
    }
}