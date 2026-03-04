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
        // 1. Progress Observation (1 float)
        sensor.AddObservation(board.GetStepCount() / 42f);

        // 2. Grid Observations (42 floats)
        for (int c = 0; c < 7; c++)
        {
            for (int r = 0; r < 6; r++)
            {
                int cell = board.GetCell(c, r);
                if (cell == 0) sensor.AddObservation(0f);
                else sensor.AddObservation(cell == playerID ? 1f : -1f);
            }
        }
        // Total = 43. Ensure Inspector matches this!
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int col = actions.DiscreteActions[0];

        if (col == -1) // Heuristic wait
        {
            RequestDecision();
            return;
        }

        if (col >= 0 && col < 7)
        {
            bool success = board.TryMove(col, playerID);
            
            if (!success)
            {
                AddReward(-0.01f); // Tiny penalty for invalid move
                RequestDecision();
            }
            else
            {
                // Efficiency reward: encourage winning in fewer moves
                AddReward(-0.002f); 
            }
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask collectMask)
    {
        for (int c = 0; c < 7; c++)
        {
            if (board.IsColumnFull(c)) collectMask.SetActionEnabled(0, c, false);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = board.GetAndClearHumanInput();
    }
}