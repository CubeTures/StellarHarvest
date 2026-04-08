using Sunspore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public sealed class TerrainTilemap : MonoBehaviour
{
	private static class T
	{
		public const int Isolated = 0;
		public const int EndN = 1;
		public const int EndE = 2;
		public const int CornerNE = 3;
		public const int CornerNE_Inner = 4;
		public const int EndS = 5;
		public const int StraightNS = 6;
		public const int CornerES = 7;
		public const int TeeNES = 8;
		public const int TeeNES_NE = 9;
		public const int CornerES_Inner = 10;
		public const int TeeNES_SE = 11;
		public const int TeeNES_NE_SE = 12;
		public const int EndW = 13;
		public const int CornerWN = 14;
		public const int StraightEW = 15;
		public const int TeeNEW = 16;
		public const int TeeNEW_NE = 17;
		public const int CornerSW = 18;
		public const int TeeNSW = 19;
		public const int TeeESW = 20;
		public const int Cross = 21;
		public const int Cross_NE = 22;
		public const int TeeESW_SE = 23;
		public const int Cross_SE = 24;
		public const int Cross_NE_SE = 25;
		public const int CornerSW_Inner = 26;
		public const int TeeNSW_SW = 27;
		public const int TeeESW_SW = 28;
		public const int Cross_SW = 29;
		public const int Cross_NE_SW = 30;
		public const int TeeESW_SE_SW = 31;
		public const int Cross_SE_SW = 32;
		public const int Cross_NE_SE_SW = 33;
		public const int CornerWN_Inner = 34;
		public const int TeeNEW_NW = 35;
		public const int TeeNEW_NE_NW = 36;
		public const int TeeNSW_NW = 37;
		public const int Cross_NW = 38;
		public const int Cross_NW_NE = 39;
		public const int Cross_SE_NW = 40;
		public const int Cross_NE_SE_NW = 41;
		public const int TeeNSW_SW_NW = 42;
		public const int Cross_SW_NW = 43;
		public const int Cross_NE_SW_NW = 44;
		public const int Cross_SE_SW_NW = 45;
		public const int Cross_All = 46;
		public const int Count = 47;
	}

	[SerializeField] private TileBase[] emptyTiles;
	[SerializeField] private TileBase[] tilledTiles; // exactly T.Count = 47 entries
	[SerializeField] private BoundsInt initialBounds;
	[Header("Procedural Terrain Props")]
	[SerializeField] private bool generatePropsOnAwake = true;
	[SerializeField, Range(0f, 1f)] private float propSpawnChance = 0.35f;
	[SerializeField, Range(0f, 2f)] private float noiseSpawnBoost = 1f;
	[SerializeField, Range(0f, 1f)] private float bushWeight = 0.45f;
	[SerializeField, Range(0f, 1f)] private float rockWeight = 0.2f;
	[SerializeField, Range(0f, 1f)] private float treeWeight = 0.35f;
	[SerializeField] private float featureNoiseScale = 0.12f;
	[SerializeField] private float typeNoiseScale = 0.08f;
	[SerializeField] private float colorNoiseScale = 0.1f;
	[SerializeField] private int noiseSeed = 12345;
	[SerializeField] private Transform propsParent;
	[SerializeField] private GameObject[] greenBushPrefabs;
	[SerializeField] private GameObject[] blueBushPrefabs;
	[SerializeField] private GameObject[] pinkBushPrefabs;
	[SerializeField] private GameObject[] darkRockPrefabs;
	[SerializeField] private GameObject[] lightRockPrefabs;
	[SerializeField] private GameObject[] greenTreePrefabs;
	[SerializeField] private GameObject[] blueTreePrefabs;
	[SerializeField] private GameObject[] pinkTreePrefabs;

	private static readonly Dictionary<int, int> CanonicalToIndex = BuildLookup();

	private Tilemap _tilemap;
	private readonly HashSet<Vector3Int> _tilledCells = new();
	private readonly List<GameObject> _spawnedProps = new();

	private static readonly Vector3Int[] AllNeighbourOffsets =
	{
		new( 0,  1, 0), // N
        new( 1,  1, 0), // NE
        new( 1,  0, 0), // E
        new( 1, -1, 0), // SE
        new( 0, -1, 0), // S
        new(-1, -1, 0), // SW
        new(-1,  0, 0), // W
        new(-1,  1, 0), // NW
    };


	// SET TO SAME NUMBERS AS PLACEMENTMANAGER SCALES
	private float zScale = 2.25f;
	private float xScale = 1.5f;

	public static TerrainTilemap Instance { get; private set; }

	private void Awake()
	{
		_tilemap = GetComponent<Tilemap>();
		FillWithEmpty();
		if (generatePropsOnAwake)
		{
			GeneratePropsFromNoise();
		}
		GameObject parentObject = this.transform.parent.gameObject;
		parentObject.transform.localScale = new Vector3(parentObject.transform.localScale.x * xScale, parentObject.transform.localScale.y * zScale, parentObject.transform.localScale.z);

		parentObject.transform.position = new Vector3((xScale / 2), parentObject.transform.position.y, (zScale / 2));

		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}


	private void FillWithEmpty()
	{
		if (emptyTiles == null || emptyTiles.Length == 0) return;
		foreach (var cell in initialBounds.allPositionsWithin)
		{
			_tilemap.SetTile(cell, emptyTiles[Random.Range(0, emptyTiles.Length)]);
		}
	}

	private void GeneratePropsFromNoise()
	{
		// Delete all props under propsParent
		if (propsParent != null)
		{
			foreach (Transform child in propsParent)
			{
				DestroyImmediate(child.gameObject);
			}
		}
		_spawnedProps.Clear();

		float totalWeight = Mathf.Max(0.0001f, bushWeight + rockWeight + treeWeight);
		float bushCutoff = bushWeight / totalWeight;
		float rockCutoff = bushCutoff + (rockWeight / totalWeight);

		foreach (var cell in initialBounds.allPositionsWithin)
		{
			float featureNoise = SampleNoise(cell, featureNoiseScale, 17.37f);

			// Dithered clusters: each tile rolls against a probability that increases with noise.
			float spawnProbability = Mathf.Clamp01(propSpawnChance * Mathf.Lerp(0.25f, 1f + noiseSpawnBoost, featureNoise));
			if (Random.value > spawnProbability)
			{
				continue;
			}

			float typeNoise = SampleNoise(cell, typeNoiseScale, 71.11f);
			// Shared biome color noise so bushes and trees group by the same color regions.
			float colorNoise = SampleNoise(cell, colorNoiseScale, 133.7f);

			GameObject prefabToSpawn = null;
			if (typeNoise < bushCutoff)
			{
				prefabToSpawn = PickByBiome(
					colorNoise,
					greenBushPrefabs,
					blueBushPrefabs,
					pinkBushPrefabs
				);
			}
			else if (typeNoise < rockCutoff)
			{
				float rockNoise = SampleNoise(cell, colorNoiseScale, 211.13f);
				prefabToSpawn = rockNoise < 0.5f
					? PickRandomPrefab(darkRockPrefabs)
					: PickRandomPrefab(lightRockPrefabs);
			}
			else
			{
				prefabToSpawn = PickByBiome(
					colorNoise,
					greenTreePrefabs,
					blueTreePrefabs,
					pinkTreePrefabs
				);
			}

			if (prefabToSpawn == null)
			{
				continue;
			}

			Vector3 cellWorld = _tilemap.CellToWorld(cell);
			Vector3 spawnPos = new Vector3(
				cellWorld.x + (xScale * 0.5f),
				transform.position.y,
				cellWorld.z + (zScale * 0.5f)
			);
			Transform parent = propsParent != null ? propsParent : transform;
			GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, parent);
			_spawnedProps.Add(spawned);
		}
	}

	[ContextMenu("Regenerate Terrain Props")]
	private void RegenerateTerrainProps()
	{
		if (_tilemap == null)
		{
			_tilemap = GetComponent<Tilemap>();
		}
		GeneratePropsFromNoise();
	}

	[ContextMenu("Clear Terrain Props")]
	private void ClearTerrainPropsFromContextMenu()
	{
		ClearSpawnedProps();
	}

	private void ClearSpawnedProps()
	{
		foreach (Transform child in propsParent)
		{
			DestroyImmediate(child.gameObject);
		}
		_spawnedProps.Clear();
	}

	private float SampleNoise(Vector3Int cell, float scale, float offset)
	{
		float scaledX = (cell.x + noiseSeed + offset) * Mathf.Max(0.0001f, scale);
		float scaledY = (cell.y + noiseSeed - offset) * Mathf.Max(0.0001f, scale);
		return Mathf.PerlinNoise(scaledX, scaledY);
	}

	private static GameObject PickByBiome(
		float noiseValue,
		GameObject[] green,
		GameObject[] blue,
		GameObject[] pink
	)
	{
		if (noiseValue < 0.33f)
		{
			return PickRandomPrefab(green);
		}
		if (noiseValue < 0.66f)
		{
			return PickRandomPrefab(blue);
		}
		return PickRandomPrefab(pink);
	}

	private static GameObject PickRandomPrefab(GameObject[] prefabs)
	{
		if (prefabs == null || prefabs.Length == 0)
		{
			return null;
		}

		int index = Random.Range(0, prefabs.Length);
		return prefabs[index];
	}

	public void TillAtWorldPosition(Vector3 worldPosition, float abilityRange, Vector3 playerPos)
	{
		var cell = _tilemap.WorldToCell(worldPosition);
		if (!initialBounds.Contains(cell)) return;

		Debug.Log("Tilled" + cell);

		if (!_tilledCells.Add(cell)) return;

		foreach (var offset in AllNeighbourOffsets)
		{
			var neighbour = cell + offset;
			if (neighbour == cell || _tilledCells.Contains(neighbour))
				RefreshCell(neighbour == cell ? cell : neighbour);
		}
		RefreshCell(cell);
	}

	public bool CheckRange(Vector3 worldPosition, float abilityRange, Vector3 playerPos)
	{
		var cell = _tilemap.WorldToCell(worldPosition);
		if (!initialBounds.Contains(cell)) return false;

		Vector3 cellWorldPos = _tilemap.CellToWorld(cell);

		// In CheckRange, change cos(30) to match the ring's zScale of 1.5f
		float cameraComp = 1f / 1.5f; // Match the ring's zScale

		Vector3 compensatedCellPos = new Vector3(
			cellWorldPos.x,
			1.5f,
			cellWorldPos.z * cameraComp  // divide by 1.5f, same as ring multiplying by 1.5f
		);

		Vector3 compensatedSize = new Vector3(xScale, 0f, zScale * cameraComp);

		Bounds cellBounds = new Bounds(compensatedCellPos + compensatedSize * 0.5f, compensatedSize);

		Vector3 compensatedPlayer = new Vector3(playerPos.x, 1.5f, playerPos.z * cameraComp);

		Vector3 closestPoint = cellBounds.ClosestPoint(compensatedPlayer);

		float distance = Vector3.Distance(compensatedPlayer, closestPoint);

		Debug.Log("Player Position: " + compensatedPlayer + " Cell: " + cell +
				  " Cell World Pos: " + cellWorldPos + " Closest Cell Edge: " + closestPoint +
				  " Distance: " + distance);

		return distance <= abilityRange;
	}

	public bool CheckTilled(Vector3 worldPos)
	{
		Debug.Log("Checking Till For Position: " + worldPos);

		var cell = _tilemap.WorldToCell(worldPos);
		Debug.Log($"CheckTilled pos={worldPos} → cell={cell}");
		Debug.Log($"TilledCells: {string.Join(", ", _tilledCells)}");

		if (_tilledCells.Contains(cell)) return true;

		Debug.Log("tilled");
		return false;
	}

	private void RefreshCell(Vector3Int cell)
	{
		Debug.Log("refresheed");
		if (!_tilledCells.Contains(cell)) return;
		var canonical = CanonicalMask(RawMask(cell));
		_tilemap.SetTile(cell, tilledTiles[CanonicalToIndex[canonical]]);
	}

	private int RawMask(Vector3Int cell)
	{
		var m = 0;
		if (_tilledCells.Contains(cell + new Vector3Int(0, 1, 0))) m |= 1;   // N
		if (_tilledCells.Contains(cell + new Vector3Int(1, 1, 0))) m |= 2;   // NE
		if (_tilledCells.Contains(cell + new Vector3Int(1, 0, 0))) m |= 4;   // E
		if (_tilledCells.Contains(cell + new Vector3Int(1, -1, 0))) m |= 8;   // SE
		if (_tilledCells.Contains(cell + new Vector3Int(0, -1, 0))) m |= 16;  // S
		if (_tilledCells.Contains(cell + new Vector3Int(-1, -1, 0))) m |= 32;  // SW
		if (_tilledCells.Contains(cell + new Vector3Int(-1, 0, 0))) m |= 64;  // W
		if (_tilledCells.Contains(cell + new Vector3Int(-1, 1, 0))) m |= 128; // NW
		return m;
	}

	private static int CanonicalMask(int m)
	{
		var n = (m & 1) != 0;
		var ne = (m & 2) != 0;
		var e = (m & 4) != 0;
		var se = (m & 8) != 0;
		var s = (m & 16) != 0;
		var sw = (m & 32) != 0;
		var w = (m & 64) != 0;
		var nw = (m & 128) != 0;

		var r = 0;
		if (n) r |= 1;
		if (ne && n && e) r |= 2;
		if (e) r |= 4;
		if (se && s && e) r |= 8;
		if (s) r |= 16;
		if (sw && s && w) r |= 32;
		if (w) r |= 64;
		if (nw && n && w) r |= 128;
		return r;
	}

	private static Dictionary<int, int> BuildLookup() => new()
	{
		[0b00000000] = T.Isolated,
		[0b00000001] = T.EndN,
		[0b00000100] = T.EndE,
		[0b00000101] = T.CornerNE,
		[0b00000111] = T.CornerNE_Inner,
		[0b00010000] = T.EndS,
		[0b00010001] = T.StraightNS,
		[0b00010100] = T.CornerES,
		[0b00010101] = T.TeeNES,
		[0b00010111] = T.TeeNES_NE,
		[0b00011100] = T.CornerES_Inner,
		[0b00011101] = T.TeeNES_SE,
		[0b00011111] = T.TeeNES_NE_SE,
		[0b01000000] = T.EndW,
		[0b01000001] = T.CornerWN,
		[0b01000100] = T.StraightEW,
		[0b01000101] = T.TeeNEW,
		[0b01000111] = T.TeeNEW_NE,
		[0b01010000] = T.CornerSW,
		[0b01010001] = T.TeeNSW,
		[0b01010100] = T.TeeESW,
		[0b01010101] = T.Cross,
		[0b01010111] = T.Cross_NE,
		[0b01011100] = T.TeeESW_SE,
		[0b01011101] = T.Cross_SE,
		[0b01011111] = T.Cross_NE_SE,
		[0b01110000] = T.CornerSW_Inner,
		[0b01110001] = T.TeeNSW_SW,
		[0b01110100] = T.TeeESW_SW,
		[0b01110101] = T.Cross_SW,
		[0b01110111] = T.Cross_NE_SW,
		[0b01111100] = T.TeeESW_SE_SW,
		[0b01111101] = T.Cross_SE_SW,
		[0b01111111] = T.Cross_NE_SE_SW,
		[0b11000001] = T.CornerWN_Inner,
		[0b11000101] = T.TeeNEW_NW,
		[0b11000111] = T.TeeNEW_NE_NW,
		[0b11010001] = T.TeeNSW_NW,
		[0b11010101] = T.Cross_NW,
		[0b11010111] = T.Cross_NW_NE,
		[0b11011101] = T.Cross_SE_NW,
		[0b11011111] = T.Cross_NE_SE_NW,
		[0b11110001] = T.TeeNSW_SW_NW,
		[0b11110101] = T.Cross_SW_NW,
		[0b11110111] = T.Cross_NE_SW_NW,
		[0b11111101] = T.Cross_SE_SW_NW,
		[0b11111111] = T.Cross_All,
	};
}