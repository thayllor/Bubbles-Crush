using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoResizeGridLayoutGroup : MonoBehaviour
{
    Canvas Canvas;
    GridLayoutGroup gridLayoutGroup;
    [SerializeField]
    public float cellx;
    public float celly;
    // Start is called before the first frame update
    void Start()
    {
        Canvas = FindObjectOfType<Canvas>();
        gridLayoutGroup = this.GetComponent<GridLayoutGroup>();

        cellx = this.gameObject.GetComponent<RectTransform>().rect.width;
        celly = this.gameObject.GetComponent<RectTransform>().rect.height;
        Vector2 newSize = new Vector2(cellx / 4, celly / 4);
        gridLayoutGroup.cellSize = newSize;

    }
}
