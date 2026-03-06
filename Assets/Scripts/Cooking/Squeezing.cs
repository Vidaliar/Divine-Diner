using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using FMODUnity;
using FMOD.Studio;

public class Squeezing : MonoBehaviour
{
    [SerializeField] GameObject topOfSqueezeTool;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject instructions;
    [SerializeField] Transform juicedObj;
    [SerializeField] int totalRounds = 5; // 1 round = WASD

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string squeezeStepEvent;      // plays on each valid squeeze input

    [FMODUnity.EventRef]
    [SerializeField] private string squeezeCompleteEvent;  // optional finish sting

    private int numKeys = 0;
    float timeSec = 5.5f;
    bool handSqueeze = false;
    float timer;
    Coroutine currentAnim;

    bool started = false;

    KeyCode currKeyCode;
    KeyCode startKeyCode;
    Dictionary<KeyCode, List<KeyCode>> keyCodeDict = new Dictionary<KeyCode, List<KeyCode>>();

    Dictionary<KeyCode, KeyCode> clockwiseCodes = new Dictionary<KeyCode, KeyCode>()
    {
        {KeyCode.A, KeyCode.W},
        {KeyCode.W, KeyCode.D},
        {KeyCode.D, KeyCode.S},
        {KeyCode.S, KeyCode.A}
    };

    Dictionary<KeyCode, KeyCode> counterCodes = new Dictionary<KeyCode, KeyCode>()
    {
        {KeyCode.W, KeyCode.A},
        {KeyCode.D, KeyCode.W},
        {KeyCode.S, KeyCode.D},
        {KeyCode.A, KeyCode.S}
    };

    Dictionary<KeyCode, KeyCode> currKeyCodeDict = null;

    float startYJuicedObj;
    float endYJuicedObj = -0.9f;

    Event e;

    bool inPause = false;
    public CookingManager cManager;

    void Awake()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = 1;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        instructions.SetActive(true);

        // Setting up keyCodeDict
        List<KeyCode> ADList = new List<KeyCode>();
        ADList.Add(KeyCode.A);
        ADList.Add(KeyCode.D);

        List<KeyCode> WSList = new List<KeyCode>();
        WSList.Add(KeyCode.W);
        WSList.Add(KeyCode.S);

        keyCodeDict.Add(KeyCode.A, WSList);
        keyCodeDict.Add(KeyCode.W, ADList);
        keyCodeDict.Add(KeyCode.D, WSList);
        keyCodeDict.Add(KeyCode.S, ADList);

        startYJuicedObj = juicedObj.position.y;
    }

    void OnGUI()
    {
        inPause = CookingManager.instance.inPause;
        if (inPause) return;

        e = Event.current;

        if (!started)
        {
            if (e.type.Equals(EventType.KeyDown))
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    currKeyCode = e.keyCode;
                    started = true;
                    startKeyCode = e.keyCode;

                    Debug.Log("" + currKeyCode);
                    numKeys++;
                    JuiceAnimation();
                    PlaySqueezeStepSfx();
                    Debug.Log("Started is now true woah");
                }
            }
        }
        else if (started && currKeyCodeDict == null && e.type.Equals(EventType.KeyDown))
        {
            KeyCode clockwise = clockwiseCodes[startKeyCode];
            KeyCode counter = counterCodes[startKeyCode];

            if (clockwise == e.keyCode)
            {
                currKeyCodeDict = clockwiseCodes;
                currKeyCode = e.keyCode;
                Debug.Log("Clockwise dict");
                numKeys++;
                JuiceAnimation();
                PlaySqueezeStepSfx();
            }
            else if (counter == e.keyCode)
            {
                currKeyCodeDict = counterCodes;
                currKeyCode = e.keyCode;
                Debug.Log("Counter clockwise dict");
                numKeys++;
                JuiceAnimation();
                PlaySqueezeStepSfx();
            }

            Debug.Log("" + currKeyCode);
        }
        else if (currKeyCodeDict != null && e.type.Equals(EventType.KeyDown))
        {
            if (e.keyCode == currKeyCodeDict[currKeyCode])
            {
                Debug.Log("Correct next code of " + e.keyCode);
                currKeyCode = e.keyCode;
                numKeys++;
                JuiceAnimation();
                PlaySqueezeStepSfx();
            }
        }

        if (numKeys >= totalRounds * 4)
        {
            PlaySqueezeCompleteSfx();
            CookingManager.instance.Transition();
            progressBar.gameObject.SetActive(false);
            instructions.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void JuiceAnimation()
    {
        float t = numKeys / (float)(totalRounds * 4);
        float newY = Mathf.Lerp(startYJuicedObj, endYJuicedObj, t);
        juicedObj.position = new Vector3(juicedObj.position.x, newY, juicedObj.position.z);

        progressBar.value = t;
    }

    private void PlaySqueezeStepSfx()
    {
        if (string.IsNullOrEmpty(squeezeStepEvent)) return;
        RuntimeManager.PlayOneShot(squeezeStepEvent);
    }

    private void PlaySqueezeCompleteSfx()
    {
        if (string.IsNullOrEmpty(squeezeCompleteEvent)) return;
        RuntimeManager.PlayOneShot(squeezeCompleteEvent);
    }

    IEnumerator ToolSqueezeAnim()
    {
        while (true)
        {
            float topAngle = topOfSqueezeTool.transform.localRotation.eulerAngles.z;

            if (topAngle > 0)
            {
                topAngle -= 360;
            }

            if (topAngle > -40)
            {
                topOfSqueezeTool.transform.Rotate(0, 0, -5);
            }
            else
            {
                break;
            }

            yield return null;
        }
    }

    IEnumerator ToolReleaseAnim()
    {
        while (true)
        {
            float topAngle = Mathf.RoundToInt(topOfSqueezeTool.transform.localRotation.eulerAngles.z);

            if (topAngle > 0)
            {
                topAngle -= 360;
            }

            if (topAngle < 0)
            {
                topOfSqueezeTool.transform.Rotate(0, 0, 5);
            }
            else
            {
                break;
            }

            yield return null;
        }
    }
}