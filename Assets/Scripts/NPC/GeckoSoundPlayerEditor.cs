#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeckoSoundPlayer))]
public class GeckoSoundPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GeckoSoundPlayer soundPlayer = (GeckoSoundPlayer)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Click below to test playing a sound without entering play mode.", MessageType.Info);
        
        // Big red play sound button
        GUI.backgroundColor = Color.red;
        if(GUILayout.Button("►PLAY TEST SOUND (LOUD)", GUILayout.Height(40)))
        {
            if (Application.isPlaying)
            {
                soundPlayer.TestPlaySound();
            }
            else
            {
                AudioClip[] sounds = GetSoundsFromTarget(soundPlayer);
                if (sounds != null && sounds.Length > 0)
                {
                    // Find a non-null sound
                    AudioClip soundToPlay = null;
                    for (int i = 0; i < sounds.Length; i++)
                    {
                        if (sounds[i] != null)
                        {
                            soundToPlay = sounds[i];
                            break;
                        }
                    }
                    
                    if (soundToPlay != null)
                    {
                        // Play the sound in edit mode
                        EditorSFX.PlayClip(soundToPlay);
                        Debug.Log($"GeckoSoundPlayerEditor: Playing test sound '{soundToPlay.name}'");
                    }
                    else
                    {
                        Debug.LogWarning("No valid sounds assigned to GeckoSoundPlayer!");
                    }
                }
                else
                {
                    Debug.LogWarning("No sounds assigned to GeckoSoundPlayer!");
                }
            }
        }
        GUI.backgroundColor = Color.white;
    }
    
    private AudioClip[] GetSoundsFromTarget(GeckoSoundPlayer soundPlayer)
    {
        // Use reflection to get the geckoSounds field
        var fieldInfo = soundPlayer.GetType().GetField("geckoSounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(soundPlayer) as AudioClip[];
        }
        return null;
    }
}

// Helper class for playing audio in edit mode
public static class EditorSFX
{
    public static void PlayClip(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        
        System.Reflection.Assembly assembly = typeof(AudioImporter).Assembly;
        System.Type audioUtilType = assembly.GetType("UnityEditor.AudioUtil");
        
        System.Reflection.MethodInfo method = audioUtilType.GetMethod(
            "PlayPreviewClip",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
            null,
            new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );
        
        method.Invoke(null, new object[] { clip, 0, false });
    }
}
#endif 