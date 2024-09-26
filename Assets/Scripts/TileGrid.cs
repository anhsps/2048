using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public TileRow[] rows { get; private set; }
    public TileCell[] cells { get; private set; }
    public int size => cells.Length;
    public int height => rows.Length;
    public int width => size / height;

    private void Awake()
    {
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int j = 0; j < rows.Length; j++)
        {
            for (int i = 0; i < rows[j].cells.Length; i++)
            {
                rows[j].cells[i].coordinates = new Vector2Int(i, j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public TileCell GetRandomEmptyCell()
    {
        int index = Random.Range(0, size);
        int startingIndex = index;
        while (cells[index].occupied)
        {
            index++;
            if (index >= size)
                index = 0;
            if (index == startingIndex)
                return null;
        }
        return cells[index];
    }

    public TileCell GetCell(int i, int j)
    {
        if (i >= 0 && j >= 0 && i < width && j < height)
            return rows[j].cells[i];
        else
            return null;
    }

    public TileCell GetCell(Vector2Int coordinates) => GetCell(coordinates.x, coordinates.y);

    public TileCell GetAdjacentCell(Vector2Int direction, TileCell cell)
    {
        Vector2Int coordinates = cell.coordinates;
        coordinates.x += direction.x;
        coordinates.y -= direction.y;
        return GetCell(coordinates);
    }
}
// Grid 16 ô, coi ô góc trên trái là tọa độ gốc O (vì ở Unity tạo Cell đầu ở góc trên trái).
// x dương từ O -> phải ; y dương từ O -> dưới