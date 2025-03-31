using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Tooltip("The name of the game scene to load when the start button is clicked")]
    public string gameSceneName = "SampleScene";

    /// <summary>
    /// Starts the game by loading the game scene
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Starting the game...");
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 