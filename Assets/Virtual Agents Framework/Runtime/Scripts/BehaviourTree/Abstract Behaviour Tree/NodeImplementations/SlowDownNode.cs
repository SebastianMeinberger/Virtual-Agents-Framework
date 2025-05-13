using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.VirtualAgents.AgentTasks;

namespace i5.VirtualAgents.BehaviourTrees
{
    public class SlowDownNode : DecoratorNode, ISerializable
    {
        public float waitTime = 0;
		private float lastUpdate = 0;
		private TaskState lastState = TaskState.Waiting;

        public override TaskState EvaluateTaskState()
        {
			if(lastUpdate == 0 | lastUpdate + waitTime <= Time.realtimeSinceStartup)
			{
				lastUpdate = Time.realtimeSinceStartup;
				lastState = Child.Tick(executingAgent);
			}
			return lastState;
        } 

		public void Deserialize(SerializationDataContainer serializer)
		{
			waitTime = serializer.GetSerializedFloat("Wait time");
		}

		public void Serialize(SerializationDataContainer serializer)
		{
			serializer.AddSerializedData("Wait time", waitTime);
		}
    }
}
