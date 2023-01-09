using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class battleManager : MonoBehaviour
{
    public GameManager GM;
    public CamScript CS;

    public bool player1Win;
    public bool enemyWin;

    private bool battleStatus;

    public void Battle(GameObject initiator, GameObject recipient)
    {
        battleStatus = true;
        var initiatorUnit = initiator.GetComponent<Unit>();
        var recipientUnit = recipient.GetComponent<Unit>();
        int initiatorAtt = initiatorUnit.unitAttack;
        int recipientAtt = recipientUnit.unitAttack;

        if(initiatorUnit.unitRange == recipientUnit.unitRange)
        {
            recipientUnit.damage(initiatorAtt);
            if (deadCheck(recipient))
            {
                if (recipient.GetComponent<Unit>().unitName == "Leader")
                {
                    if (recipient.GetComponent<Unit>().team == 0)
                    {
                        enemyWin = true;
                    } else if (recipient.GetComponent<Unit>().team == 1)
                    {
                        player1Win = true;
                    }
                }
                recipient.transform.parent = null;
                recipientUnit.death();
                battleStatus = false;
                GM.checkIfUnitRemain(initiator, recipient);
                return;
            }
        }
        else
        {
            recipientUnit.damage(initiatorAtt);
            if (deadCheck(recipient))
            {
                recipient.transform.parent = null;
                recipientUnit.death();
                battleStatus = false;
                GM.checkIfUnitRemain(initiator, recipient);
                return;
            }
        }
        battleStatus = false;
    }

    public bool deadCheck(GameObject UnittoCheck)
    {
        if (UnittoCheck.GetComponent<Unit>().currHealth <= 0)
        {
            return true;
        }
        return false;
    }

    public void destroyObject(GameObject UnittoDestroy)
    {
        Destroy(UnittoDestroy);
    }

    public IEnumerator attack(GameObject Unit, GameObject Enemy)
    {
        battleStatus = true;
        float elapsedTime = 0f;
        Vector3 startPos = Unit.transform.position;
        Vector3 endPos = Enemy.transform.position;
        
        while(elapsedTime < 0.25f)
        {
            Unit.transform.position = Vector3.Lerp(startPos, startPos + ((((endPos - startPos) / (endPos - startPos).magnitude)).normalized * .5f), (elapsedTime / .25f));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        while (battleStatus)
        {
            StartCoroutine(CS.camShake(0.2f, Unit.GetComponent<Unit>().unitAttack, getDirection(Unit, Enemy)));

            Battle(Unit, Enemy);

            yield return new WaitForEndOfFrame();
        }

        if(Unit != null)
        {
            StartCoroutine(returnAfterAttack(Unit, startPos));
        }
    }

    public IEnumerator returnAfterAttack(GameObject Unit, Vector3 endPoint)
    {
        float elapsedTime = 0f;

        while(elapsedTime < 0.30f)
        {
            Unit.transform.position = Vector3.Lerp(Unit.transform.position, endPoint, (elapsedTime / .25f));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public Vector3 getDirection(GameObject Unit, GameObject Enemy)
    {
        Vector3 startPos = Unit.transform.position;
        Vector3 endPos = Enemy.transform.position;
        return ((endPos - startPos) / (endPos - startPos).magnitude).normalized;
    }
}
