using UnityEngine; // Added for [System.Serializable]

public enum CardUpgradeType
{
    GreenAreaOfEffect,
    BlueStun,
    RedPoison,
    RedFanShot,   // Modify to make fan shot specific to red cards
    BlueFanShot,  // Add blue fan shot
    GreenFanShot,  // Add green fan shot
    RedVampire,    // Red cards heal on hit
    BlueVampire,   // Blue cards heal on hit
    GreenVampire,   // Green cards heal on hit
    RedHomingPrecision,    // Red cards track targets
    BlueHomingPrecision,   // Blue cards track targets
    GreenHomingPrecision,   // Green cards track targets
    RedChainLightning,     // Red cards chain to nearby enemies
    BlueChainLightning,    // Blue cards chain to nearby enemies
    GreenChainLightning    // Green cards chain to nearby enemies
}

// Add the CardUpgrade class definition here
[System.Serializable]
public class CardUpgrade
{
    public CardUpgradeType type;
    public string title;
    public string description;
    
    public CardUpgrade(CardUpgradeType type, string title, string description)
    {
        this.type = type;
        this.title = title;
        this.description = description;
    }
} 