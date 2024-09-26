using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;
    public Tile tilePrefab;
    public TileState[] tileStates;
    private TileGrid grid;
    private List<Tile> tiles;
    private bool waiting;
    [SerializeField] private AudioSource moveTileAudio;
    [SerializeField] private int winCondition;
    private Vector2 startTouchPosition, endTouchPosition;
    private float minSwipeDistance = 100f;// k/c min nhận diện vuốt

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    void Update()
    {
        if (!waiting)
        {
            PCInput();
            AndroidInput();
        }
    }

    private void PCInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveTiles(Vector2Int.up, 0, 1, 1, 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
    }

    private void AndroidInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                startTouchPosition = touch.position;
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                DetectSwipe();
            }
        }
    }
    private void DetectSwipe()
    {
        //float swipeDistance = (endTouchPosition - startTouchPosition).magnitude;//magnitude: độ dài Vector
        float swipeDistance = Vector2.Distance(startTouchPosition, endTouchPosition);

        if (swipeDistance >= minSwipeDistance)
        {
            Vector2 swipeDirection = endTouchPosition - startTouchPosition;
            swipeDirection.Normalize();//chuẩn hóa hướng vuốt có gt -1 -> 1 để ss độ lớn x,y xđ hướng vuốt dễ dàng

            if (swipeDirection.y > 0 && Mathf.Abs(swipeDirection.y) > Mathf.Abs(swipeDirection.x))
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            else if (swipeDirection.y < 0 && Mathf.Abs(swipeDirection.y) > Mathf.Abs(swipeDirection.x))
                MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
            else if (swipeDirection.x < 0 && Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            else if (swipeDirection.x > 0 && Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], 2);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    public void ClearBoard()
    {
        foreach (var item in grid.cells)
            item.tile = null;

        foreach (var item in tiles)
            Destroy(item.gameObject);

        tiles.Clear();
    }

    private void MoveTiles(Vector2Int direction, int startI, int incrementI, int startJ, int incrementJ)
    {
        bool changed = false;

        for (int i = startI; i < grid.width && i >= 0; i += incrementI)
        {
            for (int j = startJ; j < grid.height && j >= 0; j += incrementJ)
            {
                TileCell cell = grid.GetCell(i, j);
                if (cell.occupied)
                    changed |= MoveTile(cell.tile, direction);// =changed || = MoveTile...
            }
        }

        if (changed)
            StartCoroutine(WaitForChanges());
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(direction, tile.cell);//tọa độ ô cell kề bên

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                if (CanMerge(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }
                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(direction, adjacent);//tọa độ ô cell kề bên tiếp theo
            //khi tọa độ adjacent ở TileGird.GetCell(i,j) return null thì stop
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool CanMerge(Tile a, Tile b) => a.number == b.number && !b.locked;

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);// nếu quá state cuối thì vẫn xài state cuối
        int number = b.number * 2;

        b.SetState(tileStates[index], number);

        gameManager.IncreaseScore(number);
        if (number == winCondition)
            gameManager.GameWin();
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
                return i;
        }
        return -1;
    }

    private IEnumerator WaitForChanges()
    {//jup move tile chính xác hơn, ko thực hiện nhiều chuyển động trong cùng bản cập nhật...
        waiting = true;
        moveTileAudio.Play();
        yield return new WaitForSeconds(0.1f);
        waiting = false;

        if (tiles.Count != grid.size)
            CreateTile();

        foreach (var item in tiles)
            item.locked = false;

        if (CheckForGameOver())
            gameManager.GameOver();
    }

    private bool CheckForGameOver()
    {
        if (tiles.Count != grid.size)
            return false;

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(Vector2Int.up, tile.cell);
            TileCell down = grid.GetAdjacentCell(Vector2Int.down, tile.cell);
            TileCell left = grid.GetAdjacentCell(Vector2Int.left, tile.cell);
            TileCell right = grid.GetAdjacentCell(Vector2Int.right, tile.cell);

            if (up != null && CanMerge(tile, up.tile)
                || down != null && CanMerge(tile, down.tile)
                || left != null && CanMerge(tile, left.tile)
                || right != null && CanMerge(tile, right.tile))
                return false;
        }
        return true;
    }
}
