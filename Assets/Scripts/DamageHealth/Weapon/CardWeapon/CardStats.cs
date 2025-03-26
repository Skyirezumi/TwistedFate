using UnityEngine;

[System.Serializable]
public class CardStat
{
    [Range(1, 5)]
    public int level = 1;
    public float baseValue;
    public float valuePerLevel;
    
    public float GetValue()
    {
        return baseValue + (level - 1) * valuePerLevel;
    }
}

[System.Serializable]
public class CardStats
{
    [Header("Level 1-5 Stats")]
    public CardStat speed = new CardStat { baseValue = 10f, valuePerLevel = 2f };
    public CardStat damage = new CardStat { baseValue = 1f, valuePerLevel = 1f };
    public CardStat explosionRadius = new CardStat { baseValue = 1f, valuePerLevel = 0.5f };
    public CardStat criticalChance = new CardStat { baseValue = 0.05f, valuePerLevel = 0.05f };
    public CardStat homingStrength = new CardStat { baseValue = 0f, valuePerLevel = 0.5f };
} 