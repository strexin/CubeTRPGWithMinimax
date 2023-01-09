using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public int team;
    public int posx;
    public int posy;

    public HashSet<Node> MoveOpt;

    public TileMap map;
    public GameManager GM;

    //Queue
    public Queue<int> movementQueue;
    public Queue<int> combatQueue;

    public GameObject OccupiedTile;

    public float VisualMovement = 0.20f;

    //public List<Node> currentPath = null;

    public List<Node> MovementPath = null;
    public bool finishMove = false;

    public bool maxPlayer;

    //Unit Stats
    [Header("Unit Stats")]
    public string unitName;
    public int speedMove = 2;
    public int unitHealth = 3;
    public int unitAttack = 1;
    public int unitRange = 1;
    public int currHealth;

    //UI
    [Header("UI")]
    public Canvas HeathBar;
    public TMP_Text hitPointsText;
    public Image HealthBarVis;

    public Transform startPoint;
    public Transform endingPoint;
    public float moveSpeedTime = 1f;

    public bool UnitMove;

    [SerializeField] private float unitDistance;

    public enum MovementStates
    {
        Unselected,
        Selected,
        Move,
        Wait
    }

    public MovementStates unitMoveState;

    private void Awake()
    {
        movementQueue = new Queue<int>();
        combatQueue = new Queue<int>();

        posx = (int)transform.position.x;
        posy = (int)transform.position.y;
        unitMoveState = MovementStates.Unselected;
        currHealth = unitHealth;
        hitPointsText.SetText(currHealth.ToString());
        map = GetComponent<TileMap>();
    }

    private void Update()
    {
        
    }

    public void MoveNextTile()
    {
        if(MovementPath.Count == 0)
        {
            return;
        } 
        else
        {
            StartCoroutine(MoveUnitEverySecond(transform.gameObject, MovementPath[MovementPath.Count - 1]));
        }
    }

    public void MoveAgain()
    {
        MovementPath = null;
        setMovementStates(0);
        finishMove = false;
    }

    public MovementStates getMovementStates(int state)
    {
        if(state == 0)
        {
            return MovementStates.Unselected;
        }
        if(state == 1)
        {
            return MovementStates.Selected;
        }
        if(state == 2)
        {
            return MovementStates.Move;
        }
        if(state == 3)
        {
            return MovementStates.Wait;
        }
        return MovementStates.Unselected;
    }

    public void setMovementStates(int state)
    {
        if(state == 0)
        {
            unitMoveState = MovementStates.Unselected;
        }
        if(state == 1)
        {
            unitMoveState = MovementStates.Selected;
        }
        if(state == 2)
        {
            unitMoveState = MovementStates.Move;
        }
        if(state == 3)
        {
            unitMoveState = MovementStates.Wait;
        }
    }

    public void healthUpdate()
    {
        HealthBarVis.fillAmount = (float)currHealth / unitHealth;
        hitPointsText.SetText(currHealth.ToString());
    }

    public void damage(int x)
    {
        currHealth = currHealth - x;
        healthUpdate();
    }

    public void wait()
    {
        //gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.gray;
    }

    public void changeHealthColor(int i)
    {
        if (i == 0)
        {
            HealthBarVis.color = Color.blue;
        }
        else if (i == 1)
        {
            HealthBarVis.color = Color.red;
        }
    }

    public void death()
    {
        StartCoroutine(FadeOut());
        StartCoroutine(checkRoutine());
    }

    public IEnumerator checkRoutine()
    {
        while(combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject, 1f);
    }

    public IEnumerator FadeOut()
    {
        combatQueue.Enqueue(1);

        for (float i = 1f; i > 0.5f; i -= 0.01f)
        {
            yield return new WaitForEndOfFrame();
        }
        combatQueue.Dequeue();
    }

    IEnumerator MoveUnitEverySecond(GameObject UnittoMove, Node finalNode)
    {
        movementQueue.Enqueue(1);

        MovementPath.RemoveAt(0);

        while(MovementPath.Count != 0)
        {
            Vector3 endingPoint = map.CoorTiletoWorld(MovementPath[0].x, MovementPath[0].y);
            UnittoMove.transform.position = Vector3.Lerp(transform.position, endingPoint, .2f);
            if((transform.position - endingPoint).sqrMagnitude < 0.001)
            {
                MovementPath.RemoveAt(0);
            }
            yield return new WaitForEndOfFrame();
        }

        transform.position = map.CoorTiletoWorld(finalNode.x, finalNode.y);

        posx = finalNode.x;
        posy = finalNode.y;
        OccupiedTile.GetComponent<TileClick>().UnitonTile = null;
        OccupiedTile = map.tilesOnMap[posx, posy];
        movementQueue.Dequeue();
    }

    void resetPath()
    {
        MovementPath = null;
        finishMove = false;
    }
}