using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.BehaviourTrees;

namespace i5.VirtualAgents
{
    public class WriteBlackboardData : BlackboardAcesser
    {
 public override void StopExecution()
        {
            base.StopExecution();
            if (target != "" && source != "")
            {
                SerializationDataContainer blackboardData = executingAgent.GetComponent<BehaviourTreeRunner>().blackBoard;
                SerializationDataContainer ancestorData = getAncestorData();
                moveBetweenDataContainers(ancestorData,blackboardData,source,target);
            }
        }
    }
}
