using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MorphingManager : MonoBehaviour
{
    //Public (Editable in Inspector)
    public List<GameObject> formList = new List<GameObject>();
    public GameObject UIFormSpriteHolder; 

    //Private
    private SpriteRenderer parentSpriteRenderer;
    private int chosenFormIndex;
    
    

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
        chosenFormIndex = 1;
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

        if (Input.GetButtonDown("MoveChosenFormUp"))
        {
            chosenFormUp();
        }

        if (Input.GetButtonDown("MoveChosenFormDown"))
        {
            chosenFormDown();
        }
    }

    public void morphToChosenForm()
    {
        parentSpriteRenderer.sprite = formList[chosenFormIndex].GetComponent<SpriteRenderer>().sprite;
        

        //Rather than setting sprites, we may eventually just want to have all the Rigidbodies and
        //Colliders and everything else in these children, and just selectively enable/disable them

        //That way we can have all the properties we want set ahead of time, and these morphs don't have to reset a bunch
        //of properties
    }

    public void morphToBaseForm()
    {
        parentSpriteRenderer.sprite = formList[0].GetComponent<SpriteRenderer>().sprite;

    }

    public void chosenFormUp()
    {
        chosenFormIndex += 1;

        //If is for wrapping around the list if we go above
        if (chosenFormIndex >= (formList.Count)) 
        {
            chosenFormIndex = 1;
        }

        //Update the sprite in the UI
        UIFormSpriteHolder.GetComponent<Image>().sprite = formList[chosenFormIndex].GetComponent<SpriteRenderer>().sprite;
    }

    public void chosenFormDown()
    {
        chosenFormIndex -= 1;

        //If is for wrapping around the list if we go below
        if (chosenFormIndex <= 0)
        {
            chosenFormIndex = (formList.Count - 1); //Need the minus one because indexing starts at zero
        }

        //Update the sprite used in the UI
        UIFormSpriteHolder.GetComponent<Image>().sprite = formList[chosenFormIndex].GetComponent<SpriteRenderer>().sprite;
    }

}
