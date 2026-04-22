using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    Pathfinding pathfinding;

    public void Awake()
    {
        if (Pathfinding.Instance == null)
            pathfinding = new();
        else
            pathfinding = Pathfinding.Instance;
    }
}