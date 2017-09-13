using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

    public List<List<TileProperties>> board;
    [SerializeField]
    private Transform game_board_transform;
    [SerializeField]
    private OnClick click_script;

    /*
    States legend:
    State 0 is nothing
    State 1 is the start position, color is green
    State 2 is the path, color is red
    State 3 is the terrain, color is black
    State 4 is the destination, color is yellow
    */

    private bool first = true;
    private int[] start = new int[2];
    private int[] goal  = new int[2];
    private TileProperties last_start;
    private TileProperties last_goal;
    
    //Untested Binary heap data structure to maintain tile with 
    // lowest f score at the top
    public class BinaryHeap{
        private List<TileProperties> heap = new List<TileProperties>(new TileProperties[1]);
        private int numberOfItems = 1; //not gonna 0 index for reasons
        
        public void Add(TileProperties newTile) { 
            heap.Add(newTile);

            int bubbleIndex = numberOfItems;
            while (bubbleIndex != 1) {
                int parentIndex = bubbleIndex / 2;
                if (heap[parentIndex].f >= heap[bubbleIndex].f) {
                    TileProperties temp = heap[parentIndex];
                    heap[parentIndex] = heap[bubbleIndex];
                    heap[bubbleIndex] = temp;
                    bubbleIndex = parentIndex;
                }
                else {
                    break;
                }
            }//while
            numberOfItems += 1;
        }//Add
    

        public TileProperties Pop() {
            numberOfItems -= 1;
            TileProperties retval = heap[1];

            //Fix the heap
            heap[1] = heap[numberOfItems];
            int swap = 1, parent = 1;
            do {
                //two children
                if (2 * parent + 1 <= numberOfItems) {
                    if (heap[parent].f >= heap[2 * parent].f) {
                        swap = 2 * parent;
                    }
                    if (heap[swap].f >= heap[2 * parent + 1].f) {
                        swap = 2 * parent + 1;
                    }
                }//if 

                //if only one child
                else if (2 * parent <= numberOfItems) {
                    //is this redundant? techinically yes, now shut up
                    if (heap[parent].f >= heap[2 * parent].f) {
                        swap = 2 * parent;
                    }
                }//else if

                if(parent != swap) {
                    //this condition may be redundant...
                    TileProperties temp = heap[parent];
                    heap[parent] = heap[swap];
                    heap[swap] = temp;
                }

            } while (parent == swap);

            return retval;
        }//Pop

        public void UpdateHeap() {
            TileProperties tile = Pop();
            Add(tile);
        }

        public void Print() {
            Debug.Log("Printing heap: ");
            for (int i = 1; i < numberOfItems - 1; i += 1) {
                Debug.Log(heap[i]);
            }
        }

        public bool isEmpty() {
            if(numberOfItems >= 1) { return true; }
            return false;
        }
    }//BinaryHeap

    //Initialize our game board, each gameobject in this 2d list is a tile
    void Start () {
        board = new List<List<TileProperties>>();
        foreach (Transform child in game_board_transform) {
            board.Add(new List<TileProperties>());
            foreach (Transform grandchild in child.transform) {
                board[board.Count - 1].Add(grandchild.gameObject.GetComponent<TileProperties>());
            }
        }

        //SMALL UNIT TEST

        //TileProperties t1 = new TileProperties();
        //t1.f = 2;
        //TileProperties t2 = new TileProperties();
        //t2.f = 1;
        //TileProperties t3 = new TileProperties();
        //t3.f = 14;
        //TileProperties t4 = new TileProperties();
        //t4.f = 3;
        //TileProperties t5 = new TileProperties();
        //t5.f = 2;

        //BinaryHeap bhep = new BinaryHeap();
        //bhep.Add(t1);
        //bhep.Add(t2);
        //bhep.Add(t3);
        //bhep.Add(t4);
        //bhep.Add(t5);

        //Debug.Log(bhep.Pop().f);
        //Debug.Log(bhep.Pop().f);
        //t4.f = 7;
        //bhep.Update();
        //bhep.Add(t2);
        //Debug.Log(bhep.Pop().f);
    }

    void Update() {
        if (click_script.last_goal == null || click_script.last_start == null) { return; }
        if (click_script.last_goal == click_script.last_start && click_script.last_start != null) { return; }

        if (click_script.last_goal != last_goal || click_script.last_start != last_start) {
            start[0] = goal[0] = -1;
            for(int i = 0; i < board.Count; ++i) {
                for(int j = 0; j < board[0].Count; ++j) {
                    if (first) {
                        board[i][j].x = i;
                        board[i][j].y = j;
                    }
                    if (board[i][j].state == 1) { start[0] = i; start[1] = j; }
                    else if (board[i][j].state == 2) { goal[0] = i; goal[1] = j; }
                    else if(board[i][j].state == 4) { board[i][j].state = 0; }
                    //if (start[0] >= 0 & goal[0] >= 0) { break; }
                }
                //if (start[0] >= 0 & goal[0] >= 0) { break; }
            }
            first = false;
            last_goal = click_script.last_goal;
            last_start = click_script.last_start;
            AstarAlgorithm(start, goal);
        }

        last_goal = click_script.last_goal;
        last_start = click_script.last_start;
    }


    void h(int x, int y, int x_goal, int y_goal, TileProperties tile) {
        int x_dist = Mathf.Abs(x_goal - x);
        int y_dist = Mathf.Abs(y_goal - y);
        if(tile.h == 0) {
            tile.h = 10 * (x_dist + y_dist);
        }
    }

    void g(TileProperties curr, TileProperties parent, ref TileProperties tile, bool diag) {
        int value;
        if (diag) {
            value = 14;
        }
        else {
            value = 10;
        }

        tile.g = parent.g + value;
        if ((parent.g + value) > (curr.g + value)){
            tile.g = curr.g + value;
            tile.parent = curr;       
        }
    }

    void f(ref TileProperties tile) {
        tile.f = tile.g + tile.h;
    }

    void AstarAlgorithm(int[] start, int[] goal) {
        //actually do the algorithm here
        BinaryHeap open_list = new BinaryHeap();

        int curr_x = start[0];
        int curr_y = start[1];
        int col_size = board[0].Count;
        int row_size = board.Count;
        board[curr_x][curr_y].closed = true;
        //add f scores and recalculations
        int counter = 0;
        while(!board[goal[0]][goal[1]].closed){
            //Debug.Log(curr_x);
            //Debug.Log(curr_y);
            counter += 1;
            //to the left
            if (curr_x - 1 >= 0) {
                check(curr_x, curr_y, curr_x - 1, curr_y, ref open_list, false);
            }
            //to the right
            if (curr_x + 1 < row_size) {
                check(curr_x, curr_y, curr_x + 1, curr_y, ref open_list, false);
            }
            //look up!
            if (curr_y - 1 >= 0) {
                check(curr_x, curr_y, curr_x, curr_y - 1, ref open_list, false);
            }
            //DUCK
            if (curr_y + 1 < col_size) {
                check(curr_x, curr_y, curr_x, curr_y + 1, ref open_list, false);
            }
            //NW
            if (curr_x - 1 >= 0 & curr_y - 1 >= 0) {
                check(curr_x, curr_y, curr_x - 1, curr_y - 1, ref open_list, true);
            }
            //NE
            if (curr_x + 1 < row_size & curr_y - 1 >= 0) {
                check(curr_x, curr_y, curr_x + 1, curr_y - 1, ref open_list, true);
            }
            //SW
            if (curr_x - 1 >= 0 & curr_y + 1 < col_size) {
                check(curr_x, curr_y, curr_x - 1, curr_y + 1, ref open_list, true);
            }
            //SE
            if (curr_x + 1 < row_size & curr_y + 1 < col_size) {
                check(curr_x, curr_y, curr_x + 1, curr_y + 1, ref open_list, true);
            }

            open_list.UpdateHeap();
            open_list.Print();
            TileProperties next_spot = open_list.Pop();
            next_spot.parent = board[curr_x][curr_y];
            next_spot.closed = true;
            
            //Update the next postion
            curr_x = next_spot.x;
            curr_y = next_spot.y;
            
            if(curr_x < 0 || curr_y < 0) {
                Debug.Log("NEGATIVE INDEX");
                return;
            }
        } //while(!board[goal[0]][goal[1]].closed || open_list.isEmpty());  //while

        TileProperties path = board[goal[0]][goal[1]].parent;
        while (path.parent != null) {
            path.state = 4;
            path.closed = false;
            path = path.parent;
        }

        //reset
        for (int i = 0; i < board.Count; ++i) {
            for (int j = 0; j < board[0].Count; ++j) {
                board[i][j].closed = false;
                board[i][j].f = 0;
                board[i][j].g = 0;
                board[i][j].h = 0;
                board[i][j].parent = null;
            }
        }
    }

    void check(int curr_x, int curr_y, int x, int y, ref BinaryHeap open_list, bool diag) {
        TileProperties tile = board[x][y];
        if (!(tile.state == 3 || tile.closed)) {
            if (tile.parent == null) { tile.parent = board[curr_x][curr_y]; }
            h(x, y, goal[0], goal[1], tile);
            g(board[curr_x][curr_y], tile.parent, ref tile, diag);
            f(ref tile);
            if (tile.inOpen) {
                open_list.UpdateHeap();
            }
            else {
                tile.inOpen = true;
                open_list.Add(tile);
            }
        }
    }
}