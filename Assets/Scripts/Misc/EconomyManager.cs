using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomyManager : Singleton<EconomyManager>
{
    private TMP_Text goldText;
    private int currentGold = 0;

    const string COIN_AMOUNT_TEXT = "Gold Amount Text";

    // Get the current gold amount
    public int GetCurrentGold()
    {
        return currentGold;
    }
    
    // Add gold to the player
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldText();
    }
    
    // Check if player has enough gold
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }
    
    // Spend gold if player has enough
    public bool SpendGold(int amount)
    {
        Debug.Log($"[ECONOMY] Attempting to spend {amount} gold. Current gold: {currentGold}");
        
        if (HasEnoughGold(amount))
        {
            currentGold -= amount;
            UpdateGoldText();
            Debug.Log($"[ECONOMY] Successfully spent {amount} gold. Remaining gold: {currentGold}");
            return true;
        }
        
        Debug.LogWarning($"[ECONOMY] Not enough gold! Tried to spend {amount} but only have {currentGold}");
        return false;
    }

    // Original method - now just adds 1 gold coin
    public void UpdateCurrentGold() 
    {
        AddGold(1);
    }
    
    // Helper method to update the UI text
    private void UpdateGoldText()
    {
        if (goldText == null) 
        {
            goldText = GameObject.Find(COIN_AMOUNT_TEXT).GetComponent<TMP_Text>();
        }

        goldText.text = currentGold.ToString("D3");
    }
}
