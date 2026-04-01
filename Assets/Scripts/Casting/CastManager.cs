using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn.Unity.Example;
using FMODUnity;

public class CastManager : MonoBehaviour
{
    public static CastManager instance;
    [SerializeField] GameObject castGO;
    [SerializeField] float linePointDist = 0.3f;
    [SerializeField] GameObject drawLineCollider;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] float passingScore = 80f;
    [SerializeField] int maxTries = 3;
    LineRenderer line;
    Vector2 lastLinePos = new Vector2(-100, -100);
    Camera cam;
    float score = 0;
    CastSO cast;
    List<GameObject> pathPoints;
    float pointScore = 100f;
    int numOutBounds = 0;
    public bool pointsInBounds = true;
    int numTries = 0;
    int maxLines;
    int numLines = 0;

    bool inPause = false;
    public CookingManager cManager;

    [Header("FMOD SFX")]
    [SerializeField] private EventReference castTraceOneShotEvent;
    [SerializeField] private EventReference castSuccessOneShotEvent;
    [SerializeField] private EventReference castRetryOneShotEvent;

    [Header("Cast Trace SFX Tuning")]
    [SerializeField] private float traceRetriggerInterval = 1.7f;

    private float nextTraceSfxTime = 0f;

    void Start()
    {
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        line = this.GetComponent<LineRenderer>();
        line.positionCount = 0;

        cam = Camera.main;

        GameObject newCast = Instantiate(castGO, Camera.main.transform.position, Quaternion.identity);

        cast = newCast.GetComponent<CastSO>();

        maxLines = cast.numLines;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = pos;

        inPause = cManager.inPause;
        if (inPause) return; // Makes sure game isn't paused before anything happens

        // drawLineCollider.transform.position = pos;

        // Draws the line while the mouse button is held down
        if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(pos, lastLinePos) > linePointDist)
            {
                line.positionCount++;
                int pointIndex = line.positionCount - 1;
                line.SetPosition(pointIndex, pos);
                lastLinePos = pos;

                CheckPointBounds(pos);
                PlayTraceSfxIfNeeded();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            numLines++;
            nextTraceSfxTime = 0f;

            if (numLines >= maxLines)
            {
                //If after 3? tries -> next button/transition

                pathPoints = cast.pointObjects;
                if (pathPoints.Count > 0) pointScore = 100f / pathPoints.Count;

                foreach (GameObject point in pathPoints)
                {
                    if (point.GetComponent<PathPoint>().hit == false)
                    {
                        score = 0;
                        break;
                    }
                    else
                    {
                        score += pointScore;
                    }
                    // Debug.Log(point.GetComponent<PathPoint>().hit);
                }

                if (score >= passingScore)
                {
                    scoreText.text = "Divine!";
                    scoreText.gameObject.SetActive(true);

                    PlayOneShotIfAssigned(castSuccessOneShotEvent);

                    CookingManager.instance.Transition();
                    gameObject.SetActive(false);
                    CookingManager.instance.cookingSuccess = true;
                }

                else
                {
                    if (numTries >= maxTries)
                    {
                        //Do 'skip cast' option
                    }
                    RetryCast();
                    // scoreText.text = "Dubious";
                    // CookingManager.instance.cookingSuccess = false;
                }
            }
        }
    }

    void CheckPointBounds(Vector3 pos)
    {
        //Use OnCollisionExit2D on cast 
        //numOutBounds could be used either way to do a percentage or 'static' score system
        if (pointsInBounds == false)
        {
            Debug.Log("Score deducted by 5");
            score -= 5; //TEMP, MAYBE MAKE IT DEPEND ON numOutBounds
            numOutBounds++;
        }
    }

    public void SetBoundsBool(bool inBounds)
    {
        pointsInBounds = inBounds;
    }

    public void RetryCast()
    {
        line.positionCount = 0;
        score = 0;
        numTries++;
        nextTraceSfxTime = 0f;
        Debug.Log("Cast is reset. Num tries is " + numTries);

        PlayOneShotIfAssigned(castRetryOneShotEvent);

        pathPoints = cast.pointObjects;
        foreach (GameObject point in pathPoints)
        {
            point.GetComponent<PathPoint>().hit = false;
        }
    }

    private void SkipCast()
    {
        //Do Hestia dialogue
        //show 'next' button
        //
    }

    private void PlayTraceSfxIfNeeded()
    {
        if (castTraceOneShotEvent.IsNull) return;

        if (Time.time >= nextTraceSfxTime)
        {
            RuntimeManager.PlayOneShot(castTraceOneShotEvent, transform.position);
            nextTraceSfxTime = Time.time + traceRetriggerInterval;
        }
    }

    private void PlayOneShotIfAssigned(EventReference eventRef)
    {
        if (eventRef.IsNull) return;
        RuntimeManager.PlayOneShot(eventRef, transform.position);
    }
}