using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSpawner : MonoBehaviour
{
    [SerializeField] private GameObject goldCoin, healthGlobe;
    [SerializeField] private int multiplier = 1;
    public void DropItems()
    {
        int randomNum = Random.Range(1, 3);

        if (randomNum == 1)
        {
            Instantiate(healthGlobe, transform.position, Quaternion.identity);
        }

        int randomAmountOfGold = Random.Range(1, 4) * multiplier;

        for (int i = 0; i < randomAmountOfGold; i++)
        {
            Instantiate(goldCoin, transform.position, Quaternion.identity);
        }

    }
}
