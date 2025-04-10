# Boss Spawn System Setup Guide

## Compiler Error Fixes

If you're experiencing compiler errors with the new scripts, here are common issues and solutions:

1. EnemyPathfinding reference:
   - Make sure your enemy prefab has an EnemyPathfinding component
   - The moveSpeed parameter should be accessible in the inspector

2. IEnemy interface:
   - Ensure you have an EnemyType component (like Shooter or another component) that implements IEnemy
   - This component should be assigned to the EnemyType field in the inspector

3. PlayAreaManager:
   - The scripts reference PlayAreaManager.Instance
   - Make sure your scene has a PlayAreaManager or the enemies don't need to be confined to the play area

## Setting Up an Angry Spawn Enemy

1. Create a new prefab based on the CactusEnemy prefab
2. Replace the EnemyAI component with AngryEnemyAI
3. Configure the fields:
   - Set your desired moveSpeed (usually higher than normal enemies)
   - Set an appropriate attackRange
   - Set the attackCooldown (usually lower than normal enemies)
   - Assign the enemy's attack script (Shooter or other IEnemy implementation) to the enemyType field

## Spawn Effect Setup

1. Create an empty GameObject called "SpawnEffect"
2. Add a ParticleSystem component and configure it
3. Add the SpawnEffect script
4. Assign the ParticleSystem to the spawnParticles field
5. Add an appropriate AudioClip if desired

If you still experience compiler errors, check the Unity console for specific error messages. 