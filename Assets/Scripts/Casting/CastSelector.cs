using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSelector : MonoBehaviour
{
    [SerializeField] GameObject castSpicy;
    [SerializeField] GameObject castPlayful;
    [SerializeField] GameObject castFancy;

    [SerializeField] CastManager castManager;

    [SerializeField] Vector2 selectionBoxSize = new Vector2(5f, 5f);

    List<GameObject> spawnedCasts = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Show all three
        //Make all 3 selectable (buttons?)
        //Once one is selected, pass it to CastManager
        //Destroy all three selectable objects
        //Move on to CastManager script
        PlaceCasts();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlaceCasts()
    {
        Vector3 camPos = Camera.main.transform.position;
        float camSize = Camera.main.orthographicSize;
        float camWidth = camSize * Camera.main.aspect;

        // In 2D, do not use camera z position.
        // Camera is usually at z = -10, while gameplay objects are usually at z = 0.
        float castZ = 0f;

        Vector3 spicyPos = new Vector3(camPos.x - camWidth * 0.5f, camPos.y, castZ);
        Vector3 playfulPos = new Vector3(camPos.x, camPos.y, castZ);
        Vector3 fancyPos = new Vector3(camPos.x + camWidth * 0.5f, camPos.y, castZ);

        GameObject playful = Instantiate(castPlayful, playfulPos, Quaternion.identity);
        playful.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        playful.GetComponent<LineRenderer>().SetWidth(0.2f, 0.2f);
        SetupSelectable(playful, castPlayful);

        GameObject spicy = Instantiate(castSpicy, spicyPos, Quaternion.identity);
        spicy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        spicy.GetComponent<LineRenderer>().SetWidth(0.2f, 0.2f);
        SetupSelectable(spicy, castSpicy);
        
        GameObject fancy = Instantiate(castFancy, fancyPos, Quaternion.identity);
        fancy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        fancy.GetComponent<LineRenderer>().SetWidth(0.2f, 0.2f);
        SetupSelectable(fancy, castFancy);
    }

    void SetupSelectable(GameObject castObject, GameObject castPrefab)
    {
        spawnedCasts.Add(castObject);

        BoxCollider2D boxCollider = castObject.GetComponent<BoxCollider2D>();

        if (boxCollider == null)
        {
            boxCollider = castObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = true;
        boxCollider.size = selectionBoxSize;

        CastSelectable selectable = castObject.GetComponent<CastSelectable>();

        if (selectable == null)
        {
            selectable = castObject.AddComponent<CastSelectable>();
        }

        selectable.SetCast(this, castPrefab);
    }

    public void SelectCast(GameObject selectedCast)
    {
        if (castManager == null)
        {
            castManager = FindObjectOfType<CastManager>();
        }

        castManager.SetCast(selectedCast);

        foreach (GameObject castObject in spawnedCasts)
        {
            Destroy(castObject);
        }

        spawnedCasts.Clear();

        gameObject.SetActive(false);
    }

    public void SelectSpicy()
    {
        SelectCast(castSpicy);
    }

    public void SelectPlayful()
    {
        SelectCast(castPlayful);
    }

    public void SelectFancy()
    {
        SelectCast(castFancy);
    }
}

public class CastSelectable : MonoBehaviour
{
    CastSelector selector;
    GameObject castPrefab;

    public void SetCast(CastSelector newSelector, GameObject newCastPrefab)
    {
        selector = newSelector;
        castPrefab = newCastPrefab;
    }

    void OnMouseDown()
    {
        selector.SelectCast(castPrefab);
    }
}