using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClick : MonoBehaviour {

    private bool click0 = false;
    private bool click1 = false;
    private bool click2 = false;

    [HideInInspector]
    public TileProperties last_start = null;
    [HideInInspector]
    public TileProperties last_goal = null;
    

	//This code is a nested mess, sorry to anyone that has to read it

	void Update () {
        if (Input.GetMouseButtonDown(0)) { click0 = true; }
        if (Input.GetMouseButtonDown(1)) { click1 = true; }
        if (Input.GetMouseButtonDown(2)) { click2 = true; }

        if (click0 | click1 | click2) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)){
                if(hit.collider.tag == "Tile") {
                    TileProperties properties = hit.transform.GetComponent<TileProperties>();
                    //Left click and we are not on terrain, make it green if not green, make grey if anything else
                    if (click0 & properties.state != 3) {
                        //properties.state = (properties.state + 1) % 2;
                        if (last_start == null) {
                            last_start = properties;
                        }
                        else {
                            last_start.state = 0;
                        }
                        properties.state = 1;
                        last_start = properties;
                    }
                    else if(click1 & properties.state != 3) {
                        if(last_goal == null) {
                            last_goal = properties;
                        }
                        else {
                            last_goal.state = 0;
                        }
                        properties.state = 2;
                        last_goal = properties;
                    }
                    //Else we did a right click, so if we clicked terrain, make it not terrain, if it is not terrain, make it terrain
                    //the emission line there prevents it from being highlighted when creating the terrain, just a visual thing
                    else {
                        if(properties.state == 3) { properties.state = 0;}
                        else {
                            properties.state = 3;
                            properties.mat.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
        }

        click0 = false;
        click1 = false;
        click2 = false;
	}
}
