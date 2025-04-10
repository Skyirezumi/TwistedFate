using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameSetupWizard : MonoBehaviour
{
    [Header("UI Canvas Reference")]
    [SerializeField] private Canvas mainCanvas;
    
    [Header("Prefabs")]
    [SerializeField] private TextMeshProUGUI textPrefab;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Image panelPrefab;
    
    public void SetupGameSystem()
    {
        #if UNITY_EDITOR
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("No Canvas found in scene! Please create one or assign it manually.");
                return;
            }
        }
        
        // Create GameManager if it doesn't exist
        CreateGameManager();
        
        // Create UI elements
        CreateTimerText();
        CreateBountyCounter();
        CreateVictoryPanel();
        
        // Log completion
        Debug.Log("Game End System Setup Complete! Please check the references in the GameManager.");
        #endif
    }
    
    private void CreateGameManager()
    {
        #if UNITY_EDITOR
        GameObject existingManager = GameObject.Find("GameManager");
        
        if (existingManager == null)
        {
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
            Debug.Log("GameManager created!");
        }
        else
        {
            if (existingManager.GetComponent<GameManager>() == null)
            {
                existingManager.AddComponent<GameManager>();
            }
            Debug.Log("GameManager already exists, using existing one.");
        }
        #endif
    }
    
    private void CreateTimerText()
    {
        #if UNITY_EDITOR
        // Check if it already exists
        TextMeshProUGUI existingTimer = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        
        if (existingTimer == null)
        {
            // Create timer
            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(mainCanvas.transform, false);
            
            TextMeshProUGUI timer = timerObj.AddComponent<TextMeshProUGUI>();
            timer.text = "00:00";
            timer.fontSize = 24;
            timer.alignment = TextAlignmentOptions.BottomRight;
            
            // Set position
            RectTransform rect = timerObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-10, 10);
            rect.sizeDelta = new Vector2(100, 30);
            
            // Assign to GameManager
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager != null)
            {
                SerializedObject serializedManager = new SerializedObject(manager);
                SerializedProperty timerProp = serializedManager.FindProperty("timerText");
                timerProp.objectReferenceValue = timer;
                serializedManager.ApplyModifiedProperties();
            }
            
            Debug.Log("Timer UI created!");
        }
        else
        {
            Debug.Log("Timer UI already exists, using existing one.");
        }
        #endif
    }
    
    private void CreateBountyCounter()
    {
        #if UNITY_EDITOR
        // Check if it already exists
        TextMeshProUGUI existingCounter = GameObject.Find("BountyCounterText")?.GetComponent<TextMeshProUGUI>();
        
        if (existingCounter == null)
        {
            // Create counter
            GameObject counterObj = new GameObject("BountyCounterText");
            counterObj.transform.SetParent(mainCanvas.transform, false);
            
            TextMeshProUGUI counter = counterObj.AddComponent<TextMeshProUGUI>();
            counter.text = "Bounties: 0/4";
            counter.fontSize = 24;
            counter.alignment = TextAlignmentOptions.TopRight;
            
            // Set position
            RectTransform rect = counterObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(150, 30);
            
            // Assign to GameManager
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager != null)
            {
                SerializedObject serializedManager = new SerializedObject(manager);
                SerializedProperty counterProp = serializedManager.FindProperty("bountyCounterText");
                counterProp.objectReferenceValue = counter;
                serializedManager.ApplyModifiedProperties();
            }
            
            Debug.Log("Bounty Counter UI created!");
        }
        else
        {
            Debug.Log("Bounty Counter UI already exists, using existing one.");
        }
        #endif
    }
    
    private void CreateVictoryPanel()
    {
        #if UNITY_EDITOR
        // Check if it already exists
        GameObject existingPanel = GameObject.Find("VictoryPanel");
        
        if (existingPanel == null)
        {
            // Create panel
            GameObject panelObj = new GameObject("VictoryPanel");
            panelObj.transform.SetParent(mainCanvas.transform, false);
            
            // Add panel image
            Image panel = panelObj.AddComponent<Image>();
            panel.color = new Color(0, 0, 0, 0.8f);
            
            // Set position
            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 300);
            
            // Add title
            GameObject titleObj = new GameObject("VictoryText");
            titleObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "VICTORY!";
            title.fontSize = 36;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.yellow;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(300, 50);
            
            // Add time text
            GameObject timeObj = new GameObject("TimeText");
            timeObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "Completion Time: 00:00";
            timeText.fontSize = 24;
            timeText.alignment = TextAlignmentOptions.Center;
            
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.5f, 0.5f);
            timeRect.anchorMax = new Vector2(0.5f, 0.5f);
            timeRect.pivot = new Vector2(0.5f, 0.5f);
            timeRect.anchoredPosition = new Vector2(0, 30);
            timeRect.sizeDelta = new Vector2(300, 40);
            
            // Add buttons
            GameObject playAgainObj = new GameObject("PlayAgainButton");
            playAgainObj.transform.SetParent(panelObj.transform, false);
            Button playAgainButton = playAgainObj.AddComponent<Button>();
            Image playAgainImg = playAgainObj.AddComponent<Image>();
            playAgainImg.color = new Color(0.2f, 0.6f, 0.2f, 1);
            
            GameObject playAgainTextObj = new GameObject("Text");
            playAgainTextObj.transform.SetParent(playAgainObj.transform, false);
            TextMeshProUGUI playAgainText = playAgainTextObj.AddComponent<TextMeshProUGUI>();
            playAgainText.text = "Play Again";
            playAgainText.fontSize = 20;
            playAgainText.alignment = TextAlignmentOptions.Center;
            playAgainText.color = Color.white;
            
            RectTransform playAgainTextRect = playAgainTextObj.GetComponent<RectTransform>();
            playAgainTextRect.anchorMin = Vector2.zero;
            playAgainTextRect.anchorMax = Vector2.one;
            playAgainTextRect.sizeDelta = Vector2.zero;
            
            RectTransform playAgainRect = playAgainObj.GetComponent<RectTransform>();
            playAgainRect.anchorMin = new Vector2(0.5f, 0);
            playAgainRect.anchorMax = new Vector2(0.5f, 0);
            playAgainRect.pivot = new Vector2(0.5f, 0);
            playAgainRect.anchoredPosition = new Vector2(-80, 40);
            playAgainRect.sizeDelta = new Vector2(140, 40);
            
            GameObject mainMenuObj = new GameObject("MainMenuButton");
            mainMenuObj.transform.SetParent(panelObj.transform, false);
            Button mainMenuButton = mainMenuObj.AddComponent<Button>();
            Image mainMenuImg = mainMenuObj.AddComponent<Image>();
            mainMenuImg.color = new Color(0.6f, 0.2f, 0.2f, 1);
            
            GameObject mainMenuTextObj = new GameObject("Text");
            mainMenuTextObj.transform.SetParent(mainMenuObj.transform, false);
            TextMeshProUGUI mainMenuText = mainMenuTextObj.AddComponent<TextMeshProUGUI>();
            mainMenuText.text = "Main Menu";
            mainMenuText.fontSize = 20;
            mainMenuText.alignment = TextAlignmentOptions.Center;
            mainMenuText.color = Color.white;
            
            RectTransform mainMenuTextRect = mainMenuTextObj.GetComponent<RectTransform>();
            mainMenuTextRect.anchorMin = Vector2.zero;
            mainMenuTextRect.anchorMax = Vector2.one;
            mainMenuTextRect.sizeDelta = Vector2.zero;
            
            RectTransform mainMenuRect = mainMenuObj.GetComponent<RectTransform>();
            mainMenuRect.anchorMin = new Vector2(0.5f, 0);
            mainMenuRect.anchorMax = new Vector2(0.5f, 0);
            mainMenuRect.pivot = new Vector2(0.5f, 0);
            mainMenuRect.anchoredPosition = new Vector2(80, 40);
            mainMenuRect.sizeDelta = new Vector2(140, 40);
            
            // Add VictoryScreen script
            VictoryScreen victoryScript = panelObj.AddComponent<VictoryScreen>();
            
            // Set references via SerializedObject to work with SerializeField fields
            SerializedObject serializedVictory = new SerializedObject(victoryScript);
            serializedVictory.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
            serializedVictory.FindProperty("playAgainButton").objectReferenceValue = playAgainButton;
            serializedVictory.FindProperty("victoryText").objectReferenceValue = title;
            serializedVictory.FindProperty("timeText").objectReferenceValue = timeText;
            serializedVictory.ApplyModifiedProperties();
            
            // Hide panel initially
            panelObj.SetActive(false);
            
            // Assign to GameManager
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager != null)
            {
                SerializedObject serializedManager = new SerializedObject(manager);
                SerializedProperty panelProp = serializedManager.FindProperty("victoryPanel");
                panelProp.objectReferenceValue = panelObj;
                
                SerializedProperty mainMenuProp = serializedManager.FindProperty("mainMenuButton");
                mainMenuProp.objectReferenceValue = mainMenuButton;
                
                SerializedProperty playAgainProp = serializedManager.FindProperty("playAgainButton");
                playAgainProp.objectReferenceValue = playAgainButton;
                
                serializedManager.ApplyModifiedProperties();
            }
            
            Debug.Log("Victory Panel UI created!");
        }
        else
        {
            Debug.Log("Victory Panel UI already exists, using existing one.");
        }
        #endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameSetupWizard))]
public class GameSetupWizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GameSetupWizard wizard = (GameSetupWizard)target;
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Setup Game End System", GUILayout.Height(30)))
        {
            wizard.SetupGameSystem();
        }
    }
}
#endif 