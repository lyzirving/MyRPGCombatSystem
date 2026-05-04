using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

//TODO: Support multiple objects
public class CanSeeObject : Conditional
{
    public SharedTransform target;

    private CharacterSensor m_Sensor = null;

    public override void OnAwake()
    {
        m_Sensor = GetComponent<CharacterSensor>();
    }

    public override TaskStatus OnUpdate()
    {
        if(target == null || target.Value == null) return TaskStatus.Failure;

        if (m_Sensor.CanSeeObject(target.Value)) return TaskStatus.Success;

        return TaskStatus.Running;
    }
}
