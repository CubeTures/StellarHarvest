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

	private static readonly Dictionary<int, int> CanonicalToIndex = BuildLookup();

	private Tilemap _tilemap;
	private readonly HashSet<Vector3Int> _tilledCells = new();

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

    public static TerrainTilemap Instance { get; private set; }

    private void Awake()
	{
		_tilemap = GetComponent<Tilemap>();
		FillWithEmpty();
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

	public void TillAtWorldPosition(Vector3 worldPosition, float abilityRange, Vector3 playerPos)
	{
		var cell = _tilemap.WorldToCell(worldPosition);
		if (!initialBounds.Contains(cell)) return;

        // 1. Get the cell's world bounds
        Vector3 cellSize = _tilemap.cellSize;
        Bounds cellBounds = new Bounds(cell, cellSize);

        // 2. Find the closest point on the cell's bounds to the player
        Vector3 closestPoint = cellBounds.ClosestPoint(playerPos);
		closestPoint.z = closestPoint.y;
		closestPoint.y = 1.5f;

        // 3. Calculate the distance
        float distance = Vector3.Distance(playerPos, closestPoint);

		Debug.Log(distance + " " + abilityRange);

		if (distance > abilityRange) return;

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

        // 1. Get the cell's world bounds
        Vector3 cellSize = _tilemap.cellSize;
        Bounds cellBounds = new Bounds(cell, cellSize);

        // 2. Find the closest point on the cell's bounds to the player
        Vector3 closestPoint = cellBounds.ClosestPoint(playerPos) + new Vector3(0, 1.5f, 0);
        closestPoint.z = closestPoint.y;
        closestPoint.y = 1.5f;

        // 3. Calculate the distance
        float distance = Vector3.Distance(playerPos, closestPoint);

        Debug.Log("Player Position: " + playerPos + " Cell: " + cell + " Closest Cell Edge: " + closestPoint + " Distance: " + distance);

        if (distance <= abilityRange)
		{
			return true;
		}

		return false;
    }

	public bool CheckTilled(Vector3 worldPos)
	{
		Debug.Log(worldPos);

		Vector3Int cell = new Vector3Int((int)worldPos.x, (int)worldPos.z, 0);

		Debug.Log(cell);

		if (!_tilledCells.Contains(cell)) return true;

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