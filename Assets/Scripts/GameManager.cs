using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Element")]
    public TMP_Text currTeamUI;
    public GameObject DisplayWinnerUI;
    public TMP_Text WinnerTextUI;

    public TMP_Text UnitcurrHealthUI;
    public TMP_Text UnitAttackDamageUI;
    public TMP_Text UnitAttackRangeUI;
    public TMP_Text UnitMoveSpeedUI;
    public TMP_Text UnitNameUI;

    public Canvas UnitCanvasUI;
    public GameObject PlayerPhaseBlock;
    private TMP_Text PlayerPhaseText;
    private Animator PlayerPhaseAnimate;

    private Ray ray;
    private RaycastHit hit;

    public int NumberofTeams = 2;
    public int currTeam;
    public GameObject UnitsonBoard;
    public GameObject pauseGame;
    public bool isPausing;

    public GameObject Team1;
    public GameObject Team2;
    public GameObject Leader1;
    public GameObject Leader2;

    public GameObject unitBeingonDisplay;
    public GameObject tileBeingonDisplay;
    public bool displayingUnitInfo;

    public TileMap TMS;
    public battleManager BMS;

    public int cursorX;
    public int cursorY;

    public int SelectedXTile;
    public int SelectedYTile;

    List<Node> currPathUnitRoute;
    List<Node> UnitPathtoCursor;

    public bool UnitPathExist;

    public Material UnitRouteUI;
    public Material UnitRouteCurveUI;
    public Material UnitRouteArrowUI;
    public Material UnitCursorUI;

    public int RouteToX;
    public int RouteToY;

    //gameobject to remember tile to disable
    public GameObject OneAwayFromUnit;

    public int TargetFrameRate = 90;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;

        currTeam = 0;
        displayingUnitInfo = false;
        setCurrTeam();
        HealthBarUpdate();
        UnitPathtoCursor = new List<Node>();
        UnitPathExist = false;
        TMS = GetComponent<TileMap>();
        BMS = GetComponent<battleManager>();
        PlayerPhaseText = PlayerPhaseBlock.GetComponentInChildren<TextMeshProUGUI>();
        PlayerPhaseAnimate = PlayerPhaseBlock.GetComponent<Animator>();
        isPausing = false;
        pauseGame.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit))
        {
            //for update cursor and unit
            cursorUIUpdate();
            unitUIUpdate();

            //highlight path
            if(TMS.SelectedUnit != null && TMS.SelectedUnit.GetComponent<Unit>().getMovementStates(1) == TMS.SelectedUnit.GetComponent<Unit>().unitMoveState)
            {
                //check range
                if (TMS.SelectedUnitMoveRange.Contains(TMS.graph[cursorX, cursorY]))
                {
                    if (cursorX != TMS.SelectedUnit.GetComponent<Unit>().posx || cursorY != TMS.SelectedUnit.GetComponent<Unit>().posy)
                    {
                        if(!UnitPathExist && TMS.SelectedUnit.GetComponent<Unit>().movementQueue.Count == 0)
                        {
                            UnitPathtoCursor = GenerateRouteTo(cursorX, cursorY);

                            RouteToX = cursorX;
                            RouteToY = cursorY;

                            if (UnitPathtoCursor.Count != 0)
                            {
                                for (int i = 0; i < UnitPathtoCursor.Count; i++)
                                {
                                    int nodeX = UnitPathtoCursor[i].x;
                                    int nodeY = UnitPathtoCursor[i].y;

                                    if (i == 0)
                                    {
                                        GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[nodeX, nodeY];
                                        quadToUpdate.GetComponent<Renderer>().material = UnitCursorUI;
                                    }
                                    else if (i != 0 && (i+1) != UnitPathtoCursor.Count)
                                    {
                                        SetCorrectRoute(nodeX, nodeY, i);
                                    }
                                    else if (i == UnitPathtoCursor.Count - 1)
                                    {
                                        SetCorrectRouteForFinal(nodeX, nodeY, i);
                                    }

                                    TMS.quadOnMapForUnitMoveDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
                                }
                            }
                            UnitPathExist = true;
                        }
                        else if (RouteToX != cursorX || RouteToY != cursorY)
                        {
                            if (UnitPathtoCursor.Count != 0)
                            {
                                for (int i = 0; i < UnitPathtoCursor.Count; i++)
                                {
                                    int nodeX = UnitPathtoCursor[i].x;
                                    int nodeY = UnitPathtoCursor[i].y;

                                    TMS.quadOnMapForUnitMoveDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = false;
                                }
                            }
                            UnitPathExist = false;
                        }
                    }
                    else if (cursorX == TMS.SelectedUnit.GetComponent<Unit>().posx && cursorY == TMS.SelectedUnit.GetComponent<Unit>().posy)
                    {
                        TMS.disableUnitUIRoute();
                        UnitPathExist = false;
                    }
                }
            }
        }

        if (!isPausing)
        {
            Time.timeScale = 1;
        } else if (isPausing)
        {
            Time.timeScale = 0;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            if (!isPausing)
            {
                GamePause();
            }           
        }
    }

    public void GamePause()
    {
        isPausing = true;
        pauseGame.SetActive(true);
    }

    public void ResumeGame()
    {
        isPausing = false;
        pauseGame.SetActive(false);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }

    public void cursorUIUpdate()
    {
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingonDisplay == null)
            {
                SelectedXTile = hit.transform.gameObject.GetComponent<TileClick>().TileX;
                SelectedYTile = hit.transform.gameObject.GetComponent<TileClick>().TileY;
                cursorX = SelectedXTile;
                cursorY = SelectedYTile;
                TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingonDisplay = hit.transform.gameObject;
            }
            else if (tileBeingonDisplay != hit.transform.gameObject)
            {
                SelectedXTile = tileBeingonDisplay.GetComponent<TileClick>().TileX;
                SelectedYTile = tileBeingonDisplay.GetComponent<TileClick>().TileY;
                TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = false;

                SelectedXTile = hit.transform.gameObject.GetComponent<TileClick>().TileX;
                SelectedYTile = hit.transform.gameObject.GetComponent<TileClick>().TileY;
                cursorX = SelectedXTile;
                cursorY = SelectedYTile;
                TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingonDisplay = hit.transform.gameObject;
            }
        }
        else if (hit.transform.CompareTag("Unit"))
        {
            if (tileBeingonDisplay == null)
            {
                SelectedXTile = hit.transform.gameObject.GetComponent<Unit>().posx;
                SelectedYTile = hit.transform.gameObject.GetComponent<Unit>().posy;
                cursorX = SelectedXTile;
                cursorY = SelectedYTile;
                TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingonDisplay = hit.transform.parent.gameObject.GetComponent<Unit>().OccupiedTile;
            }
            else if (tileBeingonDisplay != hit.transform.gameObject)
            {
                if (hit.transform.gameObject.GetComponent<Unit>().movementQueue.Count == 0)
                {
                    SelectedXTile = tileBeingonDisplay.GetComponent<TileClick>().TileX;
                    SelectedYTile = tileBeingonDisplay.GetComponent<TileClick>().TileY;
                    TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = false;

                    SelectedXTile = hit.transform.gameObject.GetComponent<Unit>().posx;
                    SelectedYTile = hit.transform.gameObject.GetComponent<Unit>().posy;
                    cursorX = SelectedXTile;
                    cursorY = SelectedYTile;
                    TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = true;
                    tileBeingonDisplay = hit.transform.parent.gameObject.GetComponent<Unit>().OccupiedTile;
                }
            }
        }
        else
        {
            TMS.quadOnMapCursor[SelectedXTile, SelectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void unitUIUpdate()
    {
        if (!displayingUnitInfo)
        {
            if (hit.transform.CompareTag("Unit"))
            {
                UnitCanvasUI.enabled = true;
                displayingUnitInfo = true;
                unitBeingonDisplay = hit.transform.gameObject;
                var highlightedUnitScript = hit.transform.parent.gameObject.GetComponent<Unit>();

                UnitcurrHealthUI.SetText(highlightedUnitScript.currHealth.ToString());
                UnitAttackDamageUI.SetText(highlightedUnitScript.unitAttack.ToString());
                UnitAttackRangeUI.SetText(highlightedUnitScript.unitRange.ToString());
                UnitMoveSpeedUI.SetText(highlightedUnitScript.speedMove.ToString());
                UnitNameUI.SetText(highlightedUnitScript.unitName);
            }
            else if (hit.transform.CompareTag("Tile"))
            {
                if(hit.transform.GetComponent<TileClick>().UnitonTile != null)
                {
                    unitBeingonDisplay = hit.transform.GetComponent<TileClick>().UnitonTile;

                    UnitCanvasUI.enabled = true;
                    displayingUnitInfo = true;
                    var highlightedUnitScript = unitBeingonDisplay.GetComponent<Unit>();

                    UnitcurrHealthUI.SetText("HP : " + highlightedUnitScript.currHealth.ToString());
                    UnitAttackDamageUI.SetText("Att DMG : " + highlightedUnitScript.unitAttack.ToString());
                    UnitAttackRangeUI.SetText("Att Range : " + highlightedUnitScript.unitRange.ToString());
                    UnitMoveSpeedUI.SetText("Move Range : " + highlightedUnitScript.speedMove.ToString());
                    UnitNameUI.SetText(highlightedUnitScript.unitName);
                }
            }
        }
        else if (hit.transform.gameObject.CompareTag("Tile"))
        {
            if (hit.transform.GetComponent<TileClick>().UnitonTile == null)
            {
                UnitCanvasUI.enabled = false;
                displayingUnitInfo = false;
            }
            else if (hit.transform.GetComponent<TileClick>().UnitonTile != unitBeingonDisplay)
            {
                UnitCanvasUI.enabled = false;
                displayingUnitInfo = false;
            }
        }
        else if (hit.transform.gameObject.CompareTag("Unit"))
        {
            if (hit.transform.parent.gameObject != unitBeingonDisplay)
            {
                UnitCanvasUI.enabled = false;
                displayingUnitInfo = false;
            }
        }
    }

    public void setCurrTeam()
    {
        if(currTeam == 0)
        {
            currTeamUI.SetText("Current Turn : Player " + (currTeam + 1).ToString());
        }      

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 2 Player"))
        {
            if (currTeam == 1)
            {
                currTeamUI.SetText("Current Turn : Player 2");
            }
        }else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 1 Player"))
        {
            if (currTeam == 1)
            {
                currTeamUI.SetText("Current Turn : Enemy");
            }
        }


    }

    public void switchCurrPlayer()
    {
        resetUnitsMovement(returnTeam(currTeam));
        currTeam++;
        if(currTeam == NumberofTeams)
        {
            currTeam = 0;
        }
    }

    public GameObject returnTeam(int i)
    {
        GameObject TeamReturn = null;
        if(i == 0)
        {
            TeamReturn = Team1;
        }
        
        if(i == 1)
        {
            TeamReturn = Team2;
        }
        return TeamReturn;
    }

    public void HealthBarUpdate()
    {
        for(int i = 8; i < NumberofTeams; i++)
        {
            GameObject team = returnTeam(i);
            if(team == returnTeam(currTeam))
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<Unit>().changeHealthColor(0);
                }
            }
            else
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<Unit>().changeHealthColor(1);
                }
            }
        }
    }

    public void resetUnitsMovement(GameObject TeamReset)
    {
        foreach (Transform unit in TeamReset.transform)
        {
            unit.GetComponent<Unit>().MoveAgain();
        }
    }

    public void endTurn()
    {
        if(TMS.SelectedUnit == null)
        {
            switchCurrPlayer();
            if(currTeam == 0)
            {
                PlayerPhaseAnimate.SetTrigger("SlideIn");
                Debug.Log("Player Turn");
                PlayerPhaseText.SetText("Player 1 Phase");
            }

            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 1 Player"))
            {
                if (currTeam == 1)
                {
                    PlayerPhaseAnimate.SetTrigger("SlideIn");
                    Debug.Log("Enemy Turn");
                    PlayerPhaseText.SetText("Enemy Phase");
                }
            } else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 2 Player"))
            {
                if (currTeam == 1)
                {
                    PlayerPhaseAnimate.SetTrigger("SlideIn");
                    Debug.Log("Player 2 Turn");
                    PlayerPhaseText.SetText("Player 2 Phase");
                }
            }

            
            HealthBarUpdate();
            setCurrTeam();
        }
    }

    public void checkIfUnitRemain(GameObject unit, GameObject enemy)
    {
        Debug.Log("Start Coroutine");
        StartCoroutine(checkIfUnitRemainCoroutine(unit, enemy));
    }

    public List<Node> GenerateRouteTo(int x, int y)
    {
        if(TMS.SelectedUnit.GetComponent<Unit>().posx == x && TMS.SelectedUnit.GetComponent<Unit>().posy == y)
        {
            currPathUnitRoute = new List<Node>();

            return currPathUnitRoute;
        }

        if (TMS.EnterTheTile(x, y) == false)
        {
            return null;
        }

        currPathUnitRoute = null;
        //djikstra
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        Node source = TMS.graph[TMS.SelectedUnit.GetComponent<Unit>().posx, TMS.SelectedUnit.GetComponent<Unit>().posy];
        Node target = TMS.graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        List<Node> unvisited = new List<Node>(); // add not yet visited node

        //intitialize
        foreach (Node v in TMS.graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            Node u = null;

            foreach (Node PossibleU in unvisited)
            {
                if (u == null || dist[PossibleU] < dist[u])
                {
                    u = PossibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node v in u.near)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + TMS.TileCost(v.x, v.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }
        if (prev[target] == null)
        {
            return null;
        }

        currPathUnitRoute = new List<Node>();

        Node now = target;

        while (now != null)
        {
            currPathUnitRoute.Add(now);
            now = prev[now];
        }

        currPathUnitRoute.Reverse();

        return currPathUnitRoute;
    }

    public IEnumerator checkIfUnitRemainCoroutine(GameObject unit, GameObject enemy)
    {
        while (unit.GetComponent<Unit>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        while (enemy.GetComponent<Unit>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 1 Player"))
        {
            if (BMS.enemyWin)
            {
                DisplayWinnerUI.SetActive(true);
                WinnerTextUI.SetText("Enemy win!");
            }
            else if (BMS.player1Win)
            {
                DisplayWinnerUI.SetActive(true);
                WinnerTextUI.SetText("Player win!");
            }
        } else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Stage 2 Player"))
        {
            if (BMS.enemyWin)
            {
                DisplayWinnerUI.SetActive(true);
                WinnerTextUI.SetText("Player 2 win!");
            }
            else if (BMS.player1Win)
            {
                DisplayWinnerUI.SetActive(true);
                WinnerTextUI.SetText("Player win!");
            }
        }

        
    }

    public void resetQuad(GameObject quadToReset)
    {
        quadToReset.GetComponent<Renderer>().material = UnitCursorUI;
        quadToReset.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void UIUnitRouteDisplay(Vector2 cursorPos, Vector3 arrowRotationVector)
    {
        GameObject quadToManipulate = TMS.quadOnMapForUnitMoveDisplay[(int)cursorPos.x, (int)cursorPos.y];
        quadToManipulate.transform.eulerAngles = arrowRotationVector;
        quadToManipulate.GetComponent<Renderer>().material = UnitRouteArrowUI;
        quadToManipulate.GetComponent<Renderer>().enabled = true;
    }

    public Vector2 directionBeetwen(Vector2 currentVector, Vector2 nextVector)
    {
        Vector2 vectorDirection = (nextVector - currentVector).normalized;

        if(vectorDirection == Vector2.right)
        {
            return Vector2.right;
        }
        else if(vectorDirection == Vector2.left)
        {
            return Vector2.left;
        }
        else if(vectorDirection == Vector2.up)
        {
            return Vector2.up;
        }
        else if(vectorDirection == Vector2.down)
        {
            return Vector2.down;
        } 
        else
        {
            Vector2 vectorToReturn = new Vector2();
            return vectorToReturn;
        }
    }

    public void SetCorrectRoute(int NodeX, int NodeY, int i)
    {
        Vector2 previousTile = new Vector2(UnitPathtoCursor[i - 1].x + 1, UnitPathtoCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(UnitPathtoCursor[i].x + 1, UnitPathtoCursor[i].y + 1);
        Vector2 nextTile = new Vector2(UnitPathtoCursor[i + 1].x + 1, UnitPathtoCursor[i + 1].y + 1);

        Vector2 backToCurrentVector = directionBeetwen(previousTile, currentTile);
        Vector2 currentToNextVector = directionBeetwen(currentTile, nextTile);

        if (backToCurrentVector == Vector2.right && currentToNextVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }

        else if (backToCurrentVector == Vector2.right && currentToNextVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }

        else if (backToCurrentVector == Vector2.right && currentToNextVector == Vector2.down)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }

        else if (backToCurrentVector == Vector2.left && currentToNextVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }

        else if (backToCurrentVector == Vector2.left && currentToNextVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToNextVector == Vector2.down)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToNextVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToNextVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToNextVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToNextVector == Vector2.down)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToNextVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToNextVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteCurveUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }

    public void SetCorrectRouteForFinal(int NodeX, int NodeY, int i)
    {
        Vector2 previousTile = new Vector2(UnitPathtoCursor[i - 1].x + 1, UnitPathtoCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(UnitPathtoCursor[i].x + 1, UnitPathtoCursor[i].y + 1);
        Vector2 backToCurrentVector = directionBeetwen(previousTile, currentTile);

        if(backToCurrentVector == Vector2.right)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteArrowUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if(backToCurrentVector == Vector2.left)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteArrowUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if(backToCurrentVector == Vector2.up)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteArrowUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if(backToCurrentVector == Vector2.down)
        {
            GameObject quadToUpdate = TMS.quadOnMapForUnitMoveDisplay[NodeX, NodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UnitRouteArrowUI;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }
}
