using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class WithinDistance : Conditional
{
    public float distanceThreshold = 2f;
    public SharedTransform target;

    public override TaskStatus OnUpdate()
    {
        if (target == null || target.Value == null)
        {
            Debug.LogError("InRange: target hasn't been asigned yet!");
            return TaskStatus.Failure;
        }
        return Vector3.Distance(transform.position, target.Value.position) < distanceThreshold ? TaskStatus.Success : TaskStatus.Failure;  
    }    
}
