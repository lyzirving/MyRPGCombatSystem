using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class WithinDistance : Conditional
{
    public DistanceZoneSettings distanceZoneSetting;
    public EDistanceZone selectedZone = EDistanceZone.None;
    public float distanceThreshold = 2f;

    public SharedTransform target;

    public override TaskStatus OnUpdate()
    {
        if (target == null || target.Value == null)
        {
            Debug.LogError("InRange: target hasn't been asigned yet!");
            return TaskStatus.Failure;
        }
        float distance = Vector3.Distance(transform.position, target.Value.position);

        if (distanceZoneSetting != null && selectedZone > EDistanceZone.None && selectedZone < EDistanceZone.Num)
        { 
            return distanceZoneSetting.WithinRange(selectedZone, distance) ? TaskStatus.Success : TaskStatus.Failure;
        }

        return distance < distanceThreshold ? TaskStatus.Success : TaskStatus.Failure;  
    }    
}
