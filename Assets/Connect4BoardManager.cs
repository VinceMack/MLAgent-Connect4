using UnityEngine;
using Unity.MLAgents;
using System.Collections;

public class Connect4BoardManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject redPrefab;
    public GameObject yellowPrefab;
    public Transform pieceContainer;

    [Header("Agents")]
    public Connect4Agent agentRed;
    public Connect4Agent agentYellow;

    private int[,] grid = new int[7, 6]; 
    private int currentTurn = 1; 
    private bool isGameOver = false;
    private int humanInputBuffer = -1;
    private int totalStepsTaken = 0;

    void Start() { ResetGame(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) humanInputBuffer = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) humanInputBuffer = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) humanInputBuffer = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) humanInputBuffer = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) humanInputBuffer = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) humanInputBuffer = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) humanInputBuffer = 6;
    }

    public int GetAndClearHumanInput()
    {
        int val = humanInputBuffer;
        humanInputBuffer = -1;
        return val;
    }

    public void ResetGame()
    {
        grid = new int[7, 6];
        isGameOver = false;
        humanInputBuffer = -1;
        totalStepsTaken = 0;

        if (pieceContainer != null)
            foreach (Transform child in pieceContainer) Destroy(child.gameObject);
        
        currentTurn = Random.Range(1, 3); 
        RequestMove();
    }

    public bool IsMyTurn(int playerID) => currentTurn == playerID;
    public int GetStepCount() => totalStepsTaken;

    public bool TryMove(int col, int playerID)
    {
        if (isGameOver || playerID != currentTurn || col < 0 || col >= 7 || IsColumnFull(col)) return false;

        for (int r = 0; r < 6; r++)
        {
            if (grid[col, r] == 0)
            {
                grid[col, r] = playerID;
                totalStepsTaken++;

                GameObject prefab = (playerID == 1) ? redPrefab : yellowPrefab;
                GameObject piece = Instantiate(prefab, pieceContainer);
                piece.transform.localPosition = new Vector3(col, r, 0);
                
                if (CheckWin(playerID)) EndMatch(playerID);
                else if (IsDraw()) EndMatch(0);
                else 
                {
                    currentTurn = (currentTurn == 1) ? 2 : 1;
                    RequestMove();
                }
                return true;
            }
        }
        return false;
    }

    private void RequestMove()
    {
        if (isGameOver) return;
        if (currentTurn == 1) agentRed.RequestDecision();
        else agentYellow.RequestDecision();
    }

    public int GetCell(int c, int r) => grid[c, r];
    public bool IsColumnFull(int c) => grid[c, 5] != 0;
    private bool IsDraw() { for (int c = 0; c < 7; c++) if (!IsColumnFull(c)) return false; return true; }

    private void EndMatch(int winner)
    {
        isGameOver = true;

        if (winner == 0) {
            Debug.Log("<color=white>[MATCH] Draw.</color>");
            agentRed.AddReward(0.1f); 
            agentYellow.AddReward(0.1f); 
        }
        else {
            Debug.Log(winner == 1 ? "<color=red>[MATCH] Red Wins!</color>" : "<color=yellow>[MATCH] Yellow Wins!</color>");
            agentRed.SetReward(winner == 1 ? 1f : -1f);
            agentYellow.SetReward(winner == 2 ? 1f : -1f);
        }

        agentRed.EndEpisode();
        agentYellow.EndEpisode();
        ResetGame();
    }

    private bool CheckWin(int p)
    {
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 4; c++)
                if (grid[c,r]==p && grid[c+1,r]==p && grid[c+2,r]==p && grid[c+3,r]==p) return true;
        for (int c = 0; c < 7; c++)
            for (int r = 0; r < 3; r++)
                if (grid[c,r]==p && grid[c,r+1]==p && grid[c,r+2]==p && grid[c,r+3]==p) return true;
        for (int c = 0; c < 4; c++)
            for (int r = 0; r < 3; r++)
                if (grid[c,r]==p && grid[c+1,r+1]==p && grid[c+2,r+2]==p && grid[c+3,r+3]==p) return true;
        for (int c = 0; c < 4; c++)
            for (int r = 3; r < 6; r++)
                if (grid[c,r]==p && grid[c+1,r-1]==p && grid[c+2,r-2]==p && grid[c+3,r-3]==p) return true;
        return false;
    }
}