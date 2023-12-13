using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal,
    Lock,
    Concrete,
    Slime
}

[System.Serializable]
public enum Goalkind
{
    Breakable,
    Lock,
    Concrete,
    Slime,
    Red,
    DarkGreen,
    LightGreen,
    Blue,
    White,
    Orange
}
[System.Serializable]
public enum Powerkind
{
    color,
    adjacent,
    row,
    column
}
[System.Serializable]
public class Power
{
    public Powerkind powerKind;
    public int column;
    public int row;

}
[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
}

public class Board : MonoBehaviour
{

    /*
     script responsaver pelas funçoes do board
     - setar os primeiros pontos
     - marcar pontos ligados
     - verificar e destruir pontos marcados

     */
    [Header ("Scriptable Object Stuff")]
    public World world;
    public int level = -1;

    public GameState currentState = GameState.move;
    [Header("board Dimensions")]
    public int width;
    public int height;
    public int offSet;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimePiecePrefab;
    public GameObject[] dots;
    public GameObject destroyEffects;

    [Header("Layout")]
    public GameObject[,] allDots;
    public Dot currentDot;
    public int basePicieValue = 20;
    public float refillDelay = 0.5f;
    public TileType [] boardLayout;
    public int[] scoreGoals;
    private BackgroundTile[,] breakableTiles ;
    private BackgroundTile[,] concreteTiles;
    private BackgroundTile[,] slimeTiles;
    public BackgroundTile[,] lockTiles;
    private bool[,] blankSpaces;
    public Power[] startPowers;

    [Header("Match Stuff")]
    public MatchType matchType;
    private ScoreManager scoreManager;
    private FindMatches findMatches;
    private SoundManager soundManager;
    private int streakValue = 1;
    private GoalManager goalManager;
    private bool makeSlime = true;

