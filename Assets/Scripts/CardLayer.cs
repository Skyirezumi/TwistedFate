using UnityEngine;

public class CardLayer : MonoBehaviour
{
    private CardLayerAnimationController animationController;

    void Awake()
    {
        // Initialize the animation controller
        animationController = GetComponent<CardLayerAnimationController>();
        if (animationController == null)
        {
            animationController = gameObject.AddComponent<CardLayerAnimationController>();
        }
    }

    void Start()
    {
        // Manually connect the LayCards method to the animation controller
        if (animationController != null)
        {
            animationController.OnLayCards = LayCards;
        }
    }

    public void InitiateCardDealing()
    {
        // Instead of directly calling LayCards(), use the animation controller
        if (animationController != null)
        {
            animationController.PlayShuffleAnimation(true); // true means LayCards will be called automatically
        }
        else
        {
            // Fallback if no animation controller
            LayCards();
        }
    }

    public void LayCards()
    {
        // Your existing card laying code here
        // ...
    }
} 