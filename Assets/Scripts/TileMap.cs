using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMap : MonoBehaviour
{
    [Header("Manager Scripts")]
    public GameManager GM;
    public battleManager BM;

    [Header("Selected Unit")]
    public GameObject SelectedUnit;
    public HashSet<Node> SelectedUnitTotalRange;
    public HashSet<Node> SelectedUnitMoveRange;

    public bool UnitSelected = false;
    public bool hasMoved = false;

    public bool isMaximax;
    public bool AutoMove;

    public int unitSelectedPrevX;
    public int unitSelectedPrevY;

    public GameObject prevOccupiedTile;

    public GameObject unitsOnBoard;

    //private GameObject target = null;

    [Header("Tiles")]
    public TileType[] tileType;
    public GameObject[,] tilesOnMap;

    public GameObject[,] quadOnMap;
    public GameObject[,] quadOnMapForUnitMoveDisplay;
    public GameObject[,] quadOnMapCursor;

    public GameObject MapUI;
    public GameObject MapCursorUI;
    public GameObject MapUnitMovementUI;

    int[,] Tiles;
    public Node[,] graph;
    public Node ResNode;
    public Node FixNode;
    public List<Node> attackableTile;
    public List<Node> currPath = null;
    private float best = -Mathf.Infinity;

    [Header("AI Unit")]
    public GameObject AILeader;
    public GameObject AIUnit1;
    public GameObject AIUnit2;
    public GameObject AIUnit3;
    public GameObject AIUnit4;

    [Header("Map Size")]
    public int MapSizeX = 15;
    public int MapSizeY = 15;

    [Header("Containers")]
    public GameObject tileContainer;
    public GameObject UIQuadPotentialMoveCursor;
    public GameObject UIQuadCursorContainer;
    public GameObject UIUnitMovementContainer;

    [Header("Materials")]
    public Material redMatUI;
    public Material greenMatUI;
    public Material blueMatUI;

    // Start is called before the first frame update
    void Start()
    {
        //set x and y pos for unit
        /*SelectedUnit.GetComponent<Unit>().posx = (int)SelectedUnit.transform.position.x;
        SelectedUnit.GetComponent<Unit>().posy = (int)SelectedUnit.transform.position.y;
        SelectedUnit.GetComponent<Unit>().map = this;*/

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 1 Player"))
        {
            AutoMove = true;
        } else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 2 Player"))
        {
            AutoMove = false;
        }

        MapData();
        generateMapVisuals();
        GeneratePathGraph();
        IfTileOccupied();
    }

    void Update()
    {
        if (AutoMove)
        {
            if (GM.currTeam == 1)
            {
                moveUnitAutomaticaly();
            }
        }              

        if (Input.GetMouseButtonDown(0))
        {
            if(SelectedUnit == null)
            {
                mouseClickToSelectUnit();
            }
            else if(SelectedUnit.GetComponent<Unit>().unitMoveState == SelectedUnit.GetComponent<Unit>().getMovementStates(1) && SelectedUnit.GetComponent<Unit>().movementQueue.Count == 0)
            {
                if (SelectUnitMove())
                {
                    unitSelectedPrevX = SelectedUnit.GetComponent<Unit>().posx;
                    unitSelectedPrevY = SelectedUnit.GetComponent<Unit>().posy;
                    prevOccupiedTile = SelectedUnit.GetComponent<Unit>().OccupiedTile;
                    moveUnit();

                    StartCoroutine(moveUnitandFinalize());
                }
            }
            else if (SelectedUnit.GetComponent<Unit>().unitMoveState == SelectedUnit.GetComponent<Unit>().getMovementStates(2))
            {
                finalizeOption();
            }
        } 

        if (Input.GetMouseButtonDown(1))
        {
            if(SelectedUnit != null)
            {
                if(SelectedUnit.GetComponent<Unit>().movementQueue.Count == 0 && SelectedUnit.GetComponent<Unit>().combatQueue.Count == 0)
                {
                    if(SelectedUnit.GetComponent<Unit>().unitMoveState != SelectedUnit.GetComponent<Unit>().getMovementStates(3))
                    {
                        deselect();
                    }
                }
                else if (SelectedUnit.GetComponent<Unit>().movementQueue.Count == 1)
                {
                    SelectedUnit.GetComponent<Unit>().VisualMovement = 0.5f;
                }
            }
        }


    }

    private void MapData()
    {
        Tiles = new int[MapSizeX, MapSizeY];

        for (int i = 0; i < MapSizeX; i++)
        {
            for (int j = 0; j < MapSizeY; j++)
            {
                Tiles[i, j] = 0;
            }
        }

        //Mud
        Tiles[7, 7] = 1;
        Tiles[6, 6] = 1;
        Tiles[7, 6] = 1;
        Tiles[8, 6] = 1;
        Tiles[7, 8] = 1;
        Tiles[8, 7] = 1;
        Tiles[8, 8] = 1;
        Tiles[6, 8] = 1;
        Tiles[6, 7] = 1;
        Tiles[7, 5] = 1;
        Tiles[7, 9] = 1;
        Tiles[5, 7] = 1;
        Tiles[4, 7] = 1;
        Tiles[9, 7] = 1;
        Tiles[10, 7] = 1;
        Tiles[2, 7] = 1;
        Tiles[2, 8] = 1;
        Tiles[12, 7] = 1;
        Tiles[12, 6] = 1;
        Tiles[12, 8] = 1;
        Tiles[2, 6] = 1;
        Tiles[7, 4] = 1;
        Tiles[7, 10] = 1;
        Tiles[6, 9] = 1;
        Tiles[8, 9] = 1;
        Tiles[5, 8] = 1;
        Tiles[5, 6] = 1;
        Tiles[6, 5] = 1;
        Tiles[8, 5] = 1;
        Tiles[9, 6] = 1;
        Tiles[9, 8] = 1;

        //Walls
        Tiles[2, 10] = 2;
        Tiles[2, 11] = 2;
        Tiles[3, 11] = 2;
        Tiles[2, 3] = 2;
        Tiles[2, 4] = 2;
        Tiles[3, 3] = 2;
        Tiles[11, 3] = 2;
        Tiles[11, 4] = 2;
        Tiles[12, 4] = 2;
        Tiles[11, 10] = 2;
        Tiles[11, 11] = 2;
        Tiles[12, 10] = 2;
        Tiles[3, 4] = 2;
        Tiles[12, 3] = 2;
        Tiles[3, 10] = 2;
        Tiles[12, 11] = 2;
    }

    public float TileCost(int endX, int endY)
    {
        if (EnterTheTile(endX, endY) == false)
        {
            return Mathf.Infinity;
        }

        TileType ttype = tileType[Tiles[endX, endY]];
        float cost = ttype.MoveCost;

        return cost;
    }

    void GeneratePathGraph()
    {
        graph = new Node[MapSizeX, MapSizeY];

        for (int i = 0; i < MapSizeX; i++)
        {
            for (int j = 0; j < MapSizeY; j++)
            {
                graph[i, j] = new Node();
                graph[i, j].x = i;
                graph[i, j].y = j;
            }
        }

        for (int i = 0; i < MapSizeX; i++)
        {
            for (int j = 0; j < MapSizeY; j++)
            {
                if (i > 0)
                {
                    graph[i, j].near.Add(graph[i - 1, j]);
                }
                if (i < MapSizeX - 1)
                {
                    graph[i, j].near.Add(graph[i + 1, j]);
                }
                if (j > 0)
                {
                    graph[i, j].near.Add(graph[i, j - 1]);
                }
                if (j < MapSizeY - 1)
                {
                    graph[i, j].near.Add(graph[i, j + 1]);
                }
            }
        }
    }

    public void generateMapVisuals()
    {
        tilesOnMap = new GameObject[MapSizeX, MapSizeY];
        quadOnMap = new GameObject[MapSizeX, MapSizeY];
        quadOnMapForUnitMoveDisplay = new GameObject[MapSizeX, MapSizeY];
        quadOnMapCursor = new GameObject[MapSizeX, MapSizeY];
        int Index;
        for(int x = 0; x < MapSizeX; x++)
        {
            for(int y = 0; y < MapSizeY; y++)
            {
                Index = Tiles[x, y];
                GameObject newTile = Instantiate(tileType[Index].TilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                newTile.GetComponent<TileClick>().TileX = x;
                newTile.GetComponent<TileClick>().TileY = y;
                newTile.GetComponent<TileClick>().tm = this;
                newTile.transform.SetParent(tileContainer.transform);
                tilesOnMap[x, y] = newTile;

                GameObject gridUI = Instantiate(MapUI, new Vector3(x, y, -0.501f), Quaternion.identity);
                gridUI.transform.SetParent(UIQuadPotentialMoveCursor.transform);
                quadOnMap[x, y] = gridUI;

                GameObject gridUIPathFindingDisplay = Instantiate(MapUnitMovementUI, new Vector3(x, y, -0.502f), Quaternion.identity);
                gridUIPathFindingDisplay.transform.SetParent(UIUnitMovementContainer.transform);
                quadOnMapForUnitMoveDisplay[x, y] = gridUIPathFindingDisplay;

                GameObject gridUICursor = Instantiate(MapCursorUI, new Vector3(x, y, -0.503f), Quaternion.identity);
                gridUICursor.transform.SetParent(UIQuadCursorContainer.transform);
                quadOnMapCursor[x, y] = gridUICursor;
            }
        }
    }

    void InstanTile()
    {
        for(int i = 0; i < MapSizeX; i++)
        {
            for(int j = 0; j < MapSizeY; j++)
            {
                TileType tile = tileType[Tiles[i, j]];
                GameObject gameObj = Instantiate(tile.TilePrefab, new Vector3(i, j, 0), Quaternion.identity);
                TileClick tc = gameObj.GetComponent<TileClick>();
                tc.TileX = i;
                tc.TileY = j;
                tc.tm = this;
            }
        }
    }

    public Vector3 CoorTiletoWorld(int x, int y)
    {
        return new Vector3(x, y, -0.75f);
    }

    public bool EnterTheTile(int x, int y)
    {
        if(tilesOnMap[x, y].GetComponent<TileClick>().UnitonTile != null)
        {
            if(tilesOnMap[x, y].GetComponent<TileClick>().UnitonTile.GetComponent<Unit>().team != SelectedUnit.GetComponent<Unit>().team)
            {
                return false;
            }
        }

        return tileType[Tiles[x, y]].isWalkable;
    }

    public void moveUnit()
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.GetComponent<Unit>().MoveNextTile();
        }
    }

    public void moveUnitAutomaticaly()
    {
        GameObject[] AIUnitArray = { AILeader, AIUnit1, AIUnit2, AIUnit3, AIUnit4 };

        for (int i = 0; i < 5; i++)
        {
            if (SelectedUnit == null)
            {
                if (AIUnitArray[i] != null)
                {
                    GameObject tempUnitAI = AIUnitArray[i];
                    if (tempUnitAI.GetComponent<Unit>().unitMoveState == tempUnitAI.GetComponent<Unit>().getMovementStates(0))
                    {
                        disableHighlightUnitRange();
                        SelectedUnit = tempUnitAI;
                        SelectedUnit.GetComponent<Unit>().map = this;
                        SelectedUnit.GetComponent<Unit>().setMovementStates(1);
                        UnitSelected = true;

                        Node unitPos = graph[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy];

                        Minimax(unitPos, 3, false);

                        MoveSelectedUnit(FixNode.x, FixNode.y);
                        best = -Mathf.Infinity;

                        highlightUnitRange();
                    }
                }                
            }
            else if (SelectedUnit.GetComponent<Unit>().unitMoveState == SelectedUnit.GetComponent<Unit>().getMovementStates(1) && SelectedUnit.GetComponent<Unit>().movementQueue.Count == 0)
            {
                unitSelectedPrevX = SelectedUnit.GetComponent<Unit>().posx;
                unitSelectedPrevY = SelectedUnit.GetComponent<Unit>().posy;
                prevOccupiedTile = SelectedUnit.GetComponent<Unit>().OccupiedTile;
                moveUnit();

                StartCoroutine(moveUnitandFinalize());
            }
            else if (SelectedUnit.GetComponent<Unit>().unitMoveState == SelectedUnit.GetComponent<Unit>().getMovementStates(2))
            {
                checkIdleorAttack();
            }
        }

        GM.endTurn();
    }

    public IEnumerator idleAfterMove()
    {
        if (SelectedUnit != null)
        {
            disableHighlightUnitRange();
            SelectedUnit.GetComponent<Unit>().wait();
            SelectedUnit.GetComponent<Unit>().setMovementStates(3);

            deselect();
            yield return new WaitForEndOfFrame();
        }       
    }

    public void mouseClickToSelectUnit()
    {
        if (UnitSelected == false && GM.tileBeingonDisplay != null)
        {         
            if (GM.tileBeingonDisplay.GetComponent<TileClick>().UnitonTile != null)
            {
                GameObject tempSelectedUnit = GM.tileBeingonDisplay.GetComponent<TileClick>().UnitonTile;
                if(tempSelectedUnit.GetComponent<Unit>().unitMoveState == tempSelectedUnit.GetComponent<Unit>().getMovementStates(0) && tempSelectedUnit.GetComponent<Unit>().team == GM.currTeam)
                {
                    disableHighlightUnitRange();
                    SelectedUnit = tempSelectedUnit;
                    SelectedUnit.GetComponent<Unit>().map = this;
                    SelectedUnit.GetComponent<Unit>().setMovementStates(1);
                    UnitSelected = true;
                    
                    highlightUnitRange();
                }
            }
        }
    }

    public void IfTileOccupied()
    {
        foreach (Transform team in unitsOnBoard.transform)
        {
            foreach (Transform unitOnTeam in team)
            {
                int unitX = unitOnTeam.GetComponent<Unit>().posx;
                int unitY = unitOnTeam.GetComponent<Unit>().posy;
                unitOnTeam.GetComponent<Unit>().OccupiedTile = tilesOnMap[unitX, unitY];
                tilesOnMap[unitX, unitY].GetComponent<TileClick>().UnitonTile = unitOnTeam.gameObject;
            }
        }
    }

    public void MoveSelectedUnit(int x, int y)
    {
        if(SelectedUnit.GetComponent<Unit>().posx == x && SelectedUnit.GetComponent<Unit>().posy == y)
        {
            currPath = new List<Node>();
            SelectedUnit.GetComponent<Unit>().MovementPath = currPath;

            return;
        }
        

        if(EnterTheTile(x, y) == false)
        {
            return;
        }

        SelectedUnit.GetComponent<Unit>().MovementPath = null;
        currPath = null;

        //djikstra pathfinding
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();  

        Node source = graph[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy];
        Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        List<Node> unvisited = new List<Node>(); // add not yet visited node

        //intitialize
        foreach (Node v in graph)
        {
            if(v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }

        while(unvisited.Count > 0)
        {
            Node u = null;

            foreach(Node PossibleU in unvisited)
            {
                if(u == null || dist[PossibleU] < dist[u])
                {
                    u = PossibleU;
                }
            }

            if(u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach(Node v in u.near)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + TileCost(v.x, v.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }
        if (prev[target] == null)
        {
            return;
        }

        currPath = new List<Node>();

        Node now = target;

        while(now != null)
        {
            currPath.Add(now);
            now = prev[now];
        }

        currPath.Reverse();

        SelectedUnit.GetComponent<Unit>().MovementPath = currPath;
    }

    #region Minimax Algorithm

    //Minimax
    public float Minimax(Node unitPos, int depth, bool maxPlayer)
    {
        float score;

        GameObject enemyUnit = GM.GetComponent<GameManager>().Leader1;

        List<Node> MoveOption = getUnitMovementOption().ToList<Node>();
        List<Node> enemyMove = GetPlayerLeaderMove().ToList<Node>();

        if (depth == 0)
        {                                
            score = evaluatePos(ResNode);

            if (score > best && score != Mathf.Infinity && tilesOnMap[ResNode.x, ResNode.y].GetComponent<TileClick>().UnitonTile == null)
            {
                best = score;
                
                FixNode = ResNode;
            }

            return best;
        }

        if (maxPlayer)
        {
            float bestScore = -Mathf.Infinity;
            int earlyPosX = SelectedUnit.GetComponent<Unit>().posx;
            int earlyPosY = SelectedUnit.GetComponent<Unit>().posy;
            foreach (Node n in MoveOption)
            {
                SelectedUnit.GetComponent<Unit>().posx = n.x;
                SelectedUnit.GetComponent<Unit>().posy = n.y;
                ResNode = n;
                float eval = Minimax(n, depth - 1, false);
                SelectedUnit.GetComponent<Unit>().posx = earlyPosX;
                SelectedUnit.GetComponent<Unit>().posy = earlyPosY;

                if (eval > bestScore)
                {
                    ResNode = n;
                    bestScore = Mathf.Max(eval, bestScore);                   
                }
                              
            }
            return bestScore; 
        }
        else
        {           
            float bestScore = Mathf.Infinity;
            int enemyEarlyPosX = enemyUnit.GetComponent<Unit>().posx;
            int enemyEarlyPosY = enemyUnit.GetComponent<Unit>().posy;
            foreach (Node n in enemyMove)
            {
                enemyUnit.GetComponent<Unit>().posx = n.x;
                enemyUnit.GetComponent<Unit>().posy = n.y;
                float eval = Minimax(n, depth - 1, true);
                enemyUnit.GetComponent<Unit>().posx = enemyEarlyPosX;
                enemyUnit.GetComponent<Unit>().posy = enemyEarlyPosY;
                if (eval < bestScore)
                {
                    bestScore = Mathf.Min(eval, bestScore);
                }               
            }
            return bestScore;
        }
    }

    private float evaluatePos(Node UnitPos)
    {
        GameObject EnemyLeader = GM.GetComponent<GameManager>().Leader1;
        List<Node> AttackableTiles = UnitAttackTiles(UnitPos.x, UnitPos.y).ToList<Node>();

        float finalScore = 0;
        float leftHp;
        float damage;

        float xDist;
        float yDist;

        if (SelectedUnit == AILeader)
        {
            if (AILeader.GetComponent<Unit>().currHealth <= 1)
            {
                xDist = UnitPos.x - EnemyLeader.GetComponent<Unit>().posx;
                yDist = UnitPos.y - EnemyLeader.GetComponent<Unit>().posy;
                finalScore = Mathf.Sqrt((xDist * xDist) + (yDist * yDist));
            }
        }

        List<GameObject> playerTeam = new List<GameObject>();

        foreach (Node tile in AttackableTiles)
        {
            var enemy = tilesOnMap[tile.x, tile.y].GetComponent<TileClick>().UnitonTile;
            var isEnemyOnTile = enemy != null;
            if (isEnemyOnTile)
            {
                if(enemy.GetComponent<Unit>().team == 0 && enemy.GetComponent<Unit>().currHealth > 0)
                {
                    playerTeam.Add(enemy);

                    List<GameObject> lowHp = playerTeam.FindAll(en => (en.GetComponent<Unit>().currHealth - SelectedUnit.GetComponent<Unit>().unitAttack) <= 0);

                    if (lowHp != null)
                    {
                        foreach (GameObject gb in lowHp)
                        {
                            if (gb == EnemyLeader)
                            {
                                finalScore += 100;
                            } else
                            {
                                finalScore += 50;
                            }
                        }
                    }

                    if (enemy == EnemyLeader) finalScore += 100;

                    leftHp = SelectedUnit.GetComponent<Unit>().currHealth - enemy.GetComponent<Unit>().unitAttack;
                    damage = SelectedUnit.GetComponent<Unit>().unitAttack - enemy.GetComponent<Unit>().currHealth;
                    finalScore += leftHp + damage + 20;
                }
            }
            else
            {
                xDist = UnitPos.x - EnemyLeader.GetComponent<Unit>().posx;
                yDist = UnitPos.y - EnemyLeader.GetComponent<Unit>().posy;
                finalScore = 1 / Mathf.Sqrt((xDist * xDist) + (yDist * yDist));
            }         
        }

        if (TileCost(UnitPos.x, UnitPos.y) == 2.0f)
        {
            finalScore -= 10;
        }

        return finalScore;
    }

    #endregion

    public void disableUnitUIRoute()
    {
        foreach(GameObject quad in quadOnMapForUnitMoveDisplay)
        {
            if(quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void highlightUnitRange()
    {
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyMovementRange = new HashSet<Node>();

        int attackRange = SelectedUnit.GetComponent<Unit>().unitRange;
        int moveSpeed = SelectedUnit.GetComponent<Unit>().speedMove;

        Node unitInitialNode = graph[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy];
        finalMovementHighlight = getUnitMovementOption();
        totalAttackableTiles = getUnitTotalAttackableTiles(finalMovementHighlight, attackRange, unitInitialNode);

        foreach (Node n in totalAttackableTiles)
        {
            if (tilesOnMap[n.x, n.y].GetComponent<TileClick>().UnitonTile != null)
            {
                GameObject unitOnCurrentlySelectedTile = tilesOnMap[n.x, n.y].GetComponent<TileClick>().UnitonTile;
                if(unitOnCurrentlySelectedTile.GetComponent<Unit>().team != SelectedUnit.GetComponent<Unit>().team)
                {
                    finalEnemyMovementRange.Add(n);
                }
            }
        }

        highlightEnemiesRange(totalAttackableTiles);
        highlightMovementRange(finalMovementHighlight);
        SelectedUnitMoveRange = finalMovementHighlight;
        SelectedUnitTotalRange = getUnitTotalRange(finalMovementHighlight, totalAttackableTiles);
    }

    public void deselect()
    {
        if (SelectedUnit != null)
        {
            if (SelectedUnit.GetComponent<Unit>().unitMoveState == SelectedUnit.GetComponent<Unit>().getMovementStates(1))
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                SelectedUnit.GetComponent<Unit>().setMovementStates(0);
                UnitSelected = false;
                SelectedUnit = null;
            }
            else if (SelectedUnit.GetComponent<Unit>().unitMoveState == SelectedUnit.GetComponent<Unit>().getMovementStates(3))
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                UnitSelected = false;
                SelectedUnit = null;
            }
            else
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                tilesOnMap[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy].GetComponent<TileClick>().UnitonTile = null;
                tilesOnMap[unitSelectedPrevX, unitSelectedPrevY].GetComponent<TileClick>().UnitonTile = SelectedUnit;

                SelectedUnit.GetComponent<Unit>().posx = unitSelectedPrevX;
                SelectedUnit.GetComponent<Unit>().posy = unitSelectedPrevY;
                SelectedUnit.GetComponent<Unit>().OccupiedTile = prevOccupiedTile;
                SelectedUnit.transform.position = CoorTiletoWorld(unitSelectedPrevX, unitSelectedPrevY);
                SelectedUnit.GetComponent<Unit>().setMovementStates(0);
                UnitSelected = false;
                SelectedUnit = null;
            }
        }
    }

    public void finalizeOption()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = GetUnitAttackOptionFrom();

        if (Physics.Raycast(ray, out hit))
        {        
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<TileClick>().UnitonTile != null)
                {
                    GameObject unitOnTile = hit.transform.GetComponent<TileClick>().UnitonTile;
                    int unitX = unitOnTile.GetComponent<Unit>().posx;
                    int unitY = unitOnTile.GetComponent<Unit>().posy;

                    if(unitOnTile == SelectedUnit)
                    {
                        disableHighlightUnitRange();
                        SelectedUnit.GetComponent<Unit>().wait();
                        SelectedUnit.GetComponent<Unit>().setMovementStates(3);
                        //add animation???

                        deselect();
                    }
                    else if(unitOnTile.GetComponent<Unit>().team != SelectedUnit.GetComponent<Unit>().team && attackableTiles.Contains(graph[unitX, unitY]))
                    {
                        if(unitOnTile.GetComponent<Unit>().currHealth > 0)
                        {
                            StartCoroutine(BM.attack(SelectedUnit, unitOnTile));

                            StartCoroutine(deselectAfterMove(SelectedUnit, unitOnTile));
                        }
                    }
                }
            }
            else if(hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
            {
                GameObject unitClicked = hit.transform.parent.gameObject;
                int unitX = gameObject.GetComponent<Unit>().posx;
                int unitY = gameObject.GetComponent<Unit>().posy;

                if(unitClicked == SelectedUnit)
                {
                    disableHighlightUnitRange();
                    SelectedUnit.GetComponent<Unit>().wait();
                    SelectedUnit.GetComponent<Unit>().setMovementStates(3);
                    //add animation???

                    deselect();
                }
                else if(unitClicked.GetComponent<Unit>().team != SelectedUnit.GetComponent<Unit>().team && attackableTiles.Contains(graph[unitX, unitY]))
                {
                    if(unitClicked.GetComponent<Unit>().currHealth > 0)
                    {
                        Debug.Log("That is enemy!");

                        StartCoroutine(BM.attack(SelectedUnit, unitClicked));
                        StartCoroutine(deselectAfterMove(SelectedUnit, unitClicked));
                    }
                }
            }
        }
    }
    public void checkIdleorAttack()
    {
        List<Node> attackTiles = UnitAttackTiles(FixNode.x, FixNode.y).ToList<Node>();

        if (SelectedUnit != null)
        {
            if (SelectedUnit.GetComponent<Unit>().posx == FixNode.x && SelectedUnit.GetComponent<Unit>().posy == FixNode.y)
            {
                foreach (Node n in attackTiles)
                {
                    if (tilesOnMap[n.x, n.y].GetComponent<TileClick>().UnitonTile != null)
                    {
                        GameObject enemyUnit = tilesOnMap[n.x, n.y].GetComponent<TileClick>().UnitonTile;
                        if (enemyUnit.GetComponent<Unit>().team == 0 && enemyUnit == GM.GetComponent<GameManager>().Leader1 && enemyUnit.GetComponent<Unit>().currHealth > 0)
                        {
                            StartCoroutine(BM.attack(SelectedUnit, enemyUnit));
                            StartCoroutine(deselectAfterMove(SelectedUnit, enemyUnit));
                        }
                        else if (enemyUnit.GetComponent<Unit>().team == 0 && enemyUnit.GetComponent<Unit>().currHealth > 0)
                        {
                            StartCoroutine(BM.attack(SelectedUnit, enemyUnit));
                            StartCoroutine(deselectAfterMove(SelectedUnit, enemyUnit));
                        }
                    }
                }
                if (SelectedUnit.GetComponent<Unit>().unitMoveState != SelectedUnit.GetComponent<Unit>().getMovementStates(3))
                {
                    StartCoroutine(idleAfterMove());
                }
                
            }
        }       
    }

    public IEnumerator moveUnitandFinalize()
    {
        disableHighlightUnitRange();
        disableUnitUIRoute();
        while (SelectedUnit.GetComponent<Unit>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        finalizeMovement();
        //Add animation???
        
    }

    public IEnumerator deselectAfterMove(GameObject unit, GameObject enemy)
    {
        SelectedUnit.GetComponent<Unit>().setMovementStates(3);
        disableHighlightUnitRange();
        disableUnitUIRoute();

        yield return new WaitForSeconds(.15f);

        while(unit.GetComponent<Unit>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        while(enemy.GetComponent<Unit>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        deselect();
    }

    public void finalizeMovement()
    {
        tilesOnMap[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy].GetComponent<TileClick>().UnitonTile = SelectedUnit;

        SelectedUnit.GetComponent<Unit>().setMovementStates(2);

        HighlightUnitAttackOptionsFromPosition();
        HighlightTileUnitOccupy();
    }

    //make unit move option
    public HashSet<Node> getUnitMovementOption()
    {
        float[,] cost = new float[MapSizeX, MapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHightlight = new HashSet<Node>();
        HashSet<Node> finalMovementHightlight = new HashSet<Node>();
        int moveSpeed = SelectedUnit.GetComponent<Unit>().speedMove;
        Node unitInitialNode = graph[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy];

        finalMovementHightlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.near)
        {
            cost[n.x, n.y] = TileCost(n.x, n.y);
            if(moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHightlight.UnionWith(UIHighlight);

        while(UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach(Node neighbour in n.near)
                {
                    if (!finalMovementHightlight.Contains(neighbour))
                    {
                        cost[neighbour.x, neighbour.y] = TileCost(neighbour.x, neighbour.y) + cost[n.x, n.y];

                        if(moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            tempUIHightlight.Add(neighbour);
                        }
                    }
                }
            }
            UIHighlight = tempUIHightlight;
            finalMovementHightlight.UnionWith(UIHighlight);
            tempUIHightlight = new HashSet<Node>();
        }

        return finalMovementHightlight;
    }

    public HashSet<Node> GetPlayerLeaderMove()
    {
        if(GM.GetComponent<GameManager>().Leader1 != null)
        {
            GameObject enemyLeader = GM.GetComponent<GameManager>().Leader1;

            float[,] cost = new float[MapSizeX, MapSizeY];
            HashSet<Node> UIHighlight = new HashSet<Node>();
            HashSet<Node> tempUIHightlight = new HashSet<Node>();
            HashSet<Node> finalMovementHightlight = new HashSet<Node>();
            int moveSpeed = enemyLeader.GetComponent<Unit>().speedMove;
            Node unitInitialNode = graph[enemyLeader.GetComponent<Unit>().posx, enemyLeader.GetComponent<Unit>().posy];

            finalMovementHightlight.Add(unitInitialNode);
            foreach (Node n in unitInitialNode.near)
            {
                cost[n.x, n.y] = TileCost(n.x, n.y);
                if (moveSpeed - cost[n.x, n.y] >= 0)
                {
                    UIHighlight.Add(n);
                }
            }

            finalMovementHightlight.UnionWith(UIHighlight);

            while (UIHighlight.Count != 0)
            {
                foreach (Node n in UIHighlight)
                {
                    foreach (Node neighbour in n.near)
                    {
                        if (!finalMovementHightlight.Contains(neighbour))
                        {
                            cost[neighbour.x, neighbour.y] = TileCost(neighbour.x, neighbour.y) + cost[n.x, n.y];

                            if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                            {
                                tempUIHightlight.Add(neighbour);
                            }
                        }
                    }
                }
                UIHighlight = tempUIHightlight;
                finalMovementHightlight.UnionWith(UIHighlight);
                tempUIHightlight = new HashSet<Node>();
            }

            return finalMovementHightlight;
        }
        return null;
    }

    public HashSet<Node> getUnitTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> unionTile = new HashSet<Node>();
        unionTile.UnionWith(finalMovementHighlight);
        unionTile.UnionWith(totalAttackableTiles);
        return unionTile;
    }

    public HashSet<Node> getUnitTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attackRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> NeighbourHash = new HashSet<Node>();
        HashSet<Node> seenNode = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        
        foreach(Node n in finalMovementHighlight)
        {
            NeighbourHash = new HashSet<Node>();
            NeighbourHash.Add(n);
            for (int i = 0; i < attackRange; i++)
            {
                foreach(Node t in NeighbourHash)
                {
                    foreach(Node tn in t.near)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }
                NeighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                if(i < attackRange - 1)
                {
                    seenNode.UnionWith(NeighbourHash);
                }
            }
            NeighbourHash.ExceptWith(seenNode);
            seenNode = new HashSet<Node>();
            totalAttackableTiles.UnionWith(NeighbourHash);
        }
        totalAttackableTiles.Remove(unitInitialNode);

        return totalAttackableTiles;
    }

    public HashSet<Node> GetUnitAttackOptionFrom()
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> NeighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node InitialNode = graph[SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy];
        int attackRange = SelectedUnit.GetComponent<Unit>().unitRange;

        NeighbourHash = new HashSet<Node>();
        NeighbourHash.Add(InitialNode);
        for(int i = 0; i < attackRange; i++)
        {
            foreach (Node t in NeighbourHash)
            {
                foreach (Node tn in t.near)
                {
                    tempNeighbourHash.Add(tn);
                }
            }
            NeighbourHash = tempNeighbourHash;
            tempNeighbourHash = new HashSet<Node>();
            if (i < attackRange - 1)
            {
                seenNodes.UnionWith(NeighbourHash);
            }
        }
        NeighbourHash.ExceptWith(seenNodes);
        NeighbourHash.Remove(InitialNode);
        return NeighbourHash;
    }

    public HashSet<Node> UnitAttackTiles(int posx, int posy)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> NeighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node InitialNode = graph[posx, posy];
        int attackRange = SelectedUnit.GetComponent<Unit>().unitRange;

        NeighbourHash = new HashSet<Node>();
        NeighbourHash.Add(InitialNode);
        for (int i = 0; i < attackRange; i++)
        {
            foreach (Node t in NeighbourHash)
            {
                foreach (Node tn in t.near)
                {
                    tempNeighbourHash.Add(tn);
                }
            }
            NeighbourHash = tempNeighbourHash;
            tempNeighbourHash = new HashSet<Node>();
            if (i < attackRange - 1)
            {
                seenNodes.UnionWith(NeighbourHash);
            }
        }
        NeighbourHash.ExceptWith(seenNodes);
        NeighbourHash.Remove(InitialNode);
        return NeighbourHash;
    }

    public HashSet<Node> getTileUnitOccupying()
    {
        int x = SelectedUnit.GetComponent<Unit>().posx;
        int y = SelectedUnit.GetComponent<Unit>().posy;
        HashSet<Node> singleTile = new HashSet<Node>();
        singleTile.Add(graph[x, y]);
        return singleTile;
    }

    public void highlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = blueMatUI;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void highlightEnemiesRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = redMatUI;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void disableHighlightUnitRange()
    {
        foreach (GameObject quad in quadOnMap)
        {
            if(quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void HighlightTileUnitOccupy()
    {
        if (SelectedUnit != null)
        {
            highlightMovementRange(getTileUnitOccupying());
        }
    }

    public void HighlightUnitAttackOptionsFromPosition()
    {
        if (SelectedUnit != null)
        {
            highlightEnemiesRange(GetUnitAttackOptionFrom());
        }
    }

    public bool SelectUnitMove()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                int clickedTileX = hit.transform.GetComponent<TileClick>().TileX;
                int clickedTileY = hit.transform.GetComponent<TileClick>().TileY;
                Node NodetoCheck = graph[clickedTileX, clickedTileY];
                if (SelectedUnitMoveRange.Contains(NodetoCheck))
                {
                    if((hit.transform.gameObject.GetComponent<TileClick>().UnitonTile == null || hit.transform.gameObject.GetComponent<TileClick>().UnitonTile == SelectedUnit) && (SelectedUnitMoveRange.Contains(NodetoCheck)))
                    {
                        MoveSelectedUnit(clickedTileX, clickedTileY);
                        return true;
                    }
                }
            }
            else if (hit.transform.gameObject.CompareTag("Unit"))
            {
                if (hit.transform.parent.GetComponent<Unit>().team != SelectedUnit.GetComponent<Unit>().team)
                {
                    Debug.Log("Not your team");
                }
                else if(hit.transform.parent.gameObject == SelectedUnit)
                {
                    MoveSelectedUnit(SelectedUnit.GetComponent<Unit>().posx, SelectedUnit.GetComponent<Unit>().posy);
                    return true;
                }
            }
        }

        return false;
    }
}