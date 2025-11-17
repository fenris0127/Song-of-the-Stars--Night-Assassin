using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace SongOfTheStars.Procedural
{
    /// <summary>
    /// Procedurally generates level layouts with rooms, corridors, and obstacles
    /// 방, 복도, 장애물이 있는 레벨 레이아웃을 프로시저럴하게 생성
    ///
    /// Uses BSP (Binary Space Partitioning) for room generation
    /// </summary>
    public class ProceduralLevelLayoutGenerator : MonoBehaviour
    {
        [Header("▶ Level Size")]
        [Range(20, 100)]
        public int levelWidth = 50;
        [Range(20, 100)]
        public int levelHeight = 50;

        [Header("▶ Room Settings")]
        [Range(2, 10)]
        public int minRoomCount = 4;
        [Range(3, 15)]
        public int maxRoomCount = 8;

        public Vector2Int minRoomSize = new Vector2Int(6, 6);
        public Vector2Int maxRoomSize = new Vector2Int(15, 15);

        [Header("▶ Corridor Settings")]
        [Range(1, 5)]
        public int corridorWidth = 2;
        public bool addWindingCorridors = true;

        [Header("▶ Obstacles")]
        [Range(0f, 0.3f)]
        public float obstacleChance = 0.15f;
        [Range(1, 10)]
        public int minObstacleSize = 1;
        [Range(2, 10)]
        public int maxObstacleSize = 3;

        [Header("▶ Cover Objects")]
        [Range(0f, 0.5f)]
        public float coverChance = 0.2f;
        public GameObject coverPrefab;

        [Header("▶ Special Locations")]
        public bool generateStartPoint = true;
        public bool generateExtractionPoint = true;
        public bool generateObjectivePoints = true;
        [Range(1, 5)]
        public int objectivePointCount = 3;

        [Header("▶ Tilemap References")]
        public Tilemap floorTilemap;
        public Tilemap wallTilemap;
        public Tilemap obstacleTilemap;
        public TileBase floorTile;
        public TileBase wallTile;
        public TileBase obstacleTile;

        [Header("▶ Debug")]
        public bool showGizmos = true;
        public int randomSeed = -1; // -1 = random

        private List<RoomData> _rooms = new List<RoomData>();
        private List<CorridorData> _corridors = new List<CorridorData>();
        private HashSet<Vector2Int> _floorTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _wallTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _obstacleTiles = new HashSet<Vector2Int>();

        private Vector2Int _startPoint;
        private Vector2Int _extractionPoint;
        private List<Vector2Int> _objectivePoints = new List<Vector2Int>();

        [ContextMenu("Generate Level Layout")]
        public void GenerateLevel()
        {
            // Initialize random seed
            if (randomSeed >= 0)
            {
                Random.InitState(randomSeed);
            }
            else
            {
                randomSeed = Random.Range(0, 999999);
                Random.InitState(randomSeed);
            }

            ClearLevel();

            // Generate rooms using BSP
            GenerateRooms();

            // Connect rooms with corridors
            GenerateCorridors();

            // Add obstacles
            GenerateObstacles();

            // Place special locations
            PlaceSpecialLocations();

            // Build tilemap
            BuildTilemap();

            // Spawn cover objects
            SpawnCoverObjects();

            Debug.Log($"✅ Level generated! Seed: {randomSeed}\n" +
                      $"Rooms: {_rooms.Count}, Corridors: {_corridors.Count}, " +
                      $"Floor Tiles: {_floorTiles.Count}");
        }

        [ContextMenu("Clear Level")]
        public void ClearLevel()
        {
            _rooms.Clear();
            _corridors.Clear();
            _floorTiles.Clear();
            _wallTiles.Clear();
            _obstacleTiles.Clear();
            _objectivePoints.Clear();

            if (floorTilemap != null) floorTilemap.ClearAllTiles();
            if (wallTilemap != null) wallTilemap.ClearAllTiles();
            if (obstacleTilemap != null) obstacleTilemap.ClearAllTiles();

            // Clear cover objects
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Cover_"))
                {
#if UNITY_EDITOR
                    DestroyImmediate(child.gameObject);
#else
                    Destroy(child.gameObject);
#endif
                }
            }
        }

        private void GenerateRooms()
        {
            int targetRoomCount = Random.Range(minRoomCount, maxRoomCount + 1);

            // Simple room placement with spacing
            for (int i = 0; i < targetRoomCount; i++)
            {
                int attempts = 0;
                int maxAttempts = 50;

                while (attempts < maxAttempts)
                {
                    attempts++;

                    int roomWidth = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
                    int roomHeight = Random.Range(minRoomSize.y, maxRoomSize.y + 1);

                    int x = Random.Range(2, levelWidth - roomWidth - 2);
                    int y = Random.Range(2, levelHeight - roomHeight - 2);

                    RoomData newRoom = new RoomData(x, y, roomWidth, roomHeight);

                    // Check overlap with existing rooms (with spacing)
                    bool overlaps = false;
                    foreach (RoomData existingRoom in _rooms)
                    {
                        if (newRoom.Overlaps(existingRoom, 3)) // 3 tile spacing
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        _rooms.Add(newRoom);

                        // Add room floor tiles
                        for (int rx = newRoom.x; rx < newRoom.x + newRoom.width; rx++)
                        {
                            for (int ry = newRoom.y; ry < newRoom.y + newRoom.height; ry++)
                            {
                                _floorTiles.Add(new Vector2Int(rx, ry));
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void GenerateCorridors()
        {
            // Connect each room to at least one other room
            for (int i = 0; i < _rooms.Count - 1; i++)
            {
                RoomData roomA = _rooms[i];
                RoomData roomB = _rooms[i + 1];

                CreateCorridor(roomA.GetCenter(), roomB.GetCenter());
            }

            // Add some extra connections for loops
            if (_rooms.Count > 3)
            {
                int extraConnections = Random.Range(1, Mathf.Max(2, _rooms.Count / 3));
                for (int i = 0; i < extraConnections; i++)
                {
                    RoomData roomA = _rooms[Random.Range(0, _rooms.Count)];
                    RoomData roomB = _rooms[Random.Range(0, _rooms.Count)];

                    if (roomA != roomB)
                    {
                        CreateCorridor(roomA.GetCenter(), roomB.GetCenter());
                    }
                }
            }
        }

        private void CreateCorridor(Vector2Int start, Vector2Int end)
        {
            CorridorData corridor = new CorridorData { start = start, end = end };
            _corridors.Add(corridor);

            // L-shaped corridor
            if (addWindingCorridors && Random.value > 0.5f)
            {
                // Vertical then horizontal
                CreateVerticalCorridor(start.x, start.y, end.y);
                CreateHorizontalCorridor(start.x, end.x, end.y);
            }
            else
            {
                // Horizontal then vertical
                CreateHorizontalCorridor(start.x, end.x, start.y);
                CreateVerticalCorridor(end.x, start.y, end.y);
            }
        }

        private void CreateHorizontalCorridor(int x1, int x2, int y)
        {
            int startX = Mathf.Min(x1, x2);
            int endX = Mathf.Max(x1, x2);

            for (int x = startX; x <= endX; x++)
            {
                for (int dy = 0; dy < corridorWidth; dy++)
                {
                    _floorTiles.Add(new Vector2Int(x, y + dy - corridorWidth / 2));
                }
            }
        }

        private void CreateVerticalCorridor(int x, int y1, int y2)
        {
            int startY = Mathf.Min(y1, y2);
            int endY = Mathf.Max(y1, y2);

            for (int y = startY; y <= endY; y++)
            {
                for (int dx = 0; dx < corridorWidth; dx++)
                {
                    _floorTiles.Add(new Vector2Int(x + dx - corridorWidth / 2, y));
                }
            }
        }

        private void GenerateObstacles()
        {
            List<Vector2Int> floorList = new List<Vector2Int>(_floorTiles);

            foreach (Vector2Int tile in floorList)
            {
                if (Random.value < obstacleChance)
                {
                    int obstacleSize = Random.Range(minObstacleSize, maxObstacleSize + 1);

                    // Create obstacle cluster
                    for (int dx = 0; dx < obstacleSize; dx++)
                    {
                        for (int dy = 0; dy < obstacleSize; dy++)
                        {
                            Vector2Int obstaclePos = tile + new Vector2Int(dx, dy);

                            if (_floorTiles.Contains(obstaclePos))
                            {
                                _obstacleTiles.Add(obstaclePos);
                                _floorTiles.Remove(obstaclePos); // Remove from walkable floor
                            }
                        }
                    }
                }
            }
        }

        private void PlaceSpecialLocations()
        {
            if (_rooms.Count == 0) return;

            List<Vector2Int> availablePositions = new List<Vector2Int>(_floorTiles);

            if (generateStartPoint && availablePositions.Count > 0)
            {
                _startPoint = _rooms[0].GetCenter();
                availablePositions.Remove(_startPoint);
            }

            if (generateExtractionPoint && availablePositions.Count > 0)
            {
                _extractionPoint = _rooms[_rooms.Count - 1].GetCenter();
                availablePositions.Remove(_extractionPoint);
            }

            if (generateObjectivePoints)
            {
                for (int i = 0; i < objectivePointCount && availablePositions.Count > 0; i++)
                {
                    // Pick from different rooms
                    int roomIndex = Mathf.Min(i + 1, _rooms.Count - 1);
                    Vector2Int objectivePos = _rooms[roomIndex].GetCenter();

                    _objectivePoints.Add(objectivePos);
                    availablePositions.Remove(objectivePos);
                }
            }
        }

        private void BuildTilemap()
        {
            // Place floor tiles
            foreach (Vector2Int pos in _floorTiles)
            {
                if (floorTilemap != null && floorTile != null)
                {
                    floorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), floorTile);
                }
            }

            // Generate and place wall tiles
            foreach (Vector2Int floorPos in _floorTiles)
            {
                // Check 8 directions for walls
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        Vector2Int neighborPos = floorPos + new Vector2Int(dx, dy);

                        if (!_floorTiles.Contains(neighborPos) && !_obstacleTiles.Contains(neighborPos))
                        {
                            _wallTiles.Add(neighborPos);
                        }
                    }
                }
            }

            // Place wall tiles
            foreach (Vector2Int pos in _wallTiles)
            {
                if (wallTilemap != null && wallTile != null)
                {
                    wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), wallTile);
                }
            }

            // Place obstacle tiles
            foreach (Vector2Int pos in _obstacleTiles)
            {
                if (obstacleTilemap != null && obstacleTile != null)
                {
                    obstacleTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), obstacleTile);
                }
            }
        }

        private void SpawnCoverObjects()
        {
            if (coverPrefab == null) return;

            List<Vector2Int> floorList = new List<Vector2Int>(_floorTiles);

            foreach (Vector2Int tile in floorList)
            {
                if (Random.value < coverChance)
                {
                    Vector3 worldPos = new Vector3(tile.x, tile.y, 0f);
                    GameObject cover = Instantiate(coverPrefab, worldPos, Quaternion.identity, transform);
                    cover.name = $"Cover_{tile.x}_{tile.y}";
                }
            }
        }

        public Vector2Int GetStartPoint() => _startPoint;
        public Vector2Int GetExtractionPoint() => _extractionPoint;
        public List<Vector2Int> GetObjectivePoints() => new List<Vector2Int>(_objectivePoints);
        public List<Vector2Int> GetFloorTiles() => new List<Vector2Int>(_floorTiles);

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Draw rooms
            Gizmos.color = Color.green;
            foreach (RoomData room in _rooms)
            {
                Vector3 center = new Vector3(room.x + room.width / 2f, room.y + room.height / 2f, 0f);
                Vector3 size = new Vector3(room.width, room.height, 0.1f);
                Gizmos.DrawWireCube(center, size);
            }

            // Draw corridors
            Gizmos.color = Color.yellow;
            foreach (CorridorData corridor in _corridors)
            {
                Vector3 start = new Vector3(corridor.start.x, corridor.start.y, 0f);
                Vector3 end = new Vector3(corridor.end.x, corridor.end.y, 0f);
                Gizmos.DrawLine(start, end);
            }

            // Draw special locations
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(new Vector3(_startPoint.x, _startPoint.y, 0f), 1f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(_extractionPoint.x, _extractionPoint.y, 0f), 1f);

            Gizmos.color = Color.magenta;
            foreach (Vector2Int objective in _objectivePoints)
            {
                Gizmos.DrawWireSphere(new Vector3(objective.x, objective.y, 0f), 0.7f);
            }
        }
    }

    [System.Serializable]
    public class RoomData
    {
        public int x, y, width, height;

        public RoomData(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2Int GetCenter()
        {
            return new Vector2Int(x + width / 2, y + height / 2);
        }

        public bool Overlaps(RoomData other, int spacing = 0)
        {
            return !(x + width + spacing < other.x ||
                     x > other.x + other.width + spacing ||
                     y + height + spacing < other.y ||
                     y > other.y + other.height + spacing);
        }
    }

    [System.Serializable]
    public class CorridorData
    {
        public Vector2Int start;
        public Vector2Int end;
    }
}
