using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeObserver
{
    public enum Swipe { Right, Left, Up, Down, None };

    public void Setup()
    {
        dragDistance = Screen.height * 15 / 100;
    }

    public Swipe CheckForSwipe()
    {
        if (Input.touchCount == 1) // user is touching the screen with a single touch
        {
            Touch touch = Input.GetTouch(0); // get the touch
            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                lp = touch.position;  //last touch position. Ommitted if you use list

                //Check if drag distance is greater than 20% of the screen height
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                {//It's a drag
                 //check if the drag is vertical or horizontal
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   //If the horizontal movement is greater than the vertical movement...
                        if ((lp.x > fp.x))  //If the movement was to the right)
                        {   //Right swipe
                            return Swipe.Right;
                        }
                        else
                        {   //Left swipe
                            return Swipe.Left;
                        }
                    }
                    else
                    {   //the vertical movement is greater than the horizontal movement
                        if (lp.y > fp.y)  //If the movement was up
                        {   //Up swipe
                            return Swipe.Up;
                        }
                        else
                        {   //Down swipe
                            return Swipe.Down;
                        }
                    }
                }
                else
                {   //It's a tap as the drag distance is less than 20% of the screen height
                    return Swipe.None;
                }
            }
        }
        return Swipe.None;
    }

    private Vector2 fp;
    private Vector2 lp;
    private float dragDistance;
}