    // Inicialização
    private void Awake()
    {
        if(level == -1)
        {
            if(PlayerPrefs.HasKey("Current Level"))
            {
                level = PlayerPrefs.GetInt("Current Level");
            }
        }
        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level]!= null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    startPowers = world.levels[level].startPowers;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
       
    }
    void Start()
    {
        breakableTiles = new BackgroundTile[width, height];
        lockTiles = new BackgroundTile[width, height];
        concreteTiles = new BackgroundTile[width, height];
        slimeTiles = new BackgroundTile[width, height];
        findMatches = FindAnyObjectByType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        scoreManager = FindAnyObjectByType<ScoreManager>();
        soundManager = FindAnyObjectByType<SoundManager>();
        goalManager = FindAnyObjectByType<GoalManager>();
        SetUp();
        currentState = GameState.pause;
    }
    private void SetUp()
    {
        ///agrupar tudo em um metodo
        GenerateBreakableTiles();
        GenerateBlankSpaces();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();
        for (int i = 0; i< width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && !concreteTiles[i,j] && !slimeTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + "," + j + " )";

                    int dotToUse = Random.Range(0, dots.Length);

                    int maxInteration = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxInteration < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxInteration++;
                    }
                    maxInteration = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                 
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + "," + j + " )";
                    allDots[i, j] = dot;
                }
                else
                {

                }
            }
        }
        SetStartPowers();
    }
    // Inicialização
    private void SetStartPowers()
    {
        
        for (int i = 0; i < startPowers.Length; i++)
        {
            int colunm = startPowers[i].column;
            int row = startPowers[i].row;
            Debug.Log(colunm + ""+ row);
            if((colunm < 0 || colunm > width) || (row < 0 || row > height))
            {
                continue;
            }
            switch (startPowers[i].powerKind)
            {
                case Powerkind.color:
                    allDots[colunm, row].GetComponent<Dot>().MakeColorBomb();
                    break;
                case Powerkind.adjacent:
                    allDots[colunm, row].GetComponent<Dot>().MakeAdjacentBomb();
                    break;
                case Powerkind.row:
                    allDots[colunm, row].GetComponent<Dot>().MakeRowBomb();
                    break;
                case Powerkind.column:
                    allDots[colunm, row].GetComponent<Dot>().MakeColumnBomb();
                    break;
                default:
                    break;
            }
        }
    }

    // Geradores
    public void GenerateBlankSpaces()
    {
        for(int i = 0; i < boardLayout.Length; i++)
        {
            if(boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }
    public void GenerateBreakableTiles()
    {
        //olha todos os tiles do layout
        for(int i =0; i < boardLayout.Length; i++)
        {
            //se for um Jelly
            if(boardLayout[i].tileKind == TileKind.Breakable)
            {
                //cria um Jelly na posição
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                tile.GetComponent<BackgroundTile>().hitPoints = 3;
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    public void GenerateLockTiles()
    {
        //olha todos os tiles do layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //se for um Lock
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                //cria um Lock na posição
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    public void GenerateSlimeTiles()
    {
        //olha todos os tiles do layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //se for um slime
            if (boardLayout[i].tileKind == TileKind.Slime)
            {
                //cria um Lock na posição
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    public void GenerateConcreteTiles()
    {
        //olha todos os tiles do layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //se for um Concrete
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                //cria um Concrete na posição
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }
    // Geradores
     


    // BASE do jogo
    private bool MatchesOnBoard()
    {
        findMatches.FindAllMatches();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allDots[i,j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    ///MatchesAt - Define quais pontos estão marcados "ligados"
    private bool MatchesAt(int collumn, int row, GameObject piece)
    {
        if(collumn> 1 && row > 1)
        {
            if(allDots[collumn - 1, row] != null && allDots[collumn - 2, row] != null)
            {
                if(allDots[collumn - 1, row].tag == piece.tag && allDots[collumn - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }

            if(allDots[collumn, row - 1] != null && allDots[collumn, row - 2] != null)
            {
                if (allDots[collumn, row -1].tag == piece.tag && allDots[collumn, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
        }else if (collumn<= 1 || row<= 1)
        {
            if(row > 1)
            {
                if(allDots[collumn, row -1] != null && allDots[collumn, row - 2] != null)
                {
                    if (allDots[collumn, row - 1].tag == piece.tag && allDots[collumn, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }

            if (collumn > 1)
            {
                if(allDots[collumn - 1, row] != null && allDots[collumn - 2, row] != null)
                {
                    if (allDots[collumn - 1, row].tag == piece.tag && allDots[collumn - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    public void DestroyMatches()
    {
        Debug.Log("DestroyMatches");
        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();
        for (int i = 0; i< width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j]!= null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo2());
    }
    ///DecreaseRowCo2 - Corotine que abaixa o restante dos pontos
    private IEnumerator DecreaseRowCo2()
    {
        //yield return new WaitForSeconds(refillDelay * 1.5f);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // se o locar esta vazio e é branco
                if(!blankSpaces[i,j] && allDots[i,j] == null && !concreteTiles[i,j] && !slimeTiles[i, j])
                {//loop de cima do espaço até o topo
                    for (int k = j + 1; k < height; k++)
                    {
                        //se achar o dot
                        if(allDots[i,k] !=null)
                        {
                            //move até op espaço em branco
                            allDots[i, k].GetComponent<Dot>().row = j;
                            //seta o espaço como vazio
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        Debug.Log("Refilling the board");
        StartCoroutine(FillBoardCo());

    }
    private void RefilBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankSpaces[i,j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIteractions = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIteractions < 100)
                    {
                        maxIteractions ++;
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    maxIteractions = 0;

                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }
    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(refillDelay);
        RefilBoard();
        yield return new WaitForSeconds(refillDelay);
        while (MatchesOnBoard())
        {
            streakValue ++;
            DestroyMatches();
            yield break;
        }
            yield return new WaitForSeconds(refillDelay/2);
        while (MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield break;
        }

        currentDot = null;
        CheckToMakeSlime();
        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoard());
            Debug.Log("DeadLocked!!!!");
        }
        yield return new WaitForSeconds(refillDelay);
        System.GC.Collect();
        if(currentState != GameState.pause)
        {
            currentState = GameState.move;
        }
        makeSlime = true;
        streakValue = 1;
    }
    ///DestroyMatchesAt - Destroi os pontos marcados
    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {

            //olha se um tile tem que quebrar
            DamageBreakable(column, row);
            DamageLock(column, row);
            DamageConcrete(column, row);
            DamageSlime(column, row);
            if (goalManager != null)
            {
                goalManager.CompareGoal(allDots[column,row].tag.ToString());
                goalManager.UpdateGoals();
            }

            // se tem o som
            if(soundManager != null)
            {
                soundManager.PlayRandomDestroyNoise();
            }
            //GameObject particle = Instantiate(destroyEffects, allDots[column, row].transform.position, Quaternion.identity);
            //Destroy(particle, .5f);
            allDots[column, row].GetComponent<Dot>().PopAnimation();
            Destroy(allDots[column, row], .7f);
            scoreManager.IncreaseScore(basePicieValue * streakValue);
            allDots[column, row] = null;
        }
    }
    // BASE do jogo

    //funçoes de dano nos tales
    public void DamageBreakable(int column, int row)
    {
        if (breakableTiles[column, row] != null)
        {
            breakableTiles[column, row].TakeDamage(1);
            if (breakableTiles[column, row].hitPoints <= 0)
            {
                breakableTiles[column, row] = null;
            }
        }
    }
    public void DamageLock(int column, int row)
    {
        if (lockTiles[column, row] != null)
        {
            lockTiles[column, row].TakeDamage(1);
            if (lockTiles[column, row].hitPoints <= 0)
            {
                lockTiles[column, row] = null;
            }
        }
    }
    public void DamageConcrete(int column, int row)
    {
        if (column > 0)
        {
            if (concreteTiles[column - 1, row])
            {
                concreteTiles[column -1, row].TakeDamage(1);
                if (concreteTiles[column - 1, row].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row] = null;
                }
            }
        }
        if (column < width - 1)
        {
            if (concreteTiles[column + 1, row])
            {
                concreteTiles[column + 1, row].TakeDamage(1);
                if (concreteTiles[column + 1, row].hitPoints <= 0)
                {
                    concreteTiles[column + 1, row] = null;
                }
            }
        }
        if (row > 0)
        {
            if (concreteTiles[column, row - 1])
            {
                concreteTiles[column, row - 1].TakeDamage(1);
                if (concreteTiles[column, row - 1].hitPoints <= 0)
                {
                    concreteTiles[column, row - 1] = null;
                }
            }
        }
        if (row < height - 1)
        {
            if (concreteTiles[column, row + 1])
            {
                concreteTiles[column, row + 1].TakeDamage(1);
                if (concreteTiles[column, row + 1].hitPoints <= 0)
                {
                    concreteTiles[column, row + 1] = null;
                }
            }
        }
    }
    public void DamageSlime(int column, int row)
    {
        if (column > 0)
        {
            if (slimeTiles[column - 1, row])
            {
                slimeTiles[column - 1, row].TakeDamage(1);
                if (slimeTiles[column - 1, row].hitPoints <= 0)
                {
                    slimeTiles[column - 1, row] = null;
                    makeSlime = false;
                }
            }
        }
        if (column < width - 1)
        {
            if (slimeTiles[column + 1, row])
            {
                slimeTiles[column + 1, row].TakeDamage(1);
                if (slimeTiles[column + 1, row].hitPoints <= 0)
                {
                    slimeTiles[column + 1, row] = null;
                    makeSlime = false;
                }
            }
        }
        if (row > 0)
        {
            if (slimeTiles[column, row - 1])
            {
                slimeTiles[column, row - 1].TakeDamage(1);
                if (slimeTiles[column, row - 1].hitPoints <= 0)
                {
                    slimeTiles[column, row - 1] = null;
                    makeSlime = false;
                }
            }
        }
        if (row < height - 1)
        {
            if (slimeTiles[column, row + 1])
            {
                slimeTiles[column, row + 1].TakeDamage(1);
                if (slimeTiles[column, row + 1].hitPoints <= 0)
                {
                    slimeTiles[column, row + 1] = null;
                    makeSlime = false;
                }
            }
        }
    }
    //funçoes de dano nos tales

    //funçoes de controle das bomas
    public void BombRow(int row) 
    {
        for (int i = 0; i < width; i++)
        {
            if (concreteTiles[i, row])
            {
                concreteTiles[i, row].TakeDamage(1);
                if (concreteTiles[i, row].hitPoints <= 0)
                {
                    concreteTiles[i, row] = null;
                }
            }
            //if (concreteTiles[i, j])
            //{
            //    DamageConcrete(i, j);
            //}
            //if (breakableTiles[i, j])
            //{
            //    DamageBreakable(i, j);
            //}
            //if (slimeTiles[i, j])
            //{
            //    DamageSlime(i, j);
            //}
            //if (lockTiles[i, j])
            //{
            //    DamageLock(i, j);
            //}
        }
    }
    public void BombColumn(int column)
    {
        for (int i = 0; i < height; i++)
        {
            if (concreteTiles[column, i])
            {
                concreteTiles[column, i].TakeDamage(1);
                if (concreteTiles[column, i].hitPoints <= 0)
                {
                    concreteTiles[column, i] = null;
                }
            }

            //if (concreteTiles[i, j])
            //{
            //    DamageConcrete(i, j);
            //}
            //if (breakableTiles[i, j])
            //{
            //    DamageBreakable(i, j);
            //}
            //if (slimeTiles[i, j])
            //{
            //    DamageSlime(i, j);
            //}
            //if (lockTiles[i, j])
            //{
            //    DamageLock(i, j);
            //}
        }
    }
    private MatchType ColumnOrRow()
    {
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;
        matchType.type = 0;
        matchType.color = "";
        for (int i = 0; i < matchCopy.Count; i++)
        {
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            string color = matchCopy[i].tag;
            int colunmMatch = 0;
            int rowMatch = 0;
            for (int j = 0; j < matchCopy.Count; j++)
            {
                Dot nextDot = matchCopy[j].GetComponent<Dot>();
                if (nextDot == thisDot)
                {
                    continue;
                }
                if (nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    colunmMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            //return 3 se é coluns ou row
            //return 2 se é adjacent
            //return 1 pra color bomb

            if (colunmMatch == 4 || rowMatch == 4)
            {
                //Debug.Log("ColorBomb");
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            else if (colunmMatch == 2 || rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if (colunmMatch == 3 || rowMatch == 3)
            {
                matchType.type = 3;
                matchType.color = color;
                return matchType;
            }
        }
        matchType.type = 0;
        matchType.color = "";
        return matchType;

    }
    private void CheckToMakeBombs()
    {
        if(findMatches.currentMatches.Count > 3)
        {
            MatchType typeOffMatch = ColumnOrRow();
            if(typeOffMatch.type == 1)
            {
                //faz a bomba de cor
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOffMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeColorBomb();
                }
                else
                {
                    if (currentDot.otherDot != null)
                    {
                        Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                        if (otherDot.isMatched && otherDot.tag == typeOffMatch.color)
                        {
                            otherDot.isMatched = false;
                            otherDot.MakeColorBomb();
                        }
                    }
                }
            }
            else if(typeOffMatch.type == 2)
            {
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOffMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeAdjacentBomb();
                        
                }
                else
                {
                    if (currentDot.otherDot != null)
                    {
                        Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                        if (otherDot.isMatched && otherDot.tag == typeOffMatch.color)
                        {
                            otherDot.isMatched = false;
                            otherDot.MakeAdjacentBomb();
                        }
                    }
                }
            }
            else if (typeOffMatch.type == 3)
            {
                findMatches.CheckBombs(typeOffMatch);
            }
        }

    }
    //funçoes de controle das bomas

    //funçoes envolvendo deadLock
    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        //pega a segunda peça e salva no holder 
        if(allDots[column + (int)direction.x, row + (int)direction.y] != null)
        {
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
            // troca com a primeira peça
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            allDots[column, row] = holder;
        }
    } 
    private bool CheckForMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    //verifica se não saiu pra fora do board
                    if(i< width - 2) 
                    { 
                        //verifica se existe um e 2 pontos a direita
                        if (allDots[i + 1, j]!= null && allDots[i + 2, j] != null)
                        {
                            //verifica se faria um match pela ponta
                            if (allDots[i + 1, j].tag == allDots[i, j].tag &&
                                allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }

                    //verifica se não saiu pra fora do board
                    if (j < height - 2)
                    {
                        //verifica se existe um e 2 pontos a cima
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null){
                            //verifica se faria um match pela ponta
                            if (allDots[i, j + 1].tag == allDots[i, j].tag &&
                                allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    
                }
            }
        }

        return false;
    }
    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }
    private bool IsDeadlocked()
    {
        for(int i = 0; i< width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allDots[i,j]!= null)
                {
                    if(i< width - 1)
                    {
                        if(SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }

                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }

                }
            }
        }
        return true;
    }
    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(0.5f);
        List<GameObject> newBoard = new List<GameObject>();
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                newBoard.Add(allDots[i, j]);
            }
        }
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && !concreteTiles[i,j] && !slimeTiles[i, j])
                {
                    //pega um espaço aleatorio
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();

                    int maxInteration = 0;

                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxInteration < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxInteration++;
                    }
                    maxInteration = 0;

                    //atribui ao Dot em questão
                    piece.column = i;
                    piece.row = j;
                    allDots[i, j] = newBoard[pieceToUse];
                    //remove da lista dos remaneçentes
                    newBoard.Remove(newBoard[pieceToUse]);

                }
            }
        }
        // se estiver travado
        if (IsDeadlocked())
        {
            StartCoroutine(ShuffleBoard());
        }
    }
    //funçoes envolvendo deadLock

    //funçoes slime
    private void CheckToMakeSlime()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(slimeTiles[i,j] != null && makeSlime)
                {
                    //make slime
                    Debug.Log(i + "," + j);
                    Debug.Log("make slime");
                    MakeNewSlime();
                    return;
                }
            }
        }
    }
    private Vector2 CheckForAdjacent(int column, int row)
    {
        if (allDots[column + 1, row] && column < width - 1)
        {
            return Vector2.right;
        }
        if (allDots[column - 1, row] && column > 0)
        {
            return Vector2.left;
        }
        if (allDots[column, row + 1] && row < height - 1)
        {
            return Vector2.up;
        }
        if (allDots[column, row - 1] && row > 0)
        {
            return Vector2.down;
        }
        return Vector2.zero;
    }
    private void MakeNewSlime()
    {
        bool slime = false;
        int loops = 0;
        while (!slime && loops < 200)
        {
            int newX = Random.Range(0, width - 1);
            int newY = Random.Range(0, height - 1);
            if(slimeTiles[newX, newY] != null)
            {
                Vector2 adjacent = CheckForAdjacent(newX, newY);
                if(adjacent != Vector2.zero)
                {
                    Destroy(allDots[newX + (int)adjacent.x, newY + (int)adjacent.y]);
                    Vector2 tempPosition = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                    Debug.Log(tempPosition);
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTile>();
                    slime = true;
                }
            }

            loops++;
        }
    }
    //funçoes slime


}
