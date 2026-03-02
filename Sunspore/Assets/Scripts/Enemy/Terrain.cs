using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
class NavMeshTerrain : MonoBehaviour
{
	public static NavMeshTerrain Instance { get; private set; }
	private NavMeshSurface surface;
	private Dictionary<int, BoxCollider> colliders = new();

	void Awake()
	{
		Instance = this;
		surface = GetComponent<NavMeshSurface>();
	}

	public void AddObstacle(GameObject obj, BoxCollider collider)
	{
		colliders[obj.GetHashCode()] = collider;
		ReBake();
	}

	public void RemoveObstacle(GameObject obj)
	{
		colliders.Remove(obj.GetHashCode());
		ReBake();
	}

	void ReBake()
	{
		foreach (var collider in colliders.Values)
		{
			collider.isTrigger = false;
		}
		surface.BuildNavMesh();
		foreach (var collider in colliders.Values)
		{
			collider.isTrigger = true;
		}
	}
}