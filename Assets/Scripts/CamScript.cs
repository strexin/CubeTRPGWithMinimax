using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public IEnumerator camShake(float duration, float shakeStrenght, Vector3 direction)
    {
        float updatedShakeStrengt = shakeStrenght;
        if(shakeStrenght > 10)
        {
            shakeStrenght = 10;
        }
        Vector3 originalPos = transform.position;
        Vector3 endPoint = new Vector3(direction.x, 0, direction.z) * (shakeStrenght / 2);

        float timepassed = 0f;
        while (timepassed < duration)
        {
            float xPos = Random.Range(-.1f, .1f) * shakeStrenght;
            float zPos = Random.Range(-.1f, .1f) * shakeStrenght;
            Vector3 newPos = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z + zPos);
            transform.position = Vector3.Lerp(transform.position, newPos, 0.15f);
            timepassed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }
}
