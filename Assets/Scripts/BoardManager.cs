using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private CellData[,] m_BoardData;
    private List<Vector2Int> m_EmptyCellsList;

    private List<FoodObject> m_FoodPool = new();
    private List<WallObject> m_WallPool = new();
    private List<Enemy> m_EnemyPool = new();
    private ExitCellObject m_ExitObject;

    public int Width = 8;
    public int Height = 8;

    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    public FoodObject FoodPrefab;
    public FoodObject LargeFoodPrefab;
    public WallObject WallPrefab;
    public ExitCellObject ExitCellPrefab;
    public Enemy EnemyPrefab;

    [SerializeField] private int foodPoolSize = 8;
    [SerializeField] private int wallPoolSize = 12;
    [SerializeField] private int enemyPoolSize = 8;

    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        PreparePools();

        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                m_BoardData[x, y] = new CellData();

                bool border =
                    x == 0 || y == 0 ||
                    x == Width - 1 || y == Height - 1;

                Tile tile;

                if (border)
                {
                    tile =
                        WallTiles[
                            Random.Range(0, WallTiles.Length)
                        ];

                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile =
                        GroundTiles[
                            Random.Range(0, GroundTiles.Length)
                        ];

                    m_BoardData[x, y].Passable = true;

                    m_EmptyCellsList.Add(
                        new Vector2Int(x, y)
                    );
                }

                m_Tilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    tile
                );
            }
        }

        m_EmptyCellsList.Remove(
            new Vector2Int(1, 1)
        );

        Vector2Int exitPosition =
            new Vector2Int(Width - 2, Height - 2);

        AddObject(
            GetExitObject(),
            exitPosition
        );

        m_EmptyCellsList.Remove(exitPosition);

        GenerateWalls();
        GenerateEnemies();
        GenerateFood();
    }

    private void PreparePools()
    {
        while (m_FoodPool.Count < foodPoolSize)
        {
            FoodObject prefab = FoodPrefab;

            if (LargeFoodPrefab != null &&
                m_FoodPool.Count % 2 == 1)
            {
                prefab = LargeFoodPrefab;
            }

            FoodObject food =
                Instantiate(prefab);

            food.gameObject.SetActive(false);
            m_FoodPool.Add(food);
        }

        while (m_WallPool.Count < wallPoolSize)
        {
            WallObject wall =
                Instantiate(WallPrefab);

            wall.gameObject.SetActive(false);
            m_WallPool.Add(wall);
        }

        while (m_EnemyPool.Count < enemyPoolSize)
        {
            Enemy enemy =
                Instantiate(EnemyPrefab);

            enemy.gameObject.SetActive(false);
            m_EnemyPool.Add(enemy);
        }

        if (m_ExitObject == null)
        {
            m_ExitObject =
                Instantiate(ExitCellPrefab);

            m_ExitObject.gameObject.SetActive(false);
        }
    }

    public void Clean()
    {
        foreach (FoodObject food in m_FoodPool)
            food.gameObject.SetActive(false);

        foreach (WallObject wall in m_WallPool)
            wall.gameObject.SetActive(false);

        foreach (Enemy enemy in m_EnemyPool)
            enemy.gameObject.SetActive(false);

        if (m_ExitObject != null)
            m_ExitObject.gameObject.SetActive(false);

        if (m_Tilemap != null)
            m_Tilemap.ClearAllTiles();
    }

    private ExitCellObject GetExitObject()
    {
        m_ExitObject.gameObject.SetActive(true);
        return m_ExitObject;
    }

    private FoodObject GetFoodObject()
    {
        List<FoodObject> available =
            new List<FoodObject>();

        foreach (FoodObject food in m_FoodPool)
        {
            if (!food.gameObject.activeSelf)
                available.Add(food);
        }

        if (available.Count == 0)
            return null;

        FoodObject selected =
            available[
                Random.Range(0, available.Count)
            ];

        selected.gameObject.SetActive(true);
        return selected;
    }

    private WallObject GetWallObject()
    {
        foreach (WallObject wall in m_WallPool)
        {
            if (!wall.gameObject.activeSelf)
            {
                wall.gameObject.SetActive(true);
                return wall;
            }
        }

        return null;
    }

    private Enemy GetEnemyObject()
    {
        foreach (Enemy enemy in m_EnemyPool)
        {
            if (!enemy.gameObject.activeSelf)
            {
                enemy.gameObject.SetActive(true);
                return enemy;
            }
        }

        return null;
    }

    private void AddObject(
        CellObject cellObject,
        Vector2Int cell
    )
    {
        if (cellObject == null)
            return;

        cellObject.gameObject.SetActive(true);
        cellObject.transform.position =
            CellToWorld(cell);

        m_BoardData[cell.x, cell.y]
            .ContainedObject = cellObject;

        cellObject.Init(cell);
    }

    private void GenerateWalls()
    {
        int level =
            GameManager.Instance.CurrentLevel;

        int wallCount =
            Mathf.Clamp(
                5 + level,
                6,
                wallPoolSize
            );

        for (int i = 0; i < wallCount; i++)
        {
            AddObject(
                GetWallObject(),
                RemoveRandomEmptyCell()
            );
        }
    }

    private void GenerateEnemies()
    {
        int level =
            GameManager.Instance.CurrentLevel;

        int enemyCount =
            Mathf.Clamp(
                1 + level,
                2,
                enemyPoolSize
            );

        for (int i = 0; i < enemyCount; i++)
        {
            AddObject(
                GetEnemyObject(),
                RemoveRandomEmptyCell()
            );
        }
    }

    private void GenerateFood()
    {
        for (int i = 0; i < 5; i++)
        {
            AddObject(
                GetFoodObject(),
                RemoveRandomEmptyCell()
            );
        }
    }

    private Vector2Int RemoveRandomEmptyCell()
    {
        int index =
            Random.Range(
                0,
                m_EmptyCellsList.Count
            );

        Vector2Int cell =
            m_EmptyCellsList[index];

        m_EmptyCellsList.RemoveAt(index);
        return cell;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return m_Grid.GetCellCenterWorld(
            (Vector3Int)cell
        );
    }

    public void SetCellTile(
        Vector2Int cell,
        Tile tile
    )
    {
        m_Tilemap.SetTile(
            new Vector3Int(cell.x, cell.y, 0),
            tile
        );
    }

    public Tile GetCellTile(Vector2Int cell)
    {
        return m_Tilemap.GetTile<Tile>(
            new Vector3Int(cell.x, cell.y, 0)
        );
    }

    public CellData GetCellData(Vector2Int cell)
    {
        if (cell.x < 0 ||
            cell.x >= Width ||
            cell.y < 0 ||
            cell.y >= Height)
        {
            return null;
        }

        return m_BoardData[cell.x, cell.y];
    }
}
