using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }
    public int number { get; private set; }
    public bool locked { get; set; }

    private Image background;
    private TextMeshProUGUI tileText;

    private void Awake()
    {
        background = GetComponent<Image>();
        tileText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state, int number)
    {
        this.state = state;
        this.number = number;

        background.color = state.backgroundColor;
        tileText.text = number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null)
            this.cell.tile = null;

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
            this.cell.tile = null;

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(Animate(cell.transform.position, false));// move tile to adjacent
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)
            this.cell.tile = null;// tile a

        this.cell = null;// cell a
        cell.tile.locked = true;// tile b

        StartCoroutine(Animate(cell.transform.position, true));// merge tile a to b, Destroy tile a
    }

    IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(transform.position, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;

        if (merging)
            Destroy(gameObject);
    }
}
