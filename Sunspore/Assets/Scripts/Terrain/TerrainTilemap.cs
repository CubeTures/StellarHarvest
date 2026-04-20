using System.Collections.Generic;
using Sunspore;
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

	[SerializeField]
	private TileBase[] emptyTiles;

	[SerializeField]
	private TileBase[] tilledTiles; // exactly T.Count = 47 entries

	[SerializeField]
	private BoundsInt initialBounds;

	[Header("Procedural Terrain Props")]
	[SerializeField]
	private bool generatePropsOnAwake = true;

	[SerializeField, Range(0f, 1f)]
	private float propSpawnChance = 0.35f;

	[SerializeField, Range(0f, 2f)]
	private float noiseSpawnBoost = 1f;
	[SerializeField]
	private Vector2 noiseSpawnYOffset = new Vector2(-0.1f, 0.1f);

	[SerializeField, Range(0f, 1f)]
	private float bushWeight = 0.45f;

	[SerializeField, Range(0f, 1f)]
	private float rockWeight = 0.2f;

	[SerializeField, Range(0f, 1f)]
	private float treeWeight = 0.35f;

	[SerializeField, Range(0f, 2f)]
	private float bushDensityMultiplier = 1f;

	[SerializeField, Range(0f, 2f)]
	private float rockDensityMultiplier = 1f;

	[SerializeField, Range(0f, 2f)]
	private float treeDensityMultiplier = 1f;

	[Header("Bush-only clusters")]
	[SerializeField]
	private bool bushUseClusterMask = true;

	[SerializeField]
	private float bushClusterNoiseScale = 0.022f;

	[SerializeField, Range(0f, 1f)]
	private float bushClusterDetailBlend = 0.4f;

	[SerializeField, Range(0f, 1f)]
	private float bushClusterSoftLow = 0.38f;

	[SerializeField, Range(0f, 1f)]
	private float bushClusterSoftHigh = 0.72f;

	[Tooltip("Spawn chance multiplier for bushes outside bush patches (solo bushes). Lower = fewer lone bushes.")]
	[SerializeField, Range(0f, 1f)]
	private float bushOutsideClusterSpawnChance = 0.07f;

	[Tooltip("Extra bush instances spawned in a tight ring around each bush roll so sprites overlap.")]
	[SerializeField]
	private int bushOverlapExtraSpawns = 5;

	[Tooltip("World XZ radius for overlap spawns (smaller = tighter pile).")]
	[SerializeField]
	private float bushOverlapRadius = 0.22f;

	[SerializeField]
	private float featureNoiseScale = 0.12f;

	[SerializeField]
	private bool usePropClumps = true;

	[SerializeField]
	private float clumpNoiseScale = 0.028f;

	[SerializeField, Range(0f, 1f)]
	private float clumpDetailBlend = 0.45f;

	[SerializeField, Range(0f, 1f)]
	private float clumpSoftLow = 0.32f;

	[SerializeField, Range(0f, 1f)]
	private float clumpSoftHigh = 0.68f;

	[SerializeField, Range(0f, 1f)]
	private float clumpOutsideDensity = 0.04f;

	[SerializeField]
	private bool useDensityFilter = true;

	[SerializeField]
	private float densityNoiseScale = 0.06f;

	[SerializeField, Range(0f, 1f)]
	private float densityMin = 0.35f;

	[SerializeField, Range(0f, 1f)]
	private float densityMax = 1f;

	[SerializeField]
	private float typeNoiseScale = 0.08f;

	[SerializeField]
	private float colorNoiseScale = 0.1f;

	[SerializeField]
	private int noiseSeed = 12345;

	[SerializeField]
	private Transform propsParent;

	[SerializeField]
	private GameObject[] greenBushPrefabs;

	[SerializeField]
	private GameObject[] blueBushPrefabs;

	[SerializeField]
	private GameObject[] pinkBushPrefabs;

	[SerializeField]
	private GameObject[] darkRockPrefabs;

	[SerializeField]
	private GameObject[] lightRockPrefabs;

	[SerializeField]
	private GameObject[] greenTreePrefabs;

	[SerializeField]
	private GameObject[] blueTreePrefabs;

	[SerializeField]
	private GameObject[] pinkTreePrefabs;

	[Tooltip("Approximate number of placement tries vs. count of cells in initialBounds (1 ≈ one try per cell).")]
	[SerializeField, Range(0.25f, 8f)]
	private float worldPropSampleAttemptsMultiplier = 1f;

	private static readonly Dictionary<int, int> CanonicalToIndex = BuildLookup();

	private Tilemap _tilemap;
	private readonly HashSet<Vector3Int> _tilledCells = new();
	private readonly List<GameObject> _spawnedProps = new();

	private static readonly Vector3Int[] AllNeighbourOffsets =
	{
		new(0, 1, 0), // N
        new(1, 1, 0), // NE
        new(1, 0, 0), // E
        new(1, -1, 0), // SE
        new(0, -1, 0), // S
        new(-1, -1, 0), // SW
        new(-1, 0, 0), // W
        new(-1, 1, 0), // NW
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
		parentObject.transform.localScale = new Vector3(
			parentObject.transform.localScale.x * xScale,
			parentObject.transform.localScale.y * zScale,
			parentObject.transform.localScale.z
		);

		parentObject.transform.position = new Vector3(
			(xScale / 2),
			parentObject.transform.position.y,
			(zScale / 2)
		);

		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void FillWithEmpty()
	{
		if (emptyTiles == null || emptyTiles.Length == 0)
			return;
		foreach (var cell in initialBounds.allPositionsWithin)
		{
			_tilemap.SetTile(cell, emptyTiles[Random.Range(0, emptyTiles.Length)]);
		}
	}

	private void GeneratePropsFromNoise()
	{
		ClearSpawnedProps();

		GetTerrainWorldXZExtents(out float minX, out float maxX, out float minZ, out float maxZ);
		if (minX > maxX || minZ > maxZ)
		{
			return;
		}

		float totalWeight = Mathf.Max(0.0001f, bushWeight + rockWeight + treeWeight);
		float bushCutoff = bushWeight / totalWeight;
		float rockCutoff = bushCutoff + (rockWeight / totalWeight);

		long cellCount = (long)initialBounds.size.x * initialBounds.size.y * initialBounds.size.z;
		int attempts = (int)Mathf.Clamp(
			Mathf.RoundToInt(cellCount * worldPropSampleAttemptsMultiplier),
			1,
			int.MaxValue - 1
		);

		Transform parent = propsParent != null ? propsParent : transform;

		for (int i = 0; i < attempts; i++)
		{
			float wx = Random.Range(minX, maxX);
			float wz = Random.Range(minZ, maxZ);
			if (!IsWorldPositionInInitialBounds(wx, wz))
			{
				continue;
			}

			float featureNoise = SampleNoiseWorld(wx, wz, featureNoiseScale, 17.37f);
			float densityNoise = SampleNoiseWorld(wx, wz, densityNoiseScale, 19.91f);

			if (useDensityFilter && (densityNoise < densityMin || densityNoise > densityMax))
			{
				continue;
			}

			float typeNoise = SampleNoiseWorld(wx, wz, typeNoiseScale, 71.11f);
			float colorNoise = SampleNoiseWorld(wx, wz, colorNoiseScale, 133.7f);

			float densityMultiplier = 1f;

			GameObject prefabToSpawn = null;
			if (typeNoise < bushCutoff)
			{
				densityMultiplier = bushDensityMultiplier;
				prefabToSpawn = PickByBiome(
					colorNoise,
					greenBushPrefabs,
					blueBushPrefabs,
					pinkBushPrefabs
				);
			}
			else if (typeNoise < rockCutoff)
			{
				densityMultiplier = rockDensityMultiplier;
				float rockNoise = SampleNoiseWorld(wx, wz, colorNoiseScale, 211.13f);
				prefabToSpawn =
					rockNoise < 0.5f
						? PickRandomPrefab(darkRockPrefabs)
						: PickRandomPrefab(lightRockPrefabs);
			}
			else
			{
				densityMultiplier = treeDensityMultiplier;
				prefabToSpawn = PickByBiome(
					colorNoise,
					greenTreePrefabs,
					blueTreePrefabs,
					pinkTreePrefabs
				);
			}

			float clumpStrength = 1f;
			if (usePropClumps)
			{
				float clumpA = SampleNoiseWorld(wx, wz, clumpNoiseScale, 301.71f);
				float clumpB = SampleNoiseWorld(wx, wz, clumpNoiseScale * 1.85f, 409.13f);
				float clumpCombined = Mathf.Lerp(clumpA, (clumpA + clumpB) * 0.5f, clumpDetailBlend);
				float softLo = Mathf.Min(clumpSoftLow, clumpSoftHigh);
				float softHi = Mathf.Max(clumpSoftLow, clumpSoftHigh);
				clumpStrength = Mathf.SmoothStep(softLo, softHi, clumpCombined);
				clumpStrength = Mathf.Clamp01(clumpStrength);
			}

			float bushClusterFactor = 1f;
			if (typeNoise < bushCutoff && bushUseClusterMask)
			{
				float bcA = SampleNoiseWorld(wx, wz, bushClusterNoiseScale, 501.2f);
				float bcB = SampleNoiseWorld(wx, wz, bushClusterNoiseScale * 1.72f, 608.5f);
				float bcCombined = Mathf.Lerp(bcA, (bcA + bcB) * 0.5f, bushClusterDetailBlend);
				float bcLo = Mathf.Min(bushClusterSoftLow, bushClusterSoftHigh);
				float bcHi = Mathf.Max(bushClusterSoftLow, bushClusterSoftHigh);
				float bushPatch = Mathf.Clamp01(Mathf.SmoothStep(bcLo, bcHi, bcCombined));
				bushClusterFactor = Mathf.Lerp(bushOutsideClusterSpawnChance, 1f, bushPatch);
			}

			float spawnProbability = Mathf.Clamp01(
				propSpawnChance
				* Mathf.Lerp(clumpOutsideDensity, 1f, clumpStrength)
				* Mathf.Lerp(0.25f, 1f + noiseSpawnBoost, featureNoise)
				* Mathf.Max(0f, densityMultiplier)
				* bushClusterFactor
			);
			if (Random.value > spawnProbability)
			{
				continue;
			}

			if (prefabToSpawn == null)
			{
				continue;
			}

			bool isRock = typeNoise >= bushCutoff && typeNoise < rockCutoff;
			float yOffset = isRock ? 0.1f : Random.Range(noiseSpawnYOffset.x, noiseSpawnYOffset.y);
			Vector3 spawnPos = new Vector3(wx, transform.position.y + yOffset, wz);
			GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, parent);
			ApplySpriteSortingByY(spawned);
			_spawnedProps.Add(spawned);

			bool isBush = typeNoise < bushCutoff;
			if (isBush && bushOverlapExtraSpawns > 0 && bushOverlapRadius > 0f)
			{
				float r = bushOverlapRadius;
				for (int b = 0; b < bushOverlapExtraSpawns; b++)
				{
					Vector2 offset = Random.insideUnitCircle * r;
					float ox = wx + offset.x;
					float oz = wz + offset.y;
					if (!IsWorldPositionInInitialBounds(ox, oz))
					{
						continue;
					}

					float colorJitter = Mathf.Clamp01(colorNoise + Random.Range(-0.09f, 0.09f));
					GameObject overlapPrefab = PickByBiome(
						colorJitter,
						greenBushPrefabs,
						blueBushPrefabs,
						pinkBushPrefabs
					);
					if (overlapPrefab == null)
					{
						continue;
					}

					Vector3 extraPos = new Vector3(ox, transform.position.y, oz);
					GameObject extra = Instantiate(overlapPrefab, extraPos, Quaternion.identity, parent);
					ApplySpriteSortingByY(extra);
					_spawnedProps.Add(extra);
				}
			}
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
		Transform parent = propsParent != null ? propsParent : transform;
		if (parent == null)
		{
			return;
		}

		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			GameObject child = parent.GetChild(i).gameObject;
			if (Application.isPlaying)
			{
				Destroy(child);
			}
			else
			{
				DestroyImmediate(child);
			}
		}
		_spawnedProps.Clear();
	}

	private void GetTerrainWorldXZExtents(out float minX, out float maxX, out float minZ, out float maxZ)
	{
		minX = float.MaxValue;
		maxX = float.MinValue;
		minZ = float.MaxValue;
		maxZ = float.MinValue;

		Vector3 cellSize = _tilemap.cellSize;
		Vector3 scale = _tilemap.transform.lossyScale;
		float halfX = 0.5f * Mathf.Abs(cellSize.x * scale.x);
		float halfZ = 0.5f * Mathf.Abs(cellSize.z * scale.z);

		foreach (var cell in initialBounds.allPositionsWithin)
		{
			Vector3 c = _tilemap.GetCellCenterWorld(cell);
			minX = Mathf.Min(minX, c.x - halfX);
			maxX = Mathf.Max(maxX, c.x + halfX);
			minZ = Mathf.Min(minZ, c.z - halfZ);
			maxZ = Mathf.Max(maxZ, c.z + halfZ);
		}
	}

	private bool IsWorldPositionInInitialBounds(float worldX, float worldZ)
	{
		Vector3 world = new Vector3(worldX, transform.position.y, worldZ);
		Vector3Int cell = _tilemap.WorldToCell(world);
		return initialBounds.Contains(cell);
	}

	/// <summary>
	/// Perlin noise sampled from continuous world XZ (mapped through fractional tile coordinates so scales match grid-sized noise).
	/// </summary>
	private float SampleNoiseWorld(float worldX, float worldZ, float scale, float offset)
	{
		Vector3 cellSize = _tilemap.cellSize;
		Vector3 lossyScale = _tilemap.transform.lossyScale;
		float lx = worldX / Mathf.Max(0.001f, Mathf.Abs(cellSize.x * lossyScale.x));
		float lz = worldZ / Mathf.Max(0.001f, Mathf.Abs(cellSize.z * lossyScale.z));
		float scaledX = (lx + noiseSeed + offset) * Mathf.Max(0.0001f, scale);
		float scaledY = (lz + noiseSeed - offset) * Mathf.Max(0.0001f, scale);
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

	private static void ApplySpriteSortingByY(GameObject root)
	{
		if (root == null)
		{
			return;
		}

		SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
		foreach (SpriteRenderer sr in renderers)
		{
			// Lower world Z renders in front (higher sorting order).
			sr.sortingOrder = Mathf.RoundToInt(-sr.transform.position.z * 100f) - 50 * 100;
		}
	}

	public void TillAtWorldPosition(Vector3 worldPosition, float abilityRange, Vector3 playerPos)
	{
		var cell = _tilemap.WorldToCell(worldPosition);
		if (!initialBounds.Contains(cell))
			return;

		Debug.Log("Tilled" + cell);

		if (!_tilledCells.Add(cell))
			return;

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

		// 1. Get the exact center of the cell in world space
		Vector3 cellCenter = _tilemap.GetCellCenterWorld(cell);

		// 2. Flatten both positions to the gameplay plane
		cellCenter.y = 1.5f;
		Vector3 pPos = playerPos;
		pPos.y = 1.5f;

		// 3. Calculate distance accounting for the camera's Z perspective shift
		Vector3 difference = cellCenter - pPos;
		difference.z /= 1.5f; // Scale Z to match X visually
		float distance = difference.magnitude;

		// 4. Check for Grass in the cell
		bool hasGrass = HasGrassInCell(cell);

		Debug.Log($"Cell: {cell} | Target Dist: {distance} | Range: {abilityRange} | Has Grass: {hasGrass}");

		return distance <= abilityRange && !hasGrass;
	}

	public bool CheckTilled(Vector3 worldPos)
	{
		Debug.Log("Checking Till For Position: " + worldPos);

		var cell = _tilemap.WorldToCell(worldPos);
		Debug.Log($"CheckTilled pos={worldPos} → cell={cell}");
		Debug.Log($"TilledCells: {string.Join(", ", _tilledCells)}");

		if (_tilledCells.Contains(cell))
			return true;

		Debug.Log("tilled");
		return false;
	}

	// Reusable helper method to check for any script in a given tilemap cell
	public bool HasGrassInCell(Vector3Int cell)
	{
		Vector3 cellCenter = _tilemap.GetCellCenterWorld(cell);

		// Half-extents match the size of the cell. 
		// Scaling Y to 0.5f creates a 1-unit tall detection box.
		Vector3 halfExtents = new Vector3(_tilemap.cellSize.x / 2f, 0.5f, _tilemap.cellSize.y / 2f);

		Collider[] colliders = Physics.OverlapBox(cellCenter, halfExtents, Quaternion.identity);

		foreach (var col in colliders)
		{
			if (col.GetComponent<Grass>() != null)
			{
				return true;
			}
		}

		return false;
	}

	private void RefreshCell(Vector3Int cell)
	{
		Debug.Log("refresheed");
		if (!_tilledCells.Contains(cell))
			return;
		var canonical = CanonicalMask(RawMask(cell));
		_tilemap.SetTile(cell, tilledTiles[CanonicalToIndex[canonical]]);
	}

	private int RawMask(Vector3Int cell)
	{
		var m = 0;
		if (_tilledCells.Contains(cell + new Vector3Int(0, 1, 0)))
			m |= 1; // N
		if (_tilledCells.Contains(cell + new Vector3Int(1, 1, 0)))
			m |= 2; // NE
		if (_tilledCells.Contains(cell + new Vector3Int(1, 0, 0)))
			m |= 4; // E
		if (_tilledCells.Contains(cell + new Vector3Int(1, -1, 0)))
			m |= 8; // SE
		if (_tilledCells.Contains(cell + new Vector3Int(0, -1, 0)))
			m |= 16; // S
		if (_tilledCells.Contains(cell + new Vector3Int(-1, -1, 0)))
			m |= 32; // SW
		if (_tilledCells.Contains(cell + new Vector3Int(-1, 0, 0)))
			m |= 64; // W
		if (_tilledCells.Contains(cell + new Vector3Int(-1, 1, 0)))
			m |= 128; // NW
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
		if (n)
			r |= 1;
		if (ne && n && e)
			r |= 2;
		if (e)
			r |= 4;
		if (se && s && e)
			r |= 8;
		if (s)
			r |= 16;
		if (sw && s && w)
			r |= 32;
		if (w)
			r |= 64;
		if (nw && n && w)
			r |= 128;
		return r;
	}

	private static Dictionary<int, int> BuildLookup() =>
		new()
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
