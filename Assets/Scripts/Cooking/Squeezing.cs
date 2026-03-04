using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Squeezing : MonoBehaviour
{
    // GameObject squeezeIngredient;    Likely to be used to differentiate the four types
    [SerializeField] GameObject topOfSqueezeTool;
    [SerializeField] Slider progressBar;
    float timeSec = 5.5f;   //Total time needs to be squeezed
    bool handSqueeze = false;   //If it is hand squeezing or tool squeezing
    float timer;    //Keeps track of time passed
    Coroutine currentAnim;  //Keeps track of the current animation

    bool started = false;

    KeyCode currKeyCode;
    Dictionary<KeyCode, List<KeyCode>> keyCodeDict = new Dictionary<KeyCode, List<KeyCode>>();

    Dictionary<KeyCode, KeyCode> clockwiseCodes = new Dictionary<KeyCode, KeyCode>(){{KeyCode.A,KeyCode.W}, {KeyCode.W,KeyCode.D}, {KeyCode.D,KeyCode.S}, {KeyCode.S,KeyCode.A}};
    Dictionary<KeyCode, KeyCode> counterCodes = new Dictionary<KeyCode, KeyCode>()
    {{KeyCode.W,KeyCode.A}, {KeyCode.D,KeyCode.W}, {KeyCode.S,KeyCode.D}, {KeyCode.A,KeyCode.S}};

    Event e;

    bool inPause = false;
    public CookingManager cManager;
    void Awake()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = timeSec;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        //Setting up keyCodeDict
        List<KeyCode> ADList = new List<KeyCode>();
        ADList.Add(KeyCode.A);
        ADList.Add(KeyCode.D);

        List<KeyCode> WSList = new List<KeyCode>();
        ADList.Add(KeyCode.W);
        ADList.Add(KeyCode.S);

        keyCodeDict.Add(KeyCode.A, WSList);
        keyCodeDict.Add(KeyCode.W, ADList);
        keyCodeDict.Add(KeyCode.D, WSList);
        keyCodeDict.Add(KeyCode.S, ADList);
    }

    // void OnGUI()
    // {
    //     e = Event.current;
    //     //Get key press (any should work to start)
    //     //From initial press, figure out next 2 possible acceptable presses
    //     if(!started/*Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)*/)
    //     {
    //         if(e.type.Equals(EventType.KeyDown)){
    //         started = true;
    //         // if(Input.GetKeyDown(KeyCode.W)) currKeyCode = KeyCode.W;
    //         // else if (Input.GetKeyDown(KeyCode.A)) currKeyCode = KeyCode.A;
    //         // else if (Input.GetKeyDown(KeyCode.S)) currKeyCode = KeyCode.S;
    //         // else if (Input.GetKeyDown(KeyCode.D)) currKeyCode = KeyCode.D;
    //         currKeyCode = e.keyCode;
    //         }
    //     }
        
    //     else if(e.type.Equals(EventType.KeyDown))
    //     {
    //         if(keyCodeDict[currKeyCode].Contains(e.keyCode))
    //         {
                
    //             Debug.Log("Next key is good");
    //             currKeyCode = e.keyCode;
    //         }
    //         else
    //         {
    //             Debug.Log("Next key NOT good");
    //         }
    //         Debug.Log(currKeyCode.ToString() + " is curr key code");
    //         foreach(KeyCode key in keyCodeDict[currKeyCode])
    //         {
    //             Debug.Log(key.ToString());
    //         }
    //         Debug.Log("After for loop");
    //     }
    // }
    void Update()
    {
        // inPause = cManager.inPause;
        inPause = CookingManager.instance.inPause;
        if (inPause) return; // Makes sure game isn't paused before anything happens


        // e = Event.current;
        // //Get key press (any should work to start)
        // //From initial press, figure out next 2 possible acceptable presses
        if(!started /*&& e != null*//*Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)*/)
        {
            // if(e.type.Equals(EventType.KeyDown)){
            started = true;
            if(Input.GetKeyDown(KeyCode.W)) currKeyCode = KeyCode.W;
            else if (Input.GetKeyDown(KeyCode.A)) currKeyCode = KeyCode.A;
            else if (Input.GetKeyDown(KeyCode.S)) currKeyCode = KeyCode.S;
            else if (Input.GetKeyDown(KeyCode.D)) currKeyCode = KeyCode.D;
            // currKeyCode = e.keyCode;
            // }
        }
        
        // else if(e.type.Equals(EventType.KeyDown))
        // {
        //     if(keyCodeDict[currKeyCode].Contains(e.keyCode))
        //     {
        //         Debug.Log("Next key is good");
        //     }
        //     else
        //     {
        //         Debug.Log("Next key NOT good");
        //     }
        // }


        //Starts the squeezing animation
        if (Input.GetMouseButtonDown(0))
        {
            if(currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(ToolSqueezeAnim());
        }

        //While the player holds down the mouse, progress the timer and bar
        if (Input.GetMouseButton(0))
        {
            timer += Time.deltaTime;
            progressBar.value = timer;

            if (timer >= timeSec)
            {
                progressBar.gameObject.SetActive(false);
                CookingManager.instance.Transition();
                this.gameObject.SetActive(false);
            }
        }

        //If the player lets go of the mouse, start the release animation
        if (Input.GetMouseButtonUp(0))
        {
            if (!handSqueeze)
            {
                if (currentAnim != null) StopCoroutine(currentAnim);
                currentAnim = StartCoroutine(ToolReleaseAnim());
            }
        }
    }

    //Does the squeeze animation for the tool
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
    
    //Does the release animation for the tool
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