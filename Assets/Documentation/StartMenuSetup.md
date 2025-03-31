# Start Menu Setup Instructions

Follow these steps to create a simple start menu for your game:

## 1. Create a new Scene

1. In Unity, go to **File > New Scene** (or use the shortcut Ctrl+N)
2. Save the scene as "StartMenu" in your Scenes folder (**File > Save As**)

## 2. Set up the Canvas

1. Right-click in the Hierarchy window and select **UI > Canvas**
   - This will automatically create a Canvas with an EventSystem
2. With the Canvas selected, in the Inspector, set the Canvas Scaler:
   - Set **UI Scale Mode** to "Scale With Screen Size"
   - Set **Reference Resolution** to 1920 x 1080
   - Set **Match** to 0.5 (to balance width and height scaling)

## 3. Create a Background

1. Right-click on the Canvas in the Hierarchy and select **UI > Image**
2. Rename this image to "Background"
3. In the Inspector, set the Rect Transform to stretch to fill the entire canvas:
   - Set **Anchors** to stretch in both directions
   - Set **Left, Right, Top, Bottom** all to 0
4. Optionally, add a background image or color:
   - For a solid color: set the color property in the Image component
   - For an image: drag your background image to the Source Image property

## 4. Create a Game Title

1. Right-click on the Canvas in the Hierarchy and select **UI > Text - TextMeshPro** (or regular Text if TextMeshPro is not installed)
2. Rename to "GameTitle"
3. Position it at the top of the screen
4. In the Inspector:
   - Set the text to your game title (e.g., "Twisted Fate")
   - Set a suitable font size (e.g., 72)
   - Set alignment to center
   - Customize the color and font as desired

## 5. Create the Start Button

1. Right-click on the Canvas in the Hierarchy and select **UI > Button**
2. Rename to "StartButton"
3. Position it in the center of the screen
4. In the Inspector:
   - Set the button text to "START"
   - Adjust the button size as needed
   - Customize colors as desired

## 6. Add the MenuManager Script

1. In the Project window, navigate to the Scripts/UI folder
2. Find the MenuManager script we created
3. Create an empty GameObject in the Hierarchy:
   - Right-click in the Hierarchy and select **Create Empty**
   - Rename it to "MenuManager"
4. Drag the MenuManager script onto the MenuManager GameObject
5. In the Inspector, make sure the gameSceneName field is set to the name of your game scene (default is "SampleScene")

## 7. Connect the Button to the Script

1. Select the StartButton in the Hierarchy
2. In the Inspector, find the Button component
3. Under the "On Click ()" section, click the "+" button to add a new event
4. Drag the MenuManager GameObject from the Hierarchy to the empty object field
5. From the function dropdown, select **MenuManager > StartGame()**

## 8. Optional: Add a Quit Button

1. Duplicate the StartButton (Ctrl+D or right-click > Duplicate)
2. Rename to "QuitButton"
3. Position it below the StartButton
4. Change the button text to "QUIT"
5. In the Button component's "On Click ()" section:
   - Click the "+" button to add a new event
   - Drag the MenuManager GameObject to the empty object field
   - From the function dropdown, select **MenuManager > QuitGame()**

## 9. Set the Start Menu as the First Scene

1. Go to **File > Build Settings**
2. Drag your StartMenu scene from the Project window to the "Scenes In Build" list
3. Make sure StartMenu is at index 0 (the top)
4. Add your game scene to the list as well

## 10. Test Your Menu

1. Click the Play button in Unity
2. Verify that your Start button loads the game scene
3. Verify that your Quit button works (if added)

That's it! You now have a simple start menu for your game. 