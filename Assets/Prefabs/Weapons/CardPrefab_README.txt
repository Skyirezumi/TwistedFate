CARD WEAPON PREFAB SETUP

This system uses three separate card types (Red, Green, Blue) with different special effects, selected randomly when thrown.

## Creating the Card Prefabs

Create three separate card prefabs following these steps:

1. Red Card (Explosion effect)
   - Create a new Empty GameObject, name it "RedCardPrefab"
   - Add these components:
     - SpriteRenderer: Assign your red card sprite
     - BoxCollider2D: Check "Is Trigger", adjust size to match sprite
     - TrailRenderer:
       - Set Time to 0.5
       - Set Min Vertex Distance to 0.1
       - Set Alignment to "View"
       - Width: Start at 0.3, End at 0
       - Set Color Gradient with red color that fades out
       - Material: Create a material using "Particles/Additive" shader
     - DamageSource script: Set Damage Amount to 1
     - RedCard script:
       - Set Speed to 10
       - Set Lifetime to 5
       - Set Explosion Radius to 2 (for future implementation)
       - Assign the TrailRenderer component

2. Green Card (Poison effect)
   - Create a new Empty GameObject, name it "GreenCardPrefab"
   - Add the same components as the Red Card, but:
     - Assign your green card sprite
     - Replace RedCard with GreenCard script:
       - Set Speed to 10
       - Set Lifetime to 5
       - Set Poison Duration to 3 (for future implementation)
       - Assign the TrailRenderer component

3. Blue Card (Split effect)
   - Create a new Empty GameObject, name it "BlueCardPrefab"
   - Add the same components as the Red Card, but:
     - Assign your blue card sprite
     - Replace RedCard with BlueCard script:
       - Set Speed to 10
       - Set Lifetime to 5
       - Set Split Card Count to 3 (for future implementation)
       - Set Split Angle to 30 (for future implementation)
       - Assign the TrailRenderer component

4. After adding all components to each, drag them to your Project panel to create prefabs

## Setting up the CardThrower on your player:

1. Add the CardThrower script to your player GameObject
2. Assign all three card prefabs to their respective fields:
   - Red Card Prefab
   - Green Card Prefab
   - Blue Card Prefab
3. Create an empty child GameObject named "ThrowPoint" positioned where cards should spawn
4. Assign this to the "Throw Point" field

## Setting up Cooldown Visualization (optional):

1. Create a UI Canvas in your scene
2. Add an Image component for cooldown indicator:
   - Set the Image's Fill Method to "Radial 360" or "Filled"
   - Assign this Image to the "Cooldown Image" field
3. Optionally add an AudioClip to "Cooldown End Sound"

## Special Effects (Future Implementation):

Each card type has its own special effect structure:

- Red Card: Explosion effect - damages enemies in an area
  - Backfire: Explosion damages player instead

- Green Card: Poison effect - applies damage over time
  - Backfire: Poison applies to player instead

- Blue Card: Split effect - card splits into multiple projectiles
  - Backfire: Split cards target player instead

## Usage:

- Left-click to throw a random card in the direction of the mouse cursor
- Each throw will randomly select one of the three card types 