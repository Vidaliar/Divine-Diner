using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NewMouse : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private static float offSetX = 0;
    [SerializeField] private static float offSetY = 0;
    [SerializeField] Vector3 offset = new Vector3(offSetX, offSetY, 0);

    [SerializeField] public Canvas canvas;
    public RectTransform rectTransform;
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        debugShow();
        Vector2 mouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                                                                                    Input.mousePosition,
                                                                                    canvas.worldCamera,
                                                                                    out mouse);
        transform.position = canvas.transform.TransformPoint(mouse) + offset;
    }

    void debugShow()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Cursor.visible = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Cursor.visible = false;
        }
    }
}
