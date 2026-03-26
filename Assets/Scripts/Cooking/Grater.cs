using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FMODUnity;
using FMOD.Studio;

public class Grater : MonoBehaviour
{
    [SerializeField] GameObject grateObj;
    [SerializeField] GameObject grater;
    [SerializeField] GameObject grateInstructions;
    [SerializeField] int totalGrates;
    [SerializeField] float grateSpeed = 7;
    [SerializeField] float totalDistance = 25;  //Maybe set this to be totalGrates * length of grater? 

    bool grating;
    int gratingCount = 0;

    Vector2 grateDirection;

    float minY;
    float maxY;

    float currDistance = 0;

    bool topSide = false;

    bool inPause = false;
    public CookingManager cManager;

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string gratingLoopEvent;      // example: event:/Sound Effects/Grating

    [FMODUnity.EventRef]
    [SerializeField] private string grateTickOneShotEvent; // little "scrape" tick when you hit ends, maybe implemented later

    private EventInstance gratingInstance;
    private bool gratingSfxStarted = false;

    // Small thresholds so tiny mouse jitter does not trigger the loop
    [Header("Grating SFX Tuning")]
    [SerializeField] private float minDragMagnitudeForSfx = 0.05f;
    [SerializeField] private float minDownwardDirectionForSfx = -0.15f;

    void Start()
    {
        maxY = grater.GetComponent<Collider2D>().bounds.max.y;
        minY = grater.GetComponent<Collider2D>().bounds.min.y;

        grater.transform.localPosition = new Vector3(grater.transform.localPosition.x, grater.transform.localPosition.y, 1);

        grateDirection = new Vector2(0.23f, 0.97f);

        grateInstructions.SetActive(true);

        if (!string.IsNullOrEmpty(gratingLoopEvent))
        {
            gratingInstance = RuntimeManager.CreateInstance(gratingLoopEvent);
        }
    }

    void Update()
    {
        inPause = cManager.inPause;

        if (inPause)
        {
            PauseGratingSfx(true);
            return;
        }
        else
        {
            PauseGratingSfx(false);
        }

        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Start grating when clicked on the grated object
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.up, 0.1f);
            if (hit.collider != null)
            {
                GameObject otherObj = hit.collider.gameObject;
                if (otherObj == grateObj)
                {
                    grating = true;
                }
            }
        }

        // Does the grating
        if (Input.GetMouseButton(0) && grating)
        {
            Vector2 direction = new Vector2(pos.x - grateObj.transform.position.x, pos.y - grateObj.transform.position.y);
            Vector2 normalizedDir = direction.normalized;
            float dirLength = Mathf.Min(direction.magnitude, 1);

            // Sound should only play while actively dragging downward
            bool draggingDown = normalizedDir.y < minDownwardDirectionForSfx && dirLength > minDragMagnitudeForSfx;

            if (draggingDown)
            {
                StartGratingSfx();
            }
            else
            {
                StopGratingSfx();
            }

            float newY = grateObj.transform.position.y + (normalizedDir.y * grateDirection.y * (grateSpeed * dirLength) * Time.deltaTime);
            float clampY = Mathf.Clamp(newY, minY, maxY);

            float newX = (grateDirection.x / grateDirection.y) * clampY;

            currDistance += Vector3.Distance(grateObj.transform.localPosition, new Vector3(newX, clampY, 0));

            grateObj.transform.localPosition = new Vector2(newX, clampY);

            // Count end hits and optionally play a small tick
            if (grateObj.transform.position.y <= minY && !topSide)
            {
                gratingCount++;
                topSide = true;
                PlayGrateTick();
            }
            if (grateObj.transform.position.y >= maxY && topSide)
            {
                gratingCount++;
                topSide = false;
                PlayGrateTick();
            }
        }

        // Stop grating when mouse released
        if (Input.GetMouseButtonUp(0))
        {
            grating = false;
            StopGratingSfx();
        }

        // Transition when enough distance grated
        if (currDistance >= totalDistance)
        {
            StopGratingSfx();
            Debug.Log("Transition");
            CookingManager.instance.Transition();
            grateInstructions.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void StartGratingSfx()
    {
        if (gratingSfxStarted) return;
        if (!gratingInstance.isValid()) return;

        gratingInstance.start();
        gratingSfxStarted = true;
    }

    private void StopGratingSfx()
    {
        if (!gratingSfxStarted) return;
        if (!gratingInstance.isValid()) return;

        gratingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        gratingSfxStarted = false;
    }

    private void PauseGratingSfx(bool pause)
    {
        if (!gratingSfxStarted) return;
        if (!gratingInstance.isValid()) return;

        gratingInstance.setPaused(pause);
    }

    private void PlayGrateTick()
    {
        if (string.IsNullOrEmpty(grateTickOneShotEvent)) return;
        RuntimeManager.PlayOneShot(grateTickOneShotEvent, transform.position);
    }

    private void OnDisable()
    {
        StopGratingSfx();

        if (gratingInstance.isValid())
        {
            gratingInstance.release();
        }
    }

    private void OnDestroy()
    {
        if (gratingInstance.isValid())
        {
            gratingInstance.release();
        }
    }
}