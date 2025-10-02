using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FMODUnity;

public class Seasoning : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] Transform shaker;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] float movementOffset = 1f;
    [SerializeField] int totalShakes = 10;
    [Header("FMOD SFX")]
    [SerializeField] StudioEventEmitter seasoningLoopEmitter;   // assign in Inspector

    Vector2 startPos;
    
    int shakeCount = 0;
    bool nextKeyIsD;

    void OnEnable() { if (seasoningLoopEmitter) seasoningLoopEmitter.Play(); }
    void OnDisable() { if (seasoningLoopEmitter) seasoningLoopEmitter.Stop(); }

    void Start()
    {
        controlsText.SetActive(true);
        startPos = shaker.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && !nextKeyIsD)
        {
            shaker.position = new Vector2(startPos.x - movementOffset, startPos.y);
            nextKeyIsD = true;
            shakeCount++;
            particleSystem.Play();
        }

        else if (Input.GetKeyDown(KeyCode.D) && nextKeyIsD)
        {
            shaker.position = new Vector2(startPos.x + movementOffset, startPos.y);
            nextKeyIsD = false;
            shakeCount++;
            particleSystem.Play();
        }

        if (shakeCount >= totalShakes)
        {
            CookingManager.instance.Transition();
            controlsText.SetActive(false);
            gameObject.SetActive(false);
            particleSystem.gameObject.SetActive(false);
        }
    }
}
