using UnityEngine;

// This script is just for providing setup instructions
// and doesn't need to be attached to any GameObject
public class SoundSetupInstructions : MonoBehaviour
{
    /*
    HOW TO SET UP COIN AND HEART COLLECTION SOUNDS
    ==============================================
    
    1. Create a GameObject in your scene called "SoundFXManager"
    
    2. Add the SoundFXManager component to it
    
    3. Assign sound files to the component:
       - Coin Collect Sound: Assign your coin pickup sound
       - Heart Collect Sound: Assign your heart pickup sound
       
    4. Adjust the Default Volume if needed (1.0 is default)
    
    5. That's it! The Pickup script will now play sounds when:
       - Collecting coins (GoldCoin pickup type)
       - Collecting hearts (HealthGlobe pickup type)
       
    NOTES:
    - Both the AudioManager and SoundFXManager use the singleton pattern,
      so they'll persist between scenes
    - You only need to set this up once, prefereably in your first scene
    - If you see "Playing coin collect sound" in the console, but don't hear anything,
      check your sound files and make sure they're set up correctly
    */
    
    // This script is just for documentation
    void Start()
    {
        // Remove this component after showing setup message once
        Debug.Log("SoundFXManager setup instructions: See the script for details");
        Destroy(this);
    }
} 