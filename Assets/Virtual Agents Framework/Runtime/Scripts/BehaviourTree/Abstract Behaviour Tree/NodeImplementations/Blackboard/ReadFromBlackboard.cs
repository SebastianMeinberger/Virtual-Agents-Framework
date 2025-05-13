using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.BehaviourTrees;

namespace i5.VirtualAgents
{
    public class ReadFromBlackboard : BlackboardAcesser
    {
        /// <summary>
        /// Read the value of readSource from the black board and write it into readTarget in the child
        /// </summary>
        /// <param name="executingAgent"></param>
        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            if (target != "" && source != "")
            {
                SerializationDataContainer ancestorData = getAncestorData();
                SerializationDataContainer blackboardData = executingAgent.GetComponent<BehaviourTreeRunner>().blackBoard;
                moveBetweenDataContainers(blackboardData, ancestorData, source, target);
                writeAncestorData(ancestorData);
            }
        }
    }
}
