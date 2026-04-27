using System.Collections.Generic;
using Sunspore;
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

public enum FlyingState
{
    AIRBORNE,
    LANDING,
    LANDED,
}

public class Pathfinding
{
    public static Pathfinding Instance { get; private set; }

    const float LOW_HEALTH_THRESHOLD = 0.35f;
    const float BASE_DETOUR_TOLERANCE = 5f;
    const float SWARM_DAMAGE_RADIUS = 5f;
    const int SWARM_DAMAGE_THRESHOLD = 3;
    const float TANK_AGGRO_RADIUS = 15f;

    Transform player;
    Dictionary<PlantTypes, List<Plant>> plants = new();
    List<Enemy> enemies = new();

    public Pathfinding()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("Only one instance of Pathfinding should exist");

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void AddPlant(Plant plant)
    {
        if (!plants.ContainsKey(plant.plantType))
            plants[plant.plantType] = new();
        plants[plant.plantType].Add(plant);
    }

    public void RemovePlant(Plant plant)
    {
        if (!plants.ContainsKey(plant.plantType)) return;
        plants[plant.plantType].Remove(plant);
    }

    public void RegisterEnemy(Enemy enemy) => enemies.Add(enemy);
    public void UnregisterEnemy(Enemy enemy) => enemies.Remove(enemy);

    /// <summary>
    /// Kills all registered enemies (e.g. at sunrise). Snapshot avoids mutation while enumerating.
    /// </summary>
    public void KillAllEnemies()
    {
        if (enemies.Count == 0) return;
        var snapshot = new List<Enemy>(enemies);
        foreach (var e in snapshot)
        {
            if (e == null) continue;
            e.TakeDamage(e.health, Vector2.zero, 0f);
        }
    }

    public Transform GetNextTarget(Enemy enemy)
    {
        return enemy.enemyType switch
        {
            EnemyType.SWARM => GetSwarmTarget(enemy),
            EnemyType.FLYING => GetFlyingTarget(enemy as FlyingEnemy),
            EnemyType.BREAKER => GetBreakerTarget(enemy),
            EnemyType.TANK => GetTankTarget(enemy),
            _ => GetBaseTarget(enemy),
        };
    }

    // --- Base Behavior ---

    Transform GetBaseTarget(Enemy enemy)
    {
        bool isLowHealth = enemy.health / enemy.maxHealth < LOW_HEALTH_THRESHOLD;
        if (isLowHealth)
            return GetNearestPlantAny(enemy.transform.position)?.transform ?? player;
        return GetValueOrFallback(enemy);
    }

    Transform GetValueOrFallback(Enemy enemy, float overrideTolerance = -1f)
    {
        var target = GetBestValueCrop(enemy.transform.position) ?? GetNearestPlantByGrowthPriority(enemy.transform.position);
        if (target == null) return player;

        float tolerance = overrideTolerance > 0f ? overrideTolerance : GetDetourTolerance(enemy);
        if (PathIsTooLong(enemy.transform.position, target.transform.position, tolerance))
        {
            var nearestBarrier = GetNearestOfType(PlantTypes.BARRIER, enemy.transform.position);
            if (nearestBarrier != null) return nearestBarrier.transform;
        }

        return target.transform;
    }

    // --- Swarm ---

    Transform GetSwarmTarget(Enemy enemy)
    {
        bool isLowHealth = enemy.health / enemy.maxHealth < LOW_HEALTH_THRESHOLD;
        if (isLowHealth)
            return GetNearestPlantAny(enemy.transform.position)?.transform ?? player;

        var seedling = GetNearestSeedling(enemy.transform.position);
        if (seedling != null) return seedling.transform;

        return GetValueOrFallback(enemy);
    }

    public bool IsInSwarm(Enemy enemy)
    {
        return enemies.Count(e =>
            e != enemy &&
            e.enemyType == EnemyType.SWARM &&
            Vector3.Distance(e.transform.position, enemy.transform.position) <= SWARM_DAMAGE_RADIUS
        ) >= SWARM_DAMAGE_THRESHOLD;
    }

    // --- Flying ---

