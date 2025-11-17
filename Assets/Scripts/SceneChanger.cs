using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private TMP_Text text;

    private void Start()
    {
        text = gameObject.GetComponentInChildren<TMP_Text>();
        text.text = sceneName;
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
