using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GardenPlacementMenu : MonoBehaviour
{
    public GardenPlacementController controller;
    public RectTransform panelRoot;

    [Header("Layout")]
    public string title = "Place Objects";
    public float buttonHeight = 50f;
    public float buttonSpacing = 8f;
    public float panelWidth = 300f;
    public float panelPadding = 12f;

    [Header("Style")]
    public Color panelColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color buttonColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color selectedColor = new Color(0.4f, 0.7f, 0.4f, 1f);
    public Color textColor = new Color(1f, 1f, 1f, 1f);

    private List<Button> itemButtons = new List<Button>();
    private Text titleText;
    private Text statusText;
    private GameObject panelObject;

    void Awake()
    {
        if (controller == null)
        {
            controller = FindObjectOfType<GardenPlacementController>();
        }

        if (controller != null)
        {
            controller.SelectionChanged += OnSelectionChanged;
        }
    }

    void Start()
    {
        BuildUI();
    }

    void OnDestroy()
    {
        if (controller != null)
        {
            controller.SelectionChanged -= OnSelectionChanged;
        }
    }

    private void BuildUI()
    {
        // Clean up old UI if it exists
        if (panelObject != null)
        {
            Destroy(panelObject);
        }

        // Get root
        if (panelRoot == null)
        {
            panelRoot = transform as RectTransform;
        }

        if (panelRoot == null)
        {
            Debug.LogError("GardenPlacementMenu: No RectTransform found!");
            return;
        }

        // Create main panel background
        panelObject = new GameObject("PlacementPanel");
        panelObject.transform.SetParent(panelRoot, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20);
        panelRect.sizeDelta = new Vector2(panelWidth, 800); // Increased from 500 so all buttons fit

        Image panelBg = panelObject.AddComponent<Image>();
        panelBg.color = panelColor;

        float currentY = -panelPadding;

        // Title
        titleText = AddText(panelObject, "Title", title, 24, new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, 40), TextAnchor.MiddleLeft);
        titleText.color = textColor;
        titleText.fontStyle = FontStyle.Bold;
        currentY -= 45;

        // Status
        statusText = AddText(panelObject, "Status", "Select an item", 16, new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, 50), TextAnchor.UpperLeft);
        statusText.color = textColor;
        currentY -= 60;

        // Buttons
        if (controller != null && controller.items.Count > 0)
        {
            itemButtons.Clear();

            for (int i = 0; i < controller.items.Count; i++)
            {
                if (controller.items[i]?.prefab == null)
                    continue;

                string label = string.IsNullOrEmpty(controller.items[i].label)
                    ? controller.items[i].prefab.name
                    : controller.items[i].label;

                int index = i;
                Button btn = AddButton(panelObject, label, new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, buttonHeight), () => controller.SelectItem(index));
                itemButtons.Add(btn);

                currentY -= (buttonHeight + buttonSpacing);
            }

            // Deselect button
            AddButton(panelObject, "Stop Placing / Empty Hand", new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, buttonHeight), () => controller.ClearSelection());
            currentY -= (buttonHeight + buttonSpacing);

            // Undo button
            AddButton(panelObject, "Undo Last Object", new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, buttonHeight), () => controller.UndoLastPlaced());
            currentY -= (buttonHeight + buttonSpacing);

            // Clear All button
            AddButton(panelObject, "Clear All Placed Objects", new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, buttonHeight), () => controller.ClearAllPlaced());
            currentY -= (buttonHeight + buttonSpacing);

            // Smooth/Clear Sand button
            AddButton(panelObject, "Smooth Sand", new Vector2(panelPadding, currentY), new Vector2(panelWidth - panelPadding * 2, buttonHeight), () =>
            {
                RakePainter painter = FindAnyObjectByType<RakePainter>();
                if (painter != null) painter.Clear(0f);
            });
        }

        RefreshSelection();
    }

    private Button AddButton(GameObject parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(label + "_Btn");
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = buttonColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.colors = new ColorBlock
        {
            normalColor = buttonColor,
            highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
            pressedColor = new Color(0.25f, 0.25f, 0.25f, 1f),
            selectedColor = selectedColor,
            disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f),
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };
        btn.onClick.AddListener(onClick);

        // GUARANTEED PHYSICS HITBOX - Bypasses Meta UI completely!
        BoxCollider box = btnObj.AddComponent<BoxCollider>();
        box.size = new Vector3(size.x, size.y, 4f);

        Text txt = AddText(btnObj, "Label", label, 18, Vector2.zero, size, TextAnchor.MiddleCenter);
        txt.color = textColor;
        txt.raycastTarget = false;

        RectTransform txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btn;
    }

    private Text AddText(GameObject parent, string name, string content, int fontSize, Vector2 position, Vector2 size, TextAnchor anchor)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = textColor;

        return text;
    }

    private void OnSelectionChanged(int selectedIndex)
    {
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (statusText != null)
        {
            if (controller == null || controller.SelectedIndex < 0)
            {
                statusText.text = "Select an item";
            }
            else
            {
                string label = GetSelectedLabel();
                statusText.text = $"Selected: {label}\nClick to place";
            }
        }

        for (int i = 0; i < itemButtons.Count; i++)
        {
            if (itemButtons[i] != null)
            {
                Image img = itemButtons[i].GetComponent<Image>();
                img.color = (i == controller.SelectedIndex) ? selectedColor : buttonColor;
            }
        }
    }

    private string GetSelectedLabel()
    {
        if (controller == null || controller.SelectedIndex < 0 || controller.SelectedIndex >= controller.items.Count)
            return "None";

        var entry = controller.items[controller.SelectedIndex];
        if (!string.IsNullOrWhiteSpace(entry.label))
            return entry.label;

        return entry.prefab != null ? entry.prefab.name : "Unknown";
    }
}