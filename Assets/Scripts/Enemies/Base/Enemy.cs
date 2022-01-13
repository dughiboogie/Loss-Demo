using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public void DisableEnemy()
    {
        Seeker seeker = GetComponent<Seeker>();
        if(seeker != null) {
            seeker.enabled = false;
            GetComponent<EnemyPathfinding>().StopFollowingPlayer();
        }
        else {
            GetComponent<EnemyPatrol>().StopMoving();
        }

        GetComponent<EnemyCombat>().enabled = false;
        this.enabled = false;
    }

}
