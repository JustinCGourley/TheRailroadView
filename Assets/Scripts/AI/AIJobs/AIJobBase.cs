using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AIJobBase
{
    /// <summary>
    /// Abstract method to 'work job' for any AI created job task | will return false upon completing job, and true while job is being worked
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="movement"></param>
    /// <returns></returns>
    bool WorkJob(AIController controller, AIMovement movement);
    void StartJob(AIController controller, AIMovement movement);

    void ForceFinishJob(AIController controller, AIMovement movement);
}
