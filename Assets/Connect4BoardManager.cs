using UnityEngine;
using Unity.MLAgents;
using System.Collections;
using Unity.MLAgents.Policies;

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

    void Start() { ResetGame(); }

    void Update()
    {
        // 1. Centralized Input Polling
        if (Input.GetKeyDown(KeyCode.Alpha1)) humanInputBuffer = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) humanInputBuffer = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) humanInputBuffer = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) humanInputBuffer = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) humanInputBuffer = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) humanInputBuffer = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) humanInputBuffer = 6;

        // 2. Logic Trigger: If we have a human input, tell the active agent to process it
        if (humanInputBuffer != -1)
        {
            Connect4Agent activeAgent = (currentTurn == 1) ? agentRed : agentYellow;
            var bp = activeAgent.GetComponent<BehaviorParameters>();

            // If the active agent is human-controlled, request the decision now
            if (bp.BehaviorType == BehaviorType.HeuristicOnly)
            {
                Debug.Log($"[Board] Human input detected for Column {humanInputBuffer + 1}. Requesting Decision...");
                activeAgent.RequestDecision();
            }
        }
    }

    // Called by Agent's Heuristic to get the keypress
    public int GetAndClearHumanInput()
    {
        int val = humanInputBuffer;
        humanInputBuffer = -1; // Wipe buffer so it's only used once
        return val;
    }

    public void ResetGame()
    {
        Debug.Log("[Board] Resetting Game...");
        grid = new int[7, 6];
        isGameOver = false;
        humanInputBuffer = -1;

        if (pieceContainer != null)
            foreach (Transform child in pieceContainer) Destroy(child.gameObject);
        
        currentTurn = Random.Range(1, 3); 
        Debug.Log($"[Board] Game Started. Player {currentTurn} goes first.");
        
        StartCoroutine(InitialRequest());
    }

    IEnumerator InitialRequest()
    {
        yield return new WaitForSeconds(0.5f);
        RequestMove();
    }

    private void RequestMove()
    {
        if (isGameOver) return;

        Connect4Agent activeAgent = (currentTurn == 1) ? agentRed : agentYellow;
        var bp = activeAgent.GetComponent<BehaviorParameters>();

        // If AI is playing, request decision immediately.
        // If Human is playing, the Update() loop above will handle the request when a key is pressed.
        if (bp.BehaviorType != BehaviorType.HeuristicOnly)
        {
            Debug.Log($"[Board] AI Turn (Player {currentTurn}). Requesting Decision...");
            activeAgent.RequestDecision();
        }
        else
        {
            Debug.Log($"[Board] Waiting for Human Input (Player {currentTurn}). Press 1-7...");
        }
    }

    public bool TryMove(int col, int playerID)
    {
        if (isGameOver || playerID != currentTurn) return false;
        if (col < 0 || col >= 7 || IsColumnFull(col))
        {
            Debug.LogWarning($"[Board] Player {playerID} attempted invalid move in Col {col}");
            return false;
        }

        for (int r = 0; r < 6; r++)
        {
            if (grid[col, r] == 0)
            {
                grid[col, r] = playerID;
                GameObject piece = Instantiate(playerID == 1 ? redPrefab : yellowPrefab, pieceContainer);
                piece.transform.localPosition = new Vector3(col, r, 0);
                
                Debug.Log($"[Board] Player {playerID} placed piece in Col {col}, Row {r}");

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

    // Helper functions for the Agents
    public int GetCell(int c, int r) => grid[c, r];
    public bool IsColumnFull(int c) => grid[c, 5] != 0;

    private bool IsDraw() {
        for (int c = 0; c < 7; c++) if (!IsColumnFull(c)) return false;
        return true;
    }

    private void EndMatch(int winner)
    {
        isGameOver = true;
        Debug.Log($"[Board] Match Over. Winner: {winner} (0=Draw)");

        if (winner == 0) { agentRed.AddReward(0.1f); agentYellow.AddReward(0.1f); }
        else {
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