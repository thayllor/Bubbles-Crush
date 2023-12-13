using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public bool isMatched;
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public GameObject otherDot;


    private Board board;
    private EndGameManager endGameManager;
    private FindMatches findMatches;
    private HintManager hintManager;
    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 finalTouchPosition = Vector2.zero;
    private Vector2 tempPosition;
    private Animator animator;
    private float shineDelay;
    private float shineDelaySeconds;

    [Header("Swipe Stuff")] 
    public float swipeAngle = 0;
    public float swipeResist = 1f;
    

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;
    public GameObject adjacentMarker;


    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isAdjacentBomb = false;
        isColorBomb = false;
        shineDelay = Random.Range(3f,5f);
        shineDelaySeconds = shineDelay;
        animator = GetComponent<Animator>();
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        hintManager = FindObjectOfType<HintManager>();
        endGameManager = FindObjectOfType<EndGameManager>();
    }

    // Update is called once per frame

    // this is for debug only
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }
    void Update()
    {
        shineDelaySeconds -= Time.deltaTime;
        if(shineDelaySeconds <= 0)
        {
            shineDelaySeconds = shineDelay;
            StartCoroutine(StartShineCo());
        }
        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //move to the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .01f);
            if(board.allDots[column,row]!= this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        }
        else
        {
            //set position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            //board.allDots[column, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //move to the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .01f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        }
        else
        {
            //set position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            //board.allDots[column, row] = this.gameObject;
        }
    }

    IEnumerator StartShineCo()
    {
        animator.SetBool("Shine", true);
        yield return null;
        animator.SetBool("Shine", false);

    }

    public void PopAnimation()
    {
        animator.SetBool("Popped", true);
    }
    public IEnumerator CheckMoveCo()
    {
        //se a peça mexida é a bomba e a outra tem a cor para destruir 
        if (isColorBomb)
        {
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
            otherDot.GetComponent<Dot>().isMatched = true;
        }//se a outra peça é a bomba e a peça mexida tem a cor para destruir
        else if(otherDot.GetComponent<Dot>().isColorBomb){
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            isMatched = true;
            otherDot.GetComponent<Dot>().isMatched = true;
        }
        yield return new WaitForSeconds(.5f);
        if(otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }else
            {
                if (endGameManager != null)
                {
                    if(endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();
            }
            //otherDot = null;
        }
    }
    private void OnMouseDown()
    {
        //Destroi da dica
        if(hintManager != null)
        {
            hintManager.DestroyHint();
        }

        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);            
        }

    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
        
    }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if(board.lockTiles[column,row] == null && board.lockTiles[column + (int)direction.x, row + (int)direction.y] == null)
        {
            if(otherDot != null)
            {
                otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
                otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
                column += (int)direction.x;
                row += (int)direction.y;
                StartCoroutine(CheckMoveCo());
            }
            else
            {
                board.currentState = GameState.move;
            }
        }else
            {
                board.currentState = GameState.move;
            }
    }
    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width -1)
        {   //Right Swipe
            //otherDot = board.allDots[column + 1, row];
            //previousRow = row;
            //previousColumn = column;
            //otherDot.GetComponent<Dot>().column -= 1;
            //column += 1;
            //StartCoroutine(CheckMoveCo());
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height -1)
        {   //Up Swipe
            //otherDot = board.allDots[column, row + 1];
            //previousRow = row;
            //previousColumn = column;
            //otherDot.GetComponent<Dot>().row -= 1;
            //row += 1;
            //StartCoroutine(CheckMoveCo());
            MovePiecesActual(Vector2.up);
        }
        else if (swipeAngle > 135 || swipeAngle <= -135 && column > 0)
        {   //Left Swipe
            //otherDot = board.allDots[column - 1, row];
            //previousRow = row;
            //previousColumn = column;
            //otherDot.GetComponent<Dot>().column += 1;
            //column -= 1;
            //StartCoroutine(CheckMoveCo());
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {   //Down Swipe 
            //otherDot = board.allDots[column, row - 1];
            //previousRow = row;
            //previousColumn = column;
            //otherDot.GetComponent<Dot>().row += 1;
            //row -= 1;
            //StartCoroutine(CheckMoveCo());
            MovePiecesActual(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move;
        }

        
    }
    public void MakeRowBomb()
    {
        if(!isColumnBomb && !isColorBomb && !isAdjacentBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

    public void MakeColumnBomb()
    {
        if (!isRowBomb && !isColorBomb && !isAdjacentBomb)
        { 
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

    public void MakeAdjacentBomb()
    {
        if (!isColumnBomb && !isColorBomb && !isRowBomb)
        { 
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    public void MakeColorBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isAdjacentBomb)
        { 
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color";
        }
    }


}
