using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorphingManager : MonoBehaviour
{
    public List<GameObject> formList = new List<GameObject>();
    private SpriteRenderer parentSpriteRenderer;
    

    // Start is called before the first frame update
    void Start()
    {
        parentSpriteRenderer = GetComponent<SpriteRenderer>();


        foreach (Transform child in transform)
        {
            //I sure hope this builds the list in a predictable order, with base at 0 and other forms after it
            if (child.gameObject.tag == "PlayerForm")
            {
                formList.Add(child.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //FYI I had MorphButton mapped to Left Shift
        if (Input.GetButtonDown("MorphButton"))
        {
            morphToChosenForm();
        }
        if (Input.GetButtonUp("MorphButton"))
        {
            morphToBaseForm();
        }
    }

    public void morphToChosenForm()
    {
        parentSpriteRenderer.sprite = formList[1].GetComponent<SpriteRenderer>().sprite;

        //Rather than setting sprites, we may eventually just want to have all the Rigidbodies and
        //Colliders and everything else in these children, and just selectively enable/disable them

        //That way we can have all the properties we want set ahead of time, and these morphs don't have to reset a bunch
        //of properties
    }

    public void morphToBaseForm()
    {
        parentSpriteRenderer.sprite = formList[0].GetComponent<SpriteRenderer>().sprite;
    }
}
