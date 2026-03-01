using System.Collections.Generic;
using Sunspore;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public enum PlantTypes
{
    VALUE,
    ATTACK,
    BARRIER,
}

public enum EnemyType
{
    SWARM,
    FLYING,
    TANK,
    BREAKER,
}

public class Pathfinding
{
    public static Pathfinding Instance { get; private set; }

    const float LOW_HEALTH_THRESHOLD = 0.35f;
    const float BASE_DETOUR_TOLERANCE = 5f;

    Transform player;
    Dictionary<PlantTypes, List<Plant>> plants = new();

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

    public void AddPlant(Plant plant)
    {

        if (!plants.ContainsKey(plant.plantType))
        {
            plants[plant.plantType] = new();
        }

        plants[plant.plantType].Add(plant);
    }

    public void RemovePlant(Plant plant)
    {
        if (!plants.ContainsKey(plant.plantType)) return;
        plants[plant.plantType].Remove(plant);
    }

    public Transform GetNextTarget(Enemy enemy)
    {
        bool isLowHealth = enemy.health / enemy.maxHealth < LOW_HEALTH_THRESHOLD;
        if (isLowHealth)
            return GetNearestPlantAny(enemy.transform.position)?.transform ?? player;

        return GetValueOrFallback(enemy);
    }

    Transform GetValueOrFallback(Enemy enemy, float overrideTolerance = -1f)
    {
        var target = GetBestValueCrop() ?? GetNearestPlantByGrowthPriority(enemy.transform.position);
        if (target == null) return player;

        float tolerance = overrideTolerance > 0f ? overrideTolerance : GetDetourTolerance(enemy);
        if (PathIsTooLong(enemy.transform.position, target.transform.position, tolerance))
        {
            var nearestBarrier = GetNearest(plants[PlantTypes.BARRIER], enemy.transform.position);
            if (nearestBarrier != null) return nearestBarrier.transform;
        }

        return target.transform;
    }

    Plant GetBestValueCrop()
    {
        return plants[PlantTypes.VALUE]
            .OrderBy(p => Vector3.Distance(p.transform.position, player.position))
            .FirstOrDefault();
    }

    Plant GetNearestPlantByGrowthPriority(Vector3 pos)
    {
        return plants.Values
            .SelectMany(list => list)
            .OrderBy(p => p.currentStage)
            .ThenBy(p => Vector3.Distance(p.transform.position, pos))
            .FirstOrDefault();
    }

    float GetDetourTolerance(Enemy enemy)
    {
        return BASE_DETOUR_TOLERANCE * (enemy.agent.speed / 5f);
    }

    bool PathIsTooLong(Vector3 from, Vector3 to, float tolerance)
    {
        var path = new NavMeshPath();
        NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);

        if (path.status != NavMeshPathStatus.PathComplete) return true;

        float pathLength = 0f;
        for (int i = 1; i < path.corners.Length; i++)
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);

        float straightLine = Vector3.Distance(from, to);
        return pathLength > straightLine * tolerance;
    }

    Plant GetNearest(List<Plant> list, Vector3 pos)
    {
        return list.OrderBy(p => Vector3.Distance(p.transform.position, pos)).FirstOrDefault();
    }

    public Transform GetPlayerTransform()
    {
        return player;
    }

    Plant GetNearestPlantAny(Vector3 pos)
    {
        return plants.Values
            .SelectMany(list => list)
            .OrderBy(p => Vector3.Distance(p.transform.position, pos))
            .FirstOrDefault();
    }
}