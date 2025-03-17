using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMoveTo : AIJobBase
{
    List<PathNode> pathNodes;

    public void StartJob(AIController controller, AIMovement movement)
    {
        SetupPath(controller, movement, movement.Position, controller.info.job.jobPos);
    }

    public void StartJob(AIController controller, AIMovement movement, Vector3 startPosition)
    {
        SetupPath(controller, movement, startPosition, controller.info.job.jobPos);
    }

    public void StartJobWithPosition(AIController controller, AIMovement movement, Vector3 endPos)
    {
        SetupPath(controller, movement, movement.Position, endPos);
    }

    private void SetupPath(AIController controller, AIMovement movement, Vector3 start, Vector3 end)
    {
        pathNodes = null;
        PathFinding.Instance.FindPathAsync(start, end, (path) =>
        {
            pathNodes = path;
            
            if (pathNodes == null)
            {
                controller.EndJob(1);
                Debug.LogError("Unable to find path... ending job.");
                return;
            }

            movement.MoveToPosition(Utility.Instance.GetPositionFromTileCoords(pathNodes[0].pos, true));
        });
    }

    public bool WorkJob(AIController controller, AIMovement movement)
    {
        if (!movement.ActiveSeek)
        {
            if (pathNodes == null)
            {
                return true;
            }

            if (pathNodes.Count > 0)
            {
                int numToMove = 0;
                for (int i = 0; i < (pathNodes.Count >= 8 ? 8 : pathNodes.Count); i++)
                {
                    if ((pathNodes[i].pos - Utility.Instance.GetTileCoordsFromPosition(movement.Position)).magnitude <= 1.5f)
                    {
                        numToMove = i;
                    }
                }

                if (numToMove != 0)
                    pathNodes.RemoveRange(0, numToMove);
                else
                    pathNodes.RemoveAt(0);
            }

            if (pathNodes.Count == 0)
            {
                controller.EndJob();    
                return true;

            }
            else
            {
                movement.MoveToPosition(Utility.Instance.GetPositionFromTileCoords(pathNodes[0].pos, true));
            }
        }
        if (pathNodes != null)
        {
            DrawDebug();
        }
        return false;
    }

    private void DrawDebug()
    {
        for (int i = 0; i < pathNodes.Count; i++)
        {
            Vector3 offset = new Vector3(0, 0.2f, 0);
            Vector3 node1Pos = Utility.Instance.GetPositionFromTileCoords(pathNodes[i].pos, true);
            UtilityGizmo.Instance.DrawSphere(node1Pos + offset, 0.1f, (i == 0) ? Color.green : Color.white);
            if (i != pathNodes.Count - 1)
            {
                Vector3 node2Pos = Utility.Instance.GetPositionFromTileCoords(pathNodes[i + 1].pos, true);
                UtilityGizmo.Instance.DrawLine(node1Pos + offset, node2Pos + offset, (i == 0) ? Color.green : Color.white);
            }
        }
    }

    public void ForceFinishJob(AIController controller, AIMovement movement)
    {
        //do nothing rn
    }
}