    Transform GetFlyingTarget(FlyingEnemy enemy)
    {
        if (enemy.flyingState == FlyingState.LANDED)
            return GetSwarmTarget(enemy);

        var seedling = GetNearestSeedling(enemy.transform.position);
        if (seedling != null) return seedling.transform;

        var valueCrop = GetBestValueCrop(enemy.transform.position);
        return valueCrop?.transform ?? GetNearestPlantAny(enemy.transform.position)?.transform ?? player;
    }

    public bool CanMeleeHitFlying(FlyingEnemy enemy, Plant attacker)
    {
        return enemy.enemyType == EnemyType.FLYING &&
               enemy.flyingState != FlyingState.AIRBORNE &&
               Mathf.Abs(attacker.transform.position.x - enemy.transform.position.x) < 0.5f &&
               Mathf.Abs(attacker.transform.position.z - enemy.transform.position.z) < 0.5f;
    }

    // --- Breaker ---

    Transform GetBreakerTarget(Enemy enemy)
    {
        var matureCrop = GetBestValueCrop(enemy.transform.position);
        if (matureCrop != null) return matureCrop.transform;

        var anyValueCrop = GetBestValueCrop(enemy.transform.position);
        if (anyValueCrop != null) return anyValueCrop.transform;

        return player;
    }

    // --- Tank ---

    Transform GetTankTarget(Enemy enemy)
    {
        bool isLowHealth = enemy.health / enemy.maxHealth < LOW_HEALTH_THRESHOLD;
        if (isLowHealth)
            return GetNearestPlantAny(enemy.transform.position)?.transform ?? player;

        return GetValueOrFallback(enemy, BASE_DETOUR_TOLERANCE);
    }

    public bool ShouldAggroToTank(Enemy enemy)
    {
        if (enemy.enemyType == EnemyType.TANK) return false;

        return enemies.Any(e =>
            e.enemyType == EnemyType.TANK &&
            Vector3.Distance(e.transform.position, enemy.transform.position) <= TANK_AGGRO_RADIUS
        );
    }

    public Transform GetNearestTank(Vector3 pos)
    {
        return enemies
            .Where(e => e.enemyType == EnemyType.TANK)
            .OrderBy(e => Vector3.Distance(e.transform.position, pos))
            .FirstOrDefault()?.transform;
    }

    // --- Plant Queries ---

    Plant GetBestValueCrop(Vector3 pos)
    {
        if (!plants.ContainsKey(PlantTypes.VALUE)) return null;
        return plants[PlantTypes.VALUE]
            .OrderByDescending(p => p.currentStage)
            .ThenBy(p => Vector3.Distance(p.transform.position, pos))
            .FirstOrDefault();
    }

    Plant GetNearestSeedling(Vector3 pos)
    {
        return plants.Values
            .SelectMany(list => list)
            .Where(p => p.currentStage == 0)
            .OrderBy(p => Vector3.Distance(p.transform.position, pos))
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

    Plant GetNearestPlantAny(Vector3 pos)
    {
        return plants.Values
            .SelectMany(list => list)
            .OrderBy(p => Vector3.Distance(p.transform.position, pos))
            .FirstOrDefault();
    }

    Plant GetNearestOfType(PlantTypes type, Vector3 pos)
    {
        if (!plants.ContainsKey(type)) return null;
        return plants[type].OrderBy(p => Vector3.Distance(p.transform.position, pos)).FirstOrDefault();
    }

    // --- Pathfinding Helpers ---

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

    public Transform GetPlayerTransform() => player;

    // --- Plant Targeting ---

    public Enemy GetEnemyTarget(Plant plant, float range)
    {
        var inRange = enemies
            .Where(e => Vector3.Distance(e.transform.position, plant.transform.position) <= range)
            .Where(e =>
                plant.GetType() == typeof(Bloomburst)
                || e.enemyType != EnemyType.FLYING
                || CanMeleeHitFlying(e as FlyingEnemy, plant)
            )
            .OrderBy(e => Vector3.Distance(e.transform.position, plant.transform.position));

        var tank = inRange.FirstOrDefault(e => e.enemyType == EnemyType.TANK);
        return tank ?? inRange.FirstOrDefault();
    }
}