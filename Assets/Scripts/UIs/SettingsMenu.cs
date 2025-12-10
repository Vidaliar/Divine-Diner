using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject page1;
    [SerializeField] GameObject page2;
    [SerializeField] GameObject page3;
    [SerializeField] GameObject page4;

    [SerializeField] GameObject pauseMenu;

    [SerializeField] int defaultPage = 1;
    private void OnEnable()
    {
        ShowPage(defaultPage);
    }
    public void Back()
    {
        if (pauseMenu)
            pauseMenu.SendMessage("CloseSecondLevelAndReturnLevel1", SendMessageOptions.DontRequireReceiver);
        else
            gameObject.SetActive(false);
    }

    private void ShowPage(int pageIndex)
    {
        if (page1) page1.SetActive(pageIndex == 1);
        if (page2) page2.SetActive(pageIndex == 2);
        if (page3) page3.SetActive(pageIndex == 3);
        if (page4) page4.SetActive(pageIndex == 4);
    }

    public void ShowPage1() => ShowPage(1);
    public void ShowPage2() => ShowPage(2);
    public void ShowPage3() => ShowPage(3);
    public void ShowPage4() => ShowPage(4);
}
