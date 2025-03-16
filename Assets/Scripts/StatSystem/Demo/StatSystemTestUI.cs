using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// UI setup for the stat system test suite
    /// </summary>
    public class StatSystemTestUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private RectTransform characterPanel;
        [SerializeField] private RectTransform skillPanel;
        [SerializeField] private RectTransform itemPanel;
        [SerializeField] private RectTransform controlPanel;
        
        [Header("Text Components")]
        [SerializeField] private TextMeshProUGUI characterStatsText;
        [SerializeField] private TextMeshProUGUI skillStatsText;
        [SerializeField] private TextMeshProUGUI itemStatsText;
        
        [Header("Control Components")]
        [SerializeField] private Button equip1Button;
        [SerializeField] private TextMeshProUGUI equip1Text;
        [SerializeField] private Button equip2Button;
        [SerializeField] private TextMeshProUGUI equip2Text;
        [SerializeField] private Button unequipButton;
        [SerializeField] private Button castSkillButton;
        [SerializeField] private Toggle showDebugToggle;
        [SerializeField] private TextMeshProUGUI debugToggleText;
        
        [Header("Test Suite")]
        [SerializeField] private StatSystemTestSuite testSuite;
        
        private void Start()
        {
            // If we don't have a test suite, try to find it
            if (testSuite == null)
            {
                testSuite = FindFirstObjectByType<StatSystemTestSuite>();
            }
            
            // If found, hook up all the components
            if (testSuite != null)
            {
                SetupTestSuite();
            }
            else
            {
                Debug.LogError("StatSystemTestSuite not found!");
            }
        }
        
        /// <summary>
        /// Sets up the test suite with UI references
        /// </summary>
        private void SetupTestSuite()
        {
            // Use reflection to set the UI references in the test suite
            var testSuiteType = testSuite.GetType();
            
            SetField("characterStatsText", characterStatsText);
            SetField("skillStatsText", skillStatsText);
            SetField("itemStatsText", itemStatsText);
            SetField("equip1Button", equip1Button);
            SetField("equip2Button", equip2Button);
            SetField("unequipButton", unequipButton);
            SetField("castSkillButton", castSkillButton);
            SetField("showDebugToggle", showDebugToggle);
            
            // Set button text
            if (equip1Text != null)
                equip1Text.text = "Equip Sword";
                
            if (equip2Text != null)
                equip2Text.text = "Equip Staff";
                
            if (debugToggleText != null)
                debugToggleText.text = "Show Details";
        }
        
        /// <summary>
        /// Sets a field value on the test suite using reflection
        /// </summary>
        private void SetField(string fieldName, object value)
        {
            var field = testSuite.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (field != null && value != null)
            {
                field.SetValue(testSuite, value);
            }
        }
        
        /// <summary>
        /// Creates a full UI setup for testing
        /// </summary>
        public static GameObject CreateTestUI()
        {
            // Create parent canvas
            GameObject canvas = new GameObject("Stat System Test Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            // Create main panel layout
            GameObject mainPanel = CreatePanel("Main Panel", canvas.transform);
            RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0, 0);
            mainRect.anchorMax = new Vector2(1, 1);
            mainRect.offsetMin = new Vector2(20, 20);
            mainRect.offsetMax = new Vector2(-20, -20);
            
            HorizontalLayoutGroup mainLayout = mainPanel.AddComponent<HorizontalLayoutGroup>();
            mainLayout.spacing = 10;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = true;
            mainLayout.childForceExpandWidth = false;
            
            // Create stat panels
            GameObject characterPanel = CreatePanel("Character Panel", mainPanel.transform);
            GameObject skillPanel = CreatePanel("Skill Panel", mainPanel.transform);
            GameObject itemPanel = CreatePanel("Item Panel", mainPanel.transform);
            
            // Create control panel
            GameObject controlPanel = CreatePanel("Control Panel", canvas.transform);
            RectTransform controlRect = controlPanel.GetComponent<RectTransform>();
            controlRect.anchorMin = new Vector2(0, 0);
            controlRect.anchorMax = new Vector2(1, 0);
            controlRect.pivot = new Vector2(0.5f, 0);
            controlRect.sizeDelta = new Vector2(0, 60);
            controlRect.anchoredPosition = new Vector2(0, 10);
            
            HorizontalLayoutGroup controlLayout = controlPanel.AddComponent<HorizontalLayoutGroup>();
            controlLayout.spacing = 10;
            controlLayout.padding = new RectOffset(20, 20, 10, 10);
            controlLayout.childAlignment = TextAnchor.MiddleCenter;
            
            // Create buttons
            GameObject equip1Btn = CreateButton("Equip Sword Button", controlPanel.transform, "Equip Sword");
            GameObject equip2Btn = CreateButton("Equip Staff Button", controlPanel.transform, "Equip Staff");
            GameObject unequipBtn = CreateButton("Unequip Button", controlPanel.transform, "Unequip");
            GameObject castBtn = CreateButton("Cast Skill Button", controlPanel.transform, "Cast Fireball");
            
            // Create toggle
            GameObject debugToggle = CreateToggle("Debug Toggle", controlPanel.transform, "Show Details");
            
            // Create text displays
            GameObject charText = CreateTextDisplay("Character Stats Text", characterPanel.transform);
            GameObject skillText = CreateTextDisplay("Skill Stats Text", skillPanel.transform);
            GameObject itemText = CreateTextDisplay("Item Stats Text", itemPanel.transform);
            
            // Add the test UI script
            StatSystemTestUI uiScript = canvas.AddComponent<StatSystemTestUI>();
            
            // Set references
            uiScript.characterPanel = characterPanel.GetComponent<RectTransform>();
            uiScript.skillPanel = skillPanel.GetComponent<RectTransform>();
            uiScript.itemPanel = itemPanel.GetComponent<RectTransform>();
            uiScript.controlPanel = controlPanel.GetComponent<RectTransform>();
            
            uiScript.characterStatsText = charText.GetComponent<TextMeshProUGUI>();
            uiScript.skillStatsText = skillText.GetComponent<TextMeshProUGUI>();
            uiScript.itemStatsText = itemText.GetComponent<TextMeshProUGUI>();
            
            uiScript.equip1Button = equip1Btn.GetComponent<Button>();
            uiScript.equip1Text = equip1Btn.GetComponentInChildren<TextMeshProUGUI>();
            uiScript.equip2Button = equip2Btn.GetComponent<Button>();
            uiScript.equip2Text = equip2Btn.GetComponentInChildren<TextMeshProUGUI>();
            uiScript.unequipButton = unequipBtn.GetComponent<Button>();
            uiScript.castSkillButton = castBtn.GetComponent<Button>();
            uiScript.showDebugToggle = debugToggle.GetComponent<Toggle>();
            uiScript.debugToggleText = debugToggle.GetComponentInChildren<TextMeshProUGUI>();
            
            return canvas;
        }
        
        #region UI Creation Helpers
        
        private static GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.8f);
            
            return panel;
        }
        
        private static GameObject CreateButton(string name, Transform parent, string text)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent, false);
            
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 40);
            
            Image image = button.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f);
            
            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = image;
            
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
            btn.colors = colors;
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 14;
            
            return button;
        }
        
        private static GameObject CreateToggle(string name, Transform parent, string text)
        {
            GameObject toggle = new GameObject(name);
            toggle.transform.SetParent(parent, false);
            
            RectTransform rect = toggle.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 40);
            
            Toggle tgl = toggle.AddComponent<Toggle>();
            
            // Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(toggle.transform, false);
            
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(20, 20);
            bgRect.anchoredPosition = new Vector2(10, 0);
            
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            
            tgl.targetGraphic = bgImage;
            
            // Checkmark
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(background.transform, false);
            
            RectTransform checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = new Vector2(-4, -4);
            
            Image checkImage = checkmark.AddComponent<Image>();
            checkImage.color = new Color(0.8f, 0.8f, 0.8f);
            
            tgl.graphic = checkImage;
            
            // Label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(toggle.transform, false);
            
            RectTransform labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(30, 0);
            
            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.fontSize = 14;
            
            return toggle;
        }
        
        private static GameObject CreateTextDisplay(string name, Transform parent)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -10);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.fontSize = 14;
            tmp.color = Color.white;
            tmp.text = "Loading...";
            
            return textObj;
        }
        
        #endregion
    }
}