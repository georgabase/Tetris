#region USING
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#endregion

public class GameManager : MonoBehaviour
{
    #region GUI

    public GUIText levelValue;
    public GUIText linesClearedValue;
    public GUIText scoreValue;
    public GUIText gameOverTitle;
    public GUIText bestScoreValue;

    #endregion

    #region KEYCODES

    public KeyCode rotateButton;
    public KeyCode speedUpButton;
    public KeyCode leftButton;
    public KeyCode rightButton;
    public KeyCode turboButton;

    #endregion

    #region PUBLIC

    public static int score = 0;
    public static int rowsCleared = 0;

    public static int[,] blockSpace;

    public AudioSource moveAudio;

    public GameObject[] allBlockPieces;

    #endregion

    #region PRIVATE

    private int level = 0;


    private int currentIndex;
    private Vector3 move = new Vector3(0, -0.64f, 0);
    private Vector3 moveRight = new Vector3(0.64f, 0, 0);
    private Vector3 moveLeft = new Vector3(-0.64f, 0, 0);
    private System.Random randomGenerator = new System.Random();
    private GameObject[,] blockObjects;
    private GameObject currentBlock;
    private GameObject nextBlock;
    private List<int> clearedRows;
    private float time = 0.4f;
    private int previousLevel;
    private bool gameOver = false;
    private bool preventSpeedUp = false;
    private int framesUpdated = 0;

    #endregion


    // Use this for initialization
    void Start()
    {
        //our grid
        blockSpace = new int[21, 10];
        blockObjects = new GameObject[21, 10];

        //empty the grid
        for (int y = 0; y < blockSpace.GetLength(0); y++)
        {
            for (int x = 0; x < blockSpace.GetLength(1); x++)
            { 
                blockSpace[y, x] = 0;
            }
        }

        previousLevel = level;
        Time.fixedDeltaTime = time;

        GameObject temp = allBlockPieces[randomGenerator.Next() % allBlockPieces.Length];

        currentBlock = (GameObject)Instantiate(temp, new Vector2(CalculateXTransform(5), CalculateYTransform(0)), temp.transform.rotation);
        temp = allBlockPieces[randomGenerator.Next() % allBlockPieces.Length];
        nextBlock = (GameObject)Instantiate(temp, new Vector2(0.6f, 2.0f), temp.transform.rotation);

        levelValue.text = level.ToString();
        scoreValue.text = score.ToString();
        bestScoreValue.text = PlayerPrefs.GetInt("Score").ToString();
        linesClearedValue.text = rowsCleared.ToString();

        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }

