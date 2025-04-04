# Card Layer NPC Setup Guide

This guide will help you set up the Card Layer NPC to function similarly to the Gecko NPC, with dialogue and sound effects.

## Basic Setup

1. Create a new GameObject in your scene and name it "CardLayerNPC"
2. Add the `CardLayerNPC` script to the GameObject
3. Add an `AudioSource` component to the GameObject (or the script will add one automatically)
4. Add a sprite or 3D model to represent your NPC

## Interaction Setup

1. Create a child GameObject called "InteractionPrompt" with a UI element showing the "E" key
2. Assign this to the `interactionPrompt` field in the Inspector
3. Set the `interactionRange` to determine how close the player needs to be (default: 3)
4. Set the `interactionCooldown` to prevent spam interactions (default: 1)

## Dialogue Setup

1. In the "Dialogue Settings" section, customize the `dialogLines` array with your desired NPC phrases
2. Set the `dialogueDisplayTime` to control how long dialogue appears before showing the upgrade UI
3. Assign Gecko-like sound clips to the `talkingSounds` array (use the same sounds as Gecko for consistency)
4. Adjust `talkingSoundFrequency` (0.2 matches Gecko behavior)
5. Set `talkingVolume` to control how loud the talking sounds are (0.7 is recommended)

## Upgrade Setup

1. Configure the `numUpgradesToShow` (default: 3 - shows all upgrades)
2. Set the `upgradeCost` to determine how much gold each upgrade costs
3. Assign an `interactSound` that plays when the player first interacts with the NPC

## Sound Files

For the talking sounds to match the Gecko NPC, use the same audio clips:
1. Find the Gecko NPC in your project
2. Look at the `GeckoNPC` component's `talkingSounds` array
3. Copy those same sound references to your Card Layer NPC's `talkingSounds` array

## Testing

1. Make sure you have a `DialogManager` in your scene
2. Ensure the player has the `PlayerController` component
3. Enter Play mode and approach the Card Layer NPC
4. Press E to interact
5. The NPC should display dialogue text and play talking sounds
6. After the dialogue display time, the upgrade UI should appear

## Troubleshooting

If dialogue text doesn't appear:
- Check that a `DialogManager` exists in your scene
- Verify the `DialogManager` has a `ShowDialog` method

If sounds don't play:
- Ensure the audio clips are assigned to the `talkingSounds` array
- Check that the `AudioSource` component is present
- Confirm that audio is not muted in your game settings 