===================================
SLOT MACHINE SETUP INSTRUCTIONS
===================================

# Floating Text Prefab Setup
1. Create a new empty GameObject and name it "FloatingText"
2. Add a TextMeshPro (not UI) component
3. Set Font Size to 5
4. Set Alignment to Center
5. Add the FloatingText.cs script
6. Configure the animation parameters:
   - Float Speed: 1.0
   - Fade Speed: 0.5
   - Scale Speed: 0.5
   - Initial Delay: 0.2
   - Lifetime: 2.0
7. Drag this GameObject to the Resources/Prefabs/UI folder to create a prefab
8. Delete the GameObject from the scene

# Slot Machine GameObject Setup
1. Create a new empty GameObject in your scene and name it "SlotMachine"
2. Add these components:
   - SpriteRenderer (assign your slot machine image)
   - BoxCollider2D (set IsTrigger to true)
   - AudioSource
3. Create a child GameObject named "InteractionPrompt"
   - Add Canvas component (Render Mode: World Space)
   - Add a Text component with "Press E to Play" text
   - Position it slightly above the slot machine
4. Add the SlotMachine.cs script to the main GameObject
5. Locate some sound effects for:
   - Spin Sound: A jingle/casino sound when spinning starts
   - Win Sound: A celebratory sound when getting an upgrade
6. Add these to the appropriate slots in the Inspector

# Slot Machine UI Setup
1. Create a new Canvas in your scene (Render Mode: Screen Space - Overlay)
2. Create a Panel as a child named "SlotMachineUI"
3. Set the panel's RectTransform:
   - Anchor: Bottom Left
   - Pivot: (0,0)
   - Position: (20, 20)
   - Size: (300, 200)
4. Add a CanvasGroup component to the panel
5. Add three Image components as children named "Reel1", "Reel2", "Reel3"
   - Position them side by side horizontally
   - Set their color to white
   - Make each about 60x60 pixels in size
6. Add a TextMeshProUGUI component below the reels named "ResultText"
   - Position it centrally below the reels
   - Set font size to 24
   - Set the text to "Result" (this will be changed at runtime)
7. Add a Particle System for the confetti effect (optional)
8. Add both SlotMachineUI.cs and SlotMachineAnimator.cs scripts to the panel
9. Assign references in the Inspector:
   - Reel Images: The three Image components
   - Result Text: The TextMeshProUGUI component
   - Confetti Effect: The Particle System
   - Canvas Rect: Reference to the panel's RectTransform
10. In the SlotMachine script, reference this UI panel

# Connecting Everything
1. In the SlotMachine component, assign:
   - Interaction Prompt: The child GameObject with the "Press E" text
   - Slot Machine UI: The SlotMachineUI panel
   - Spin Sound: The sound clip for spinning
   - Win Sound: The sound clip for winning
   - Win Particles: The particle system (if used)
2. Test by:
   - Running the game
   - Walking up to the slot machine
   - Pressing E to interact
   - Verifying the UI appears in the bottom left
   - Checking that reels spin and yield a result
   - Confirming the player receives the statistical upgrade

=================================== 