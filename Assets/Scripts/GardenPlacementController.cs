using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GardenPlacementController : MonoBehaviour
{
    [Serializable]
    public class PlacementEntry
    {
        public string label;
        public GameObject prefab;
        public float surfaceOffset = 0.02f;
        public bool randomYaw = true;
    }

    [Header("Placeable Items")]
    public List<PlacementEntry> items = new List<PlacementEntry>();

    [Header("Raycast")]
    public Transform rayOrigin;
    public LayerMask placementMask = ~0;
    public float rayDistance = 25f;

    [Header("Placement")]
    public Transform placedParent;
    public bool alignToWorldUp = true;
    public bool placeOnClick = true;
    public bool ignoreClicksOverUI = true;

    [Header("Input")]
    public bool useMouseInput = true;
    public bool useOculusTrigger = true;
    public float placementSuppressSeconds = 0.15f;

    [Header("Audio")]
    public AudioClip placeSound;
    public AudioClip uiClickSound;
    public AudioClip clearSound;
    private AudioSource audioSource;

    public int SelectedIndex { get; private set; } = -1;

    public event Action<int> SelectionChanged;

    private float suppressPlacementUntilTime;
    private LineRenderer placementLaser;

    // Track what we placed so we can undo it
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        // Add AudioSource if missing
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        // Add manual laser drawing
        placementLaser = gameObject.AddComponent<LineRenderer>();
        placementLaser.startWidth = 0.008f;
        placementLaser.endWidth = 0.008f;
        placementLaser.material = new Material(Shader.Find("Sprites/Default"));
        placementLaser.positionCount = 2;
        placementLaser.sortingOrder = 999;
    }

    void Update()
    {
        Ray ray = GetPlacementRay();
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, rayDistance);

        // 1. ALWAYS draw the laser manually, bypassing Meta!
        if (placementLaser != null)
        {
            placementLaser.enabled = true;
            placementLaser.SetPosition(0, ray.origin);
            placementLaser.SetPosition(1, hitSomething ? hit.point : ray.origin + ray.direction * rayDistance);

            // Turn Green when pointing at UI buttons, OR if pointing at Sand and an item IS selected
            if (hitSomething && (hit.collider.GetComponentInParent<UnityEngine.UI.Button>() != null ||
               (((1 << hit.collider.gameObject.layer) & placementMask) != 0 && SelectedIndex >= 0)))
            {
                placementLaser.startColor = new Color(0f, 1f, 0f, 0.8f);
                placementLaser.endColor = new Color(0f, 1f, 0f, 0.8f);
            }
            else
            {
                placementLaser.startColor = new Color(1f, 0f, 0f, 0.8f); // Red otherwise
                placementLaser.endColor = new Color(1f, 0f, 0f, 0.8f);
            }
        }

        // 2. Handle fool-proof manual clicks!
        if (ShouldPlaceThisFrame() && hitSomething)
        {
            // Did we hit a Menu Button?
            var btn = hit.collider.GetComponentInParent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.Invoke(); // Force click it!
                return; // Stop here so we don't accidentally place an item on the UI
            }

            // Did we hit the Sand?
            if (placeOnClick && SelectedIndex >= 0 && Time.unscaledTime >= suppressPlacementUntilTime)
            {
                if (((1 << hit.collider.gameObject.layer) & placementMask) != 0)
                {
                    PlaceSelected();
                }
            }
        }
    }

    public void SelectItem(int index)
    {
        if (uiClickSound != null && audioSource != null) audioSource.PlayOneShot(uiClickSound, 0.7f);

        if (index < 0 || index >= items.Count)
        {
            SelectedIndex = -1;
        }
        else if (items[index] != null && items[index].prefab != null)
        {
            SelectedIndex = index;
        }

        suppressPlacementUntilTime = Time.unscaledTime + placementSuppressSeconds;
        SelectionChanged?.Invoke(SelectedIndex);
    }

    public void ClearSelection()
    {
        if (uiClickSound != null && audioSource != null) audioSource.PlayOneShot(uiClickSound, 0.7f);

        SelectedIndex = -1;
        suppressPlacementUntilTime = Time.unscaledTime + placementSuppressSeconds;
        SelectionChanged?.Invoke(SelectedIndex);
    }

    public bool PlaceSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= items.Count)
        {
            return false;
        }

        PlacementEntry entry = items[SelectedIndex];
        if (entry == null || entry.prefab == null)
        {
            return false;
        }

        if (!TryGetRaycastHit(out RaycastHit hit))
        {
            return false;
        }

        Quaternion rotation = GetPlacementRotation(entry, hit.normal);
        // FORCE world Up for offset so that negative numbers literally go downwards into the ground
        Vector3 position = hit.point + Vector3.up * entry.surfaceOffset;

        GameObject placedObject = Instantiate(entry.prefab, position, rotation, placedParent);
        placedObject.transform.position = position;
        placedObject.transform.rotation = rotation;

        spawnedObjects.Add(placedObject);

        if (placeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(placeSound, 0.8f);
        }

        return true;
    }

    public void UndoLastPlaced()
    {
        if (clearSound != null && audioSource != null) audioSource.PlayOneShot(clearSound, 0.6f);

        // Go backwards through the list to find the last valid object
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
                spawnedObjects.RemoveAt(i);
                return;
            }
            // If it's already null (e.g. you manually deleted it in hierarchy), just remove it from the list and keep looking.
            spawnedObjects.RemoveAt(i);
        }
    }

    public void ClearAllPlaced()
    {
        if (clearSound != null && audioSource != null) audioSource.PlayOneShot(clearSound, 0.8f);

        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }

    public bool TryGetRaycastHit(out RaycastHit hit)
    {
        Ray ray = GetPlacementRay();
        return Physics.Raycast(ray, out hit, rayDistance, placementMask, QueryTriggerInteraction.Ignore);
    }

    public Ray GetPlacementRay()
    {
        Transform origin = rayOrigin;
        if (origin == null && Camera.main != null)
        {
            origin = Camera.main.transform;
        }

        if (origin == null)
        {
            origin = transform;
        }

        return new Ray(origin.position, origin.forward);
    }

    private Quaternion GetPlacementRotation(PlacementEntry entry, Vector3 surfaceNormal)
    {
        float yaw = entry.randomYaw ? UnityEngine.Random.Range(0f, 360f) : 0f;

        if (!alignToWorldUp)
        {
            return Quaternion.Euler(0f, yaw, 0f);
        }

        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
        return yawRotation * surfaceRotation;
    }

    private bool ShouldPlaceThisFrame()
    {
        bool mouseClicked = false;

#if ENABLE_INPUT_SYSTEM
        if (useMouseInput && UnityEngine.InputSystem.Mouse.current != null)
        {
            mouseClicked = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif

        if (mouseClicked)
        {
            return true;
        }

        if (useOculusTrigger)
        {
            // Trigger check for BOTH hands so it works universally!
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        }

        return false;
    }
}