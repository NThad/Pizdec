using System.Collections.Generic;
using UnityEngine;

public class EchoVisionMode : MonoBehaviour
{
    public static bool IsActive { get; private set; }

    [Header("Echo Vision")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private bool startEnabled = false;

    private readonly List<Renderer> hiddenRenderers = new List<Renderer>();
    private readonly List<EchoTarget> targets = new List<EchoTarget>();

    private void Awake()
    {
        SetActive(startEnabled);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetActive(!IsActive);
    }

    public void SetActive(bool active)
    {
        IsActive = active;

        if (active)
            EnableEchoVision();
        else
            DisableEchoVision();
    }

    private void EnableEchoVision()
    {
        hiddenRenderers.Clear();
        targets.Clear();

        EchoTarget[] foundTargets = FindObjectsByType<EchoTarget>(FindObjectsSortMode.None);
        targets.AddRange(foundTargets);

        HashSet<Renderer> targetRenderers = new HashSet<Renderer>();
        foreach (EchoTarget target in targets)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
                targetRenderers.Add(renderer);

            target.SetEchoVisionActive(true);
        }

        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer renderer in allRenderers)
        {
            if (targetRenderers.Contains(renderer))
                continue;

            if (renderer is LineRenderer)
                continue;

            if (!renderer.enabled)
                continue;

            renderer.enabled = false;
            hiddenRenderers.Add(renderer);
        }
    }

    private void DisableEchoVision()
    {
        foreach (Renderer renderer in hiddenRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }

        hiddenRenderers.Clear();

        foreach (EchoTarget target in targets)
        {
            if (target != null)
                target.SetEchoVisionActive(false);
        }

        targets.Clear();
    }

    private void OnDestroy()
    {
        if (IsActive)
            DisableEchoVision();

        IsActive = false;
    }
}