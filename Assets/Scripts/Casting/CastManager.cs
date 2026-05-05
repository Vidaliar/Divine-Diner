using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn.Unity.Example;
using FMODUnity;
using FMOD.Studio;

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

    bool hasCast = false;
    bool waitForMouseRelease = false;
    GameObject currentCastObject;

    [Header("FMOD SFX")]
    [SerializeField] private EventReference castTraceLoopEvent;
    [SerializeField] private EventReference castSuccessOneShotEvent;
    [SerializeField] private EventReference castRetryOneShotEvent;

    private EventInstance castTraceInstance;
    private bool castTraceStarted = false;

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

        if (!castTraceLoopEvent.IsNull)
        {
            castTraceInstance = RuntimeManager.CreateInstance(castTraceLoopEvent);
        }

        // If castGO is already assigned, old behavior still works.
        // If castGO is empty, CastSelector will call SetCast later.
        if (castGO != null)
        {
            SetCast(castGO);
        }
    }

    public void SetCast(GameObject selectedCast)
    {
        castGO = selectedCast;

        if (currentCastObject != null)
        {
            Destroy(currentCastObject);
        }

        Vector3 castPos = Camera.main.transform.position;
        castPos.z = 0f;

        currentCastObject = Instantiate(castGO, castPos, Quaternion.identity);

        cast = currentCastObject.GetComponent<CastSO>();

        maxLines = cast.numLines;

        line.positionCount = 0;
        lastLinePos = new Vector2(-100, -100);
        score = 0;
        pointScore = 100f;
        numOutBounds = 0;
        pointsInBounds = true;
        numTries = 0;
        numLines = 0;

        hasCast = true;

        // Prevent the same click used to select the cast from also drawing a line.
        waitForMouseRelease = true;

        if (!castTraceLoopEvent.IsNull && !castTraceInstance.isValid())
        {
            castTraceInstance = RuntimeManager.CreateInstance(castTraceLoopEvent);
        }
    }

    void Update()
    {
        if (hasCast == false)
        {
            return;
        }

        Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = pos;

        if (cManager != null)
        {
            inPause = cManager.inPause;
        }

        if (inPause)
        {
            PauseTraceSfx(true);
            return;
        }
        else
        {
            PauseTraceSfx(false);
        }

        if (waitForMouseRelease)
        {
            if (Input.GetMouseButton(0) == false)
            {
                waitForMouseRelease = false;
            }

            return;
        }

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

                // Start trace sound the moment the player begins a real valid stroke
                StartTraceSfx();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Sigil progress is gone on release, so stop immediately
            StopTraceSfx();

            numLines++;

            if (numLines >= maxLines)
            {
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
                }

                if (score >= passingScore)
                {
                    scoreText.text = "Divine!";
                    scoreText.gameObject.SetActive(true);

                    PlayOneShotIfAssigned(castSuccessOneShotEvent);

                    StopTraceSfx();
                    CookingManager.instance.Transition();
                    gameObject.SetActive(false);
                    CookingManager.instance.cookingSuccess = true;
                }
                else
                {
                    if (numTries >= maxTries)
                    {
                        // Do 'skip cast' option
                    }

                    RetryCast();
                }
            }
        }
    }

    void CheckPointBounds(Vector3 pos)
    {
        if (pointsInBounds == false)
        {
            Debug.Log("Score deducted by 5");
            score -= 5;
            numOutBounds++;
        }
    }

    public void SetBoundsBool(bool inBounds)
    {
        pointsInBounds = inBounds;
    }

    public void RetryCast()
    {
        StopTraceSfx();

        line.positionCount = 0;
        score = 0;
        numLines = 0;
        numTries++;
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

    private void StartTraceSfx()
    {
        if (castTraceStarted) return;

        if (!castTraceInstance.isValid() && !castTraceLoopEvent.IsNull)
        {
            castTraceInstance = RuntimeManager.CreateInstance(castTraceLoopEvent);
        }

        if (!castTraceInstance.isValid()) return;

        castTraceInstance.start();
        castTraceStarted = true;
    }

    private void StopTraceSfx()
    {
        if (!castTraceStarted) return;
        if (!castTraceInstance.isValid()) return;

        castTraceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        castTraceStarted = false;
    }

    private void PauseTraceSfx(bool pause)
    {
        if (!castTraceStarted) return;
        if (!castTraceInstance.isValid()) return;

        castTraceInstance.setPaused(pause);
    }

    private void PlayOneShotIfAssigned(EventReference eventRef)
    {
        if (eventRef.IsNull) return;
        RuntimeManager.PlayOneShot(eventRef, transform.position);
    }

    private void OnDisable()
    {
        StopTraceSfx();

        if (castTraceInstance.isValid())
        {
            castTraceInstance.release();
        }
    }

    private void OnDestroy()
    {
        if (castTraceInstance.isValid())
        {
            castTraceInstance.release();
        }
    }
}