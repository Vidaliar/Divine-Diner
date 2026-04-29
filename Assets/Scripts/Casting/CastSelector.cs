using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSelector : MonoBehaviour
{
    [SerializeField] GameObject castSpicy;
    [SerializeField] GameObject castPlayful;
    [SerializeField] GameObject castFancy;
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
        float camSize = Camera.main.orthographicSize * 6;
        Vector3 spicyPos = new Vector3(camPos.x + camSize/3, camPos.y, camPos.z);
        Vector3 fancyPos = new Vector3(camPos.x - camSize/3, camPos.y, camPos.z);

        GameObject playful = Instantiate(castPlayful, camPos, Quaternion.identity);
        playful.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        playful.GetComponent<LineRenderer>().SetWidth(0.2f,0.2f);

        GameObject spicy = Instantiate(castSpicy, spicyPos, Quaternion.identity);
        spicy.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        spicy.GetComponent<LineRenderer>().SetWidth(0.2f,0.2f);
        
        GameObject fancy = Instantiate(castFancy, fancyPos, Quaternion.identity);

        
    }
}
