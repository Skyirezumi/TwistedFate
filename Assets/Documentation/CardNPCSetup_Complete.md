# Complete Setup Guide for Card Layer NPC

## 1. Dialogue UI Setup

The CardLayerNPC uses a DialogManager to show dialogue. You need to:

1. Create a Canvas in your scene if you don't have one already:
   - Right-click in Hierarchy → UI → Canvas
   - Add an EventSystem if prompted

2. Create a dialogue panel under the Canvas:
   - Right-click on Canvas → UI → Panel
   - Rename to "DialoguePanel"
   - Set its anchor to bottom
   - Add a Text or TextMeshPro component to display dialogue

3. Create a DialogManager script:
   ```csharp
   using UnityEngine;
   using TMPro;
   using UnityEngine.UI;
   
   public class DialogManager : MonoBehaviour
   {
       public static DialogManager Instance;
       
       [SerializeField] private GameObject dialogPanel;
       [SerializeField] private TextMeshProUGUI dialogText;
       
       private void Awake()
       {
           Instance = this;
       }
       
       public void ShowDialog(string message)
       {
           dialogPanel.SetActive(true);
           dialogText.text = message;
       }
       
       public void CloseDialog()
       {
           dialogPanel.SetActive(false);
       }
   }
   ```

4. Add the DialogManager script to your Canvas and assign the references:
   - Drag the DialoguePanel to the "Dialog Panel" field
   - Drag the Text component to the "Dialog Text" field

## 2. Card Upgrade UI Setup

The CardLayerNPC displays an upgrade UI when interacted with:

1. Create the Card Upgrade UI Panel under your Canvas:
   - Right-click on Canvas → UI → Panel
   - Rename to "CardUpgradePanel" 
   - Center it on screen and size appropriately (e.g., 600×400)
   - Add a background image or color

2. Create the upgrade options (add 3 of these):
   - Create a Button under the panel for each upgrade option
   - Add TextMeshPro components for title and description
   - Position them vertically in the panel

3. Add the close button:
   - Create a Button at the top-right of the panel
   - Set its text to "X" or add a close icon

4. Create cost display:
   - Add a TextMeshPro component showing "Cost: 50 Gold"

5. Create the CardUpgradeUI script if it doesn't exist:
   ```csharp
   using UnityEngine;
   using TMPro;
   using UnityEngine.UI;
   using System;
   
   public class CardUpgradeUI : MonoBehaviour
   {
       public static CardUpgradeUI Instance;
       
       [SerializeField] private GameObject upgradePanel;
       [SerializeField] private Button[] upgradeButtons; 
       [SerializeField] private TextMeshProUGUI[] upgradeTitles;
       [SerializeField] private TextMeshProUGUI[] upgradeDescriptions;
       [SerializeField] private TextMeshProUGUI costText;
       [SerializeField] private Button closeButton;
       
       private CardUpgrade[] currentUpgrades;
       private int currentCost;
       
       private void Awake()
       {
           Instance = this;
           
           // Hide panel at start
           upgradePanel.SetActive(false);
           
           // Set up close button
           closeButton.onClick.AddListener(ClosePanel);
           
           // Set up upgrade selection buttons
           for (int i = 0; i < upgradeButtons.Length; i++)
           {
               int index = i; // Local copy for closure
               upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(index));
           }
       }
       
       public void ShowUpgrades(CardUpgrade[] upgrades, int cost)
       {
           upgradePanel.SetActive(true);
           currentUpgrades = upgrades;
           currentCost = cost;
           
           // Update cost text
           costText.text = "Cost: " + cost + " Gold";
           
           // Display upgrades
           for (int i = 0; i < upgradeButtons.Length; i++)
           {
               if (i < upgrades.Length)
               {
                   upgradeButtons[i].gameObject.SetActive(true);
                   upgradeTitles[i].text = upgrades[i].title;
                   upgradeDescriptions[i].text = upgrades[i].description;
               }
               else
               {
                   upgradeButtons[i].gameObject.SetActive(false);
               }
           }
       }
       
       private void OnUpgradeSelected(int upgradeIndex)
       {
           // Check if player has enough gold
           if (EconomyManager.Instance != null && 
               EconomyManager.Instance.GetCurrentGold() >= currentCost)
           {
               // Try to deduct gold with reflection (compatible with your economy system)
               var economyManager = EconomyManager.Instance;
               var spendMethod = economyManager.GetType().GetMethod("SpendGold");
               if (spendMethod != null)
               {
                   spendMethod.Invoke(economyManager, new object[] { currentCost });
               }
               
               // Apply upgrade
               ApplyUpgrade(currentUpgrades[upgradeIndex]);
               
               // Close the panel
               ClosePanel();
           }
       }
       
       private void ApplyUpgrade(CardUpgrade upgrade)
       {
           // Find CardThrower component
           var cardThrower = FindObjectOfType<CardThrower>();
           if (cardThrower != null)
           {
               // Apply upgrade based on type
               switch (upgrade.type)
               {
                   case CardUpgradeType.GreenAreaOfEffect:
                       cardThrower.ApplyGreenAreaUpgrade();
                       break;
                   case CardUpgradeType.BlueStun:
                       cardThrower.ApplyBlueStunUpgrade();
                       break;
                   case CardUpgradeType.RedPoison:
                       cardThrower.ApplyRedPoisonUpgrade();
                       break;
               }
           }
       }
       
       public void ClosePanel()
       {
           upgradePanel.SetActive(false);
       }
   }
   ```

6. Add the CardUpgradeUI script to your Canvas and assign references:
   - Drag the CardUpgradePanel to "Upgrade Panel"
   - Assign the upgrade buttons, titles, and descriptions arrays
   - Assign the cost text and close button

## 3. Economy Manager Setup (if missing)

If you don't have an EconomyManager, create a simple one:

```csharp
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;
    
    [SerializeField] private int currentGold = 100;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public int GetCurrentGold()
    {
        return currentGold;
    }
    
    public void SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            Debug.Log($"Spent {amount} gold. Remaining: {currentGold}");
        }
    }
    
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"Added {amount} gold. Total: {currentGold}");
    }
}
```

Add this script to a GameObject in your scene.

## 4. Testing the Complete Setup

1. Place the CardLayerNPC in your scene
2. Set up the dialogue UI components
3. Set up the card upgrade UI components 
4. Set up the economy manager
5. Play the scene and approach the NPC
6. Press E to interact
7. You should see the dialogue text appear with talking sounds
8. After the dialogue timer, the upgrade UI should appear
9. Select an upgrade (including the new Fan Shot) to purchase it
10. Test the upgraded card throws:
    - Green cards should have a larger explosion
    - Blue cards should stun enemies
    - Red cards should apply poison
    - If Fan Shot is purchased, throwing cards should launch a spread of 3 projectiles

## Upgrade Types

- **GreenAreaOfEffect**: Increases explosion radius for green cards.
- **BlueStun**: Adds a stun effect to blue cards.
- **RedPoison**: Adds a damage-over-time poison effect to red cards.
- **FanShot**: Throws three cards in a fan instead of one, each dealing slightly less damage.

## Troubleshooting

- If the dialogue doesn't appear, check the DialogManager is set up correctly and the script is attached
- If the card upgrade UI doesn't appear, ensure CardUpgradeUI is properly set up with all references
- Check the console for any error messages from the CardLayerNPC script 