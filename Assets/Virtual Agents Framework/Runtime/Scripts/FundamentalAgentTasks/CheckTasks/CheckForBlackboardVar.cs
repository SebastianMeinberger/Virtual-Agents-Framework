using System.Collections;
using System.Collections.Generic;
using i5.VirtualAgents.BehaviourTrees;
using UnityEngine;

namespace i5.VirtualAgents.AgentTasks
{
    public class CheckForBlackboardVar : CheckBaseTask, ISerializable
    {
        public string target;
        public override void StartExecution(Agent executingAgent)
        {
            
            base.StartExecution(executingAgent);
        }
        protected override TaskState Check()
        {
            SerializationDataContainer blackboardData = executingAgent.GetComponent<BehaviourTreeRunner>().blackBoard;
            bool foundAndValid = false;
            int wrapper(SerializableType type, int index)
            {
                string key = blackboardData.GetKeyByIndex(index,type);
                if(key == target)
                    foundAndValid = type switch
                    {
                        SerializableType.STRING => blackboardData.GetSerializedString(key) != null,
                        SerializableType.GAMEOBJECT => blackboardData.GetSerializedGameobjects(key) != null,
                        SerializableType.BOOL => blackboardData.GetSerializedBool(key),
                        _ => true,
                    };
                return 0;
            }
            blackboardData.MapOverData(wrapper);

            if(foundAndValid)
            {
                return TaskState.Success;
            }
            else
            {
                return TaskState.Failure;
            }
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
            target = serializer.GetSerializedString("Target");
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Target",target);
        }
    }
}
