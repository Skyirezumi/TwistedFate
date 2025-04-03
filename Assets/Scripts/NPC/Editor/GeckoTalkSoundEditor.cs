using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeckoTalkSound))]
public class GeckoTalkSoundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();
        
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Add space for separation
        EditorGUILayout.Space();
        
        // Get the target script
        GeckoTalkSound soundScript = (GeckoTalkSound)target;
        
        // Create a big, very noticeable test button
        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("TEST SOUND", GUILayout.Height(40)))
        {
            if (Application.isPlaying)
            {
                // Call the test method if in play mode
                soundScript.PlayTalkSound();
            }
            else
            {
                EditorUtility.DisplayDialog("Play Mode Required", 
                    "Enter Play Mode to test sounds.", "OK");
            }
        }
        GUI.backgroundColor = Color.white;
        
        // Show proximity visualization preview
        DrawProximityPreview();
        
        // Instructions
        EditorGUILayout.HelpBox(
            "PROXIMITY SOUND SETUP:\n\n" +
            "1. Add sound clip to 'Talk Sound'\n" +
            "2. Set volume to 10 or higher for base volume\n" +
            "3. Adjust Min/Max Distance for proximity effect:\n" +
            "   - Min Distance: Full volume within this range\n" +
            "   - Max Distance: No sound beyond this range\n\n" +
            "Sound plays when player presses E to interact,\n" +
            "Volume changes based on player distance.", 
            MessageType.Info);
            
        // Apply modified properties
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawProximityPreview()
    {
        // Make sure we have updated properties
        serializedObject.Update();
        
        // Check for volume falloff property in the updated GeckoTalkSound script
        SerializedProperty minDistance = serializedObject.FindProperty("minDistance");
        SerializedProperty maxDistance = serializedObject.FindProperty("maxDistance");
        
        // Skip drawing if properties are not found
        if (minDistance == null || maxDistance == null) 
        {
            EditorGUILayout.HelpBox("Could not find distance properties. Make sure the GeckoTalkSound script has minDistance and maxDistance fields.", MessageType.Warning);
            return;
        }
        
        // Draw preview area
        EditorGUILayout.LabelField("Proximity Volume Preview:", EditorStyles.boldLabel);
        Rect previewRect = GUILayoutUtility.GetRect(200, 50);
        
        // Draw background
        EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f));
        
        // Calculate usable area
        float width = previewRect.width - 20;
        float height = previewRect.height;
        float startX = previewRect.x + 10;
        float centerY = previewRect.y + height/2;
        
        // Draw max distance
        float maxWidth = width;
        Rect maxRect = new Rect(startX, previewRect.y + 5, maxWidth, height - 10);
        EditorGUI.DrawRect(maxRect, new Color(0, 0.5f, 0, 0.2f));
        
        // Draw min distance (safely)
        float minRatio = 0.2f; // Default value
        if (maxDistance.floatValue > 0) // Avoid division by zero
        {
            minRatio = Mathf.Clamp01(minDistance.floatValue / maxDistance.floatValue);
        }
        float minWidth = maxWidth * minRatio;
        Rect minRect = new Rect(startX, previewRect.y + 5, minWidth, height - 10);
        EditorGUI.DrawRect(minRect, new Color(0, 0.8f, 0, 0.4f));
        
        // Draw gecko position
        Rect geckoRect = new Rect(startX - 3, centerY - 3, 6, 6);
        EditorGUI.DrawRect(geckoRect, Color.yellow);
        
        // Draw labels
        EditorGUI.LabelField(new Rect(startX + minWidth - 10, centerY - 15, 50, 20), "Min");
        EditorGUI.LabelField(new Rect(startX + maxWidth - 15, centerY - 15, 50, 20), "Max");
        
        // Draw player example positions
        DrawPlayerPosition(startX + minWidth * 0.5f, centerY, "100%");
        DrawPlayerPosition(startX + minWidth + (maxWidth - minWidth) * 0.5f, centerY, "50%");
        DrawPlayerPosition(startX + maxWidth + 10, centerY, "0%");
    }
    
    private void DrawPlayerPosition(float x, float y, string label)
    {
        Rect playerRect = new Rect(x - 2, y - 2, 4, 4);
        EditorGUI.DrawRect(playerRect, Color.blue);
        EditorGUI.LabelField(new Rect(x - 10, y + 5, 40, 20), label);
    }
} 