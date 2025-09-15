using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FryingPan : MonoBehaviour
{
    [SerializeField] float timeSec = 5.5f;
    [SerializeField] GameObject spaceText;
    [SerializeField] GameObject flipObj;
    Vector2 upperPos;
    float timer;
    Vector2 startPos;
    Vector3 startRot;
    bool flipping = false;
    // Start is called before the first frame update
    void Start()
    {
        startPos = flipObj.transform.position;
        // startRot = flipObj.transform.rotation;
        upperPos = new Vector2(startPos.x, startPos.y + 3);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= timeSec)
        {
            spaceText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                timer = 0;
                spaceText.SetActive(false);
                flipping = true;
            }
        }

        timer += Time.deltaTime;
        if (flipping) Flip();
    }

    void Flip()
    {
        if (timer > 2.5f)
        {
            flipping = false;
        }
        else
        {
            Debug.Log(timer / 2.5f);
            flipObj.transform.position = new Vector2(startPos.x, Mathf.Sin(timer / 2.5f * 3.14f) * upperPos.y + startPos.y);
            flipObj.transform.Rotate(0, 0, (360f/2.5f)*Time.deltaTime);
            //rotate -> add to rotation -> add 
        }
    }
}
