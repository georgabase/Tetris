using UnityEngine;
using System.Collections;

public class PlayGameScript : MonoBehaviour
{

    public Sprite idlePlay;
    public Sprite chosenPlay;
    public Sprite idleExit;
    public Sprite chosenExit;
    public KeyCode up;
    public KeyCode down;
    public KeyCode choose;
    public GameObject playObject;
    public GameObject quitObject;
    private int choice;
    private SpriteRenderer spriteRendererPlay;
    private SpriteRenderer spriteRendererQuit;
    // Use this for initialization
    void Start()
    {

        spriteRendererPlay = playObject.GetComponent<SpriteRenderer>(); // we are accessing the SpriteRenderer that is attached to the Gameobject
        spriteRendererQuit = quitObject.GetComponent<SpriteRenderer>(); // we are accessing the SpriteRenderer that is attached to the Gameobject
        choice = 1;

        if ((choice % 2) == 1)
        {
            spriteRendererPlay.sprite = chosenPlay;
            spriteRendererQuit.sprite = idleExit;
        }
        else
        {
            spriteRendererPlay.sprite = idlePlay;
            spriteRendererQuit.sprite = chosenExit;
        }
	
    }
	
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(up))
        {
            choice--;
			
            if ((choice % 2) == 1)
            {
                spriteRendererPlay.sprite = chosenPlay;
                spriteRendererQuit.sprite = idleExit;
            }
            else
            {
                spriteRendererPlay.sprite = idlePlay;
                spriteRendererQuit.sprite = chosenExit;
            }
        }
        else if (Input.GetKeyDown(down))
        {
            choice++;
            if ((choice % 2) == 1)
            {
                spriteRendererPlay.sprite = chosenPlay;
                spriteRendererQuit.sprite = idleExit;
            }
            else
            {
                spriteRendererPlay.sprite = idlePlay;
                spriteRendererQuit.sprite = chosenExit;
            }
        }
        else if (Input.GetKeyDown(choose))
        {
            if ((choice % 2) == 1)
            {
                Application.LoadLevel(0);
            }
            else
            {
                Application.Quit();
            }
        }


    }
}
