using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Start is called before the first frame update
    private enum State
    {
        Roaming,
    }

    private State currentState;
    private EnemyPathfinding pathfinding;
    void Start()
    {
        currentState = State.Roaming;
        pathfinding = GetComponent<EnemyPathfinding>();
        StartCoroutine(RoamingRoutine());
    }

    private IEnumerator RoamingRoutine()
    {
        while (currentState == State.Roaming)
        {
            Vector2 roamPosition = GetRoamingPosition();
            pathfinding.MoveTo(roamPosition);
            yield return new WaitForSeconds(2f);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

}