    void Update()
    {
        if (!gameOver)
        {
            if (Input.GetKeyDown(rotateButton))
            {
                Rotate();
                moveAudio.Play();
            }

            if (framesUpdated == 10) //skip the empty tile every 10 frames
            {

                if (Input.GetKey(speedUpButton))
                {
                    if (!preventSpeedUp)
                    {
                        
                        if (ShouldMoveDown())
                        {
                            currentBlock.transform.position += move;
                        } 
                    }
                }
                framesUpdated = 0;
            }
            else
            {
                framesUpdated++; //update frames
            }

            if (Input.GetKeyDown(rightButton) && !Input.GetKey(leftButton))
            {
                if (ShouldMoveRight())
                {
                    currentBlock.transform.position += moveRight;
                }
            }

            if (Input.GetKeyDown(leftButton) && !Input.GetKey(rightButton))
            {
                if (ShouldMoveLeft())
                {
                    currentBlock.transform.position += moveLeft;
                }
            }

            if (Input.GetKeyDown(speedUpButton))
            {
                preventSpeedUp = false;
            }
            if (Input.GetKeyDown(turboButton))
            {
                currentBlock.transform.position += (move * ShouldMoveDownReallyFast()); 
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    public void Rotate()
    {
        if (!currentBlock.name.Contains("OShapeBlock"))
        {
            //calculates 1st blocks index, because 1st block in the prefab is always the center block
            float yLocationCenter = currentBlock.transform.GetChild(1).gameObject.transform.position.y;
            float xLocationCenter = currentBlock.transform.GetChild(1).gameObject.transform.position.x;
            int yIndexCenter = CalculateYIndex(yLocationCenter);
            int xIndexCenter = CalculateXIndex(xLocationCenter);

            List<Vector2> tempTransforms = new List<Vector2>();

            // loops through every block in the prefab and checks its position according to the central block
            for (int i = 0; i < 4; i++)
            {
                float yLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.y;
                float xLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.x;
                int yIndex = CalculateYIndex(yLocation);
                int xIndex = CalculateXIndex(xLocation);

                int yDistance = yIndex - yIndexCenter;
                int xDistance = xIndex - xIndexCenter;

                int yIndexAfter = yIndex;
                int xIndexAfter = xIndex;

                if (!(xDistance == 0 && yDistance == 0)) //if its not the central block
                {
                    if (xDistance == 0 || yDistance == 0) //if its in the straight line from the  central block
                    {
                        if (xDistance == 0 && yDistance > 0) //higher than the central block
                        {
                            yIndexAfter = yIndexCenter;
                            xIndexAfter = xIndexCenter + yDistance;
                        }
                        else if (xDistance > 0 && yDistance == 0) //to the right of the central block
                        {
                            yIndexAfter = yIndexCenter - xDistance;
                            xIndexAfter = xIndexCenter;
                        }
                        else if (xDistance == 0 && yDistance < 0) //lower than the central block
                        {
                            yIndexAfter = yIndexCenter;
                            xIndexAfter = xIndexCenter + yDistance;
                        }
                        else if (xDistance < 0 && yDistance == 0) //to the left of the central block
                        {
                            yIndexAfter = yIndexCenter - xDistance;
                            xIndexAfter = xIndexCenter;
                        }
                    }
                    else //if its not on the straight line
                    {
                        int slope = yDistance / xDistance; //calculate the angle

                        if (slope < 0)
                        {
                            yIndexAfter = yIndexCenter + yDistance;
                            xIndexAfter = xIndexCenter - xDistance;
                        }
                        else if (slope > 0)
                        {
                            yIndexAfter = yIndexCenter - yDistance;
                            xIndexAfter = xIndexCenter + xDistance;

                        }
                    }

                    //if higher than the field
                    if (yIndexAfter > 20)
                    {
                        break;
                    }

                    //if cant rotate because of right wall
                    if (xIndexAfter > 9)
                    {
                        if (ShouldMoveLeft())
                        {
                            currentBlock.transform.position += moveLeft;
                            Rotate();
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //if lower than the field
                    if (yIndexAfter < 0)
                    {
                        break;
                    }

                    //if cant rotate because of left wall
                    if (xIndexAfter < 0)
                    {
                        if (ShouldMoveRight())
                        {
                            currentBlock.transform.position += moveRight;
                            Rotate();
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //if hits another block
                    if (blockSpace[yIndexAfter, xIndexAfter] == 1)
                    {
                        if (yIndexAfter != yIndex)
                        {
                            break;
                        }
                        else if (xIndexAfter > xIndex)
                        {
                            if (ShouldMoveLeft())
                            {
                                currentBlock.transform.position += moveLeft;
                                Rotate();
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if (xIndexAfter < xIndex)
                        {
                            if (ShouldMoveRight())
                            {
                                currentBlock.transform.position += moveRight;
                                Rotate();
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                }
                //add to temp list
                tempTransforms.Add(new Vector2(CalculateXTransform(xIndexAfter), CalculateYTransform(yIndexAfter)));
            }
            //if done with all 4 blocks
            if (tempTransforms.Count == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    currentBlock.transform.GetChild(i).gameObject.transform.position = tempTransforms[i];
                }
            }
        }
        //currentBlock.transform.Rotate(0, 0, -90);// transform.Rotate(0, 0, -90);

    }

    void FixedUpdate()
    {
        if (!gameOver)
        {
            if (ShouldMoveDown())
            {
                currentBlock.transform.position += move;
            }
            else
            {
                //if any of the blocks in the prefab hits the ceiling then game over
                for (int i = 0; i < currentBlock.transform.childCount; i++)
                {
                    float yLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.y;
                    float xLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.x;
                    int yIndex = CalculateYIndex(yLocation);
                    int xIndex = CalculateXIndex(xLocation);

                    if (yIndex <= 0)
                    {
                        gameOver = true;
                        break;
                    }

                    //tell the grid that these cells are occupied
                    blockSpace[yIndex, xIndex] = 1;
                    blockObjects[yIndex, xIndex] = currentBlock.transform.GetChild(i).gameObject;
                }

                clearedRows = new List<int>();
                CheckRows();
				
                if (clearedRows.Count > 0)
                {
                    ClearRows();
                    rowsCleared += clearedRows.Count;
                    level = rowsCleared / 10;
					
                    if (level > previousLevel)
                    {
                        previousLevel = level;
                        time -= 0.025f; //speed up when level is higher
                        Time.fixedDeltaTime = time;
                    }

                    if (clearedRows.Count == 1)
                    {
                        score += (40 * (level + 1));
                    }
                    if (clearedRows.Count == 2)
                    {
                        score += (100 * (level + 1));
                    }
                    if (clearedRows.Count == 3)
                    {
                        score += (300 * (level + 1));
                    }
                    if (clearedRows.Count == 4)
                    {
                        score += (1200 * (level + 1));
                    }

                    levelValue.text = level.ToString();
                    scoreValue.text = score.ToString();
                    linesClearedValue.text = rowsCleared.ToString();
                }

                if (gameOver)
                {
                    AudioSource audio = GetComponent<AudioSource>();
                    PlayerPrefs.SetInt("Score", score);
                    Destroy(nextBlock);
                    Instantiate(gameOverTitle, gameOverTitle.transform.position, gameOverTitle.transform.rotation);
                    audio.Stop();
                    StartCoroutine(GameOverTransition());
                }
                else
                {

                    currentBlock = (GameObject)Instantiate(nextBlock, new Vector2(CalculateXTransform(5), CalculateYTransform(0)), nextBlock.transform.rotation);
                    Destroy(nextBlock);
                    GameObject temp = allBlockPieces[randomGenerator.Next() % allBlockPieces.Length];
                    nextBlock = (GameObject)Instantiate(temp, new Vector2(0.6f, 2.0f), temp.transform.rotation);
                    preventSpeedUp = true;
                }
            }
        }
    }

    private void CheckRows() //check if the row is full
    {	
        for (int y = 0; y < blockSpace.GetLength(0); y++)
        {
            for (int x = 0; x < blockSpace.GetLength(1); x++)
            {
                if (blockSpace[y, x] == 0)  //if there is no block in the row then leave
                {
                    break;
                }
                else if (x == (blockSpace.GetLength(1) - 1))  //if the row is full add this row to the clearedRows list
                {
                    clearedRows.Add(y);
                }
            }
        }
    }

    private void ClearRows()
    {
        foreach (int row in clearedRows)
        {
            for (int x = 0; x < blockSpace.GetLength(1); x++)
            {
                blockSpace[row, x] = 0;

                Destroy(blockObjects[row, x]);
            }
			
            // move everything down
            for (int y = row; y > 0; y--)
            {
                for (int x = 0; x < blockSpace.GetLength(1); x++)
                {
                    blockSpace[y, x] = blockSpace[y - 1, x];
                    blockObjects[y, x] = blockObjects[y - 1, x];
                    if (blockObjects[y, x] != null)
                    {
                        blockObjects[y, x].transform.position += move;
                    }
                    blockSpace[y - 1, x] = 0;
                    blockObjects[y - 1, x] = null;
                }
            }
			
        }
    }

    private bool ShouldMoveRight()
    {
        bool doMove = true;

        //hits the wall?
        for (int i = 0; i < 4; i++)
        {
            float yLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.y;
            float xLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.x;
            int yIndex = CalculateYIndex(yLocation);
            int xIndex = CalculateXIndex(xLocation);
            if (xIndex + 1 > 9)
            {
                doMove = false;
                break;
            }

            //block to the right?
            if (blockSpace[yIndex, xIndex + 1] == 1)
            {
                doMove = false;
                break;
            }
        }
        return doMove;
    }

    private bool ShouldMoveLeft()
    {
        bool doMove = true;

        //hits the wall?
        for (int i = 0; i < 4; i++)
        {
            float yLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.y;
            float xLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.x;
            int yIndex = CalculateYIndex(yLocation);
            int xIndex = CalculateXIndex(xLocation);
            if (xIndex - 1 < 0)
            {
                doMove = false;
                break;
            }

            //block to the left?
            if (blockSpace[yIndex, xIndex - 1] == 1)
            {
                doMove = false;
                break;
            }
        }
        return doMove;
    }

    private bool ShouldMoveDown()
    {
        bool doMove = true;

        //hits the wall?
        for (int i = 0; i < 4; i++)
        {
            float yLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.y;
            float xLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.x;
            int yIndex = CalculateYIndex(yLocation);
            int xIndex = CalculateXIndex(xLocation);
            if (yIndex + 1 > 20)
            {
                doMove = false;
                break;
            }

            //block underneath?
            if (blockSpace[yIndex + 1, xIndex] == 1)
            {
                doMove = false;
                break;
            }
        }
        return doMove;
    }

    private int ShouldMoveDownReallyFast()
    {
        int limit = 20;

        //loops for every block in prefab, calculates its index
        for (int i = 0; i < 4; i++)
        {
            float yLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.y;
            float xLocation = currentBlock.transform.GetChild(i).gameObject.transform.position.x;
            int yIndex = CalculateYIndex(yLocation);
            int xIndex = CalculateXIndex(xLocation);

            int blocksLeft = 20 - yIndex; // how many empty space blicks are left from currentBlock.position

            if (blocksLeft < limit)
            {
                limit = blocksLeft;
            }

            //loops for every block left under the prefab
            for (int j = 1; j <= blocksLeft; j++)
            {
                //if hit the floor
                if (yIndex + j > 20)
                {
                    if ((j - 1) < limit && blockSpace[yIndex + j - 1, xIndex] != 1)
                    {
                        limit = j - 1;
                        break;
                    }
                }

                // if hit another block
                if (blockSpace[yIndex + j, xIndex] == 1)
                {
                    if ((j - 1) < limit)
                    { 
                        limit = j - 1;
                        break;
                    }
                }
            }

        }
        return limit; //return how many empty spaces our prefab can skip and land straight away
    }

    IEnumerator GameOverTransition()
    {
        yield return new WaitForSeconds(2f);
        Application.LoadLevel(0);
    }

    private int CalculateXIndex(float xTransform)
    {
        int returnIndex;
        float xFloat = (xTransform + 6.72f) / 0.64f;
        returnIndex = (int)System.Math.Round(xFloat, 0);
        return returnIndex;
    }

    private int CalculateYIndex(float yTransform)
    {
        int returnIndex;
        float yFloat = (6.4f - yTransform) / 0.64f;
        returnIndex = (int)System.Math.Round(yFloat, 0);
        return returnIndex;
    }

    private float CalculateXTransform(int x)
    {
        float returnTransform;
        float xFloat = (float)x;
        returnTransform = -6.72f + (xFloat * 0.64f);
        return returnTransform;
    }

    private float CalculateYTransform(int y)
    {
        float returnTransform;
        float yFloat = (float)y;
        returnTransform = 6.4f - (yFloat * 0.64f);
        return returnTransform;
    }
}
