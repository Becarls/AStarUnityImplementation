using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperties : MonoBehaviour {

    public bool closed = false;
    public bool inOpen = false;
    public int f = 0;
    public int g = 0;
    public int h = 0;
    public int x = -1;
    public int y = -1;
    [Range(0, 4)]
    public int state = 0;
    public TileProperties parent = null; //this will be used in pathfinding script
    public Material mat;

    void Start() {
        Renderer rend = GetComponent<Renderer>();
        mat = rend.material;
    }
    void Update() {
        //start
        if (state == 1) {
            mat.color = Color.green;
        }
        //end
        else if(state == 2) {
            mat.color = Color.yellow;
        }
        //terrain
        else if (state == 3) {
            mat.color = Color.black;
        }
        //path
        else if(state == 4) {
            mat.color = Color.red;
        }
        else {
            mat.color = new Color(.7608f, .7608f, .7608f, .23f);
        }
    }

    void OnMouseEnter() {
        if (state != 3) {
            Color brighter = new Color(.4f, .4f, .4f, 0f);
            mat.SetColor("_EmissionColor", brighter);
        }  
    }

    void OnMouseExit() {
            mat.SetColor("_EmissionColor", Color.black);
    }
}
