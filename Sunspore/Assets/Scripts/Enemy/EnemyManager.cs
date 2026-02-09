using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    Pathfinding pathfinding;

    public void Awake()
    {
        pathfinding = new();
    }
}