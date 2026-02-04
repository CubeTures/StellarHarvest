using System.Collections.Generic;
using Sunspore;
using UnityEditor.PackageManager;
using UnityEngine;

public enum PlantTypes
{
    VALUE,
}

public enum EnemyType
{
    SIMPLE
}

public class Pathfinding
{
    public static Pathfinding Instance { get; private set; }

    Transform player;
    List<Transform> plants = new();

    public Pathfinding()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Only one instance of Pathfinding should exist");
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void AddPlant(Transform plant, PlantTypes type)
    {
        plants.Add(plant);
    }

    public void RemovePlant(Transform plant)
    {
        plants.Remove(plant);
    }

    public Transform GetNextTarget(Transform enemy, EnemyType type)
    {
        // TODO different algorithms for each enemy type
        if (type == EnemyType.SIMPLE)
        {
            Transform closest = player;
            float dist = float.PositiveInfinity;

            foreach (Transform plant in plants)
            {
                float d = Vector3.Distance(plant.position, enemy.position);
                if (d < dist)
                {
                    dist = d;
                    closest = plant;
                }
            }

            return closest;
        }

        return null;
    }
}