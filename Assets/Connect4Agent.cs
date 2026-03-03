using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Connect4Agent : Agent
{
    public Connect4BoardManager board;
    public int playerID; 

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int c = 0; c < 7; c++)
            for (int r = 0; r < 6; r++)
            {
                int cell = board.GetCell(c, r);
                if (cell == 0) sensor.AddObservation(0f);
                else sensor.AddObservation(cell == playerID ? 1f : -1f);
            }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int col = actions.DiscreteActions[0];

        // If Heuristic returned -1 (no key pressed), do nothing.
        // The BoardManager will handle re-requesting once a key is hit.
        if (col >= 0 && col < 7)
        {
            Debug.Log($"[Agent {playerID}] Executing Move in Col {col}");
            board.TryMove(col, playerID);
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask collectMask)
    {
        // Mask full columns
        for (int c = 0; c < 7; c++)
        {
            if (board.IsColumnFull(c)) collectMask.SetActionEnabled(0, c, false);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        // Fetch the keypress stored in the BoardManager
        int input = board.GetAndClearHumanInput();
        discreteActions[0] = input;
    }
}