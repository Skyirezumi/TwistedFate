using UnityEngine;
using System.Collections;

// Attach this to your CardLayer object
public class CardLayerAnimationController : MonoBehaviour
{
    [SerializeField] private CardShuffler cardShuffler;
    [SerializeField] private float delayBeforeDeal = 0.5f;
    
    // Define a delegate for the LayCards method
    public delegate void LayCardsDelegate();
    public LayCardsDelegate OnLayCards;
    
    private bool isAnimating = false;
    
    private void Awake()
    {
        // Try to automatically wire up the LayCards method if the component exists
        MonoBehaviour cardLayerComponent = GetComponent("CardLayer") as MonoBehaviour;
        if (cardLayerComponent != null)
        {
            System.Reflection.MethodInfo layCardsMethod = cardLayerComponent.GetType().GetMethod("LayCards");
            if (layCardsMethod != null)
            {
                OnLayCards = (LayCardsDelegate)System.Delegate.CreateDelegate(
                    typeof(LayCardsDelegate), 
                    cardLayerComponent, 
                    layCardsMethod
                );
            }
        }
    }
    
    // Call this method before laying cards
    public void PlayShuffleAnimation(bool dealAfterShuffle = true)
    {
        if (isAnimating) return;
        if (cardShuffler == null)
        {
            Debug.LogWarning("Card Shuffler not assigned");
            
            // If no shuffler, just call the lay cards method directly
            if (dealAfterShuffle && OnLayCards != null)
            {
                OnLayCards();
            }
            return;
        }
        
        isAnimating = true;
        cardShuffler.PlayShuffleAnimation(() => {
            StartCoroutine(FinishAnimation(dealAfterShuffle));
        });
    }
    
    private IEnumerator FinishAnimation(bool dealAfterShuffle)
    {
        yield return new WaitForSeconds(delayBeforeDeal);
        isAnimating = false;
        
        if (dealAfterShuffle && OnLayCards != null)
        {
            OnLayCards();
        }
        else if (dealAfterShuffle)
        {
            Debug.LogWarning("No LayCards method registered. Please register it manually via OnLayCards delegate.");
        }
    }
} 