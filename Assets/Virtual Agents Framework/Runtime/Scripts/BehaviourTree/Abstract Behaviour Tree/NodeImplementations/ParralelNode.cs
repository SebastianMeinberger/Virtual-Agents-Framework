using System.Collections;
using System.Collections.Generic;
using i5.VirtualAgents.AgentTasks;
using UnityEngine;

namespace i5.VirtualAgents.BehaviourTrees
{
    /// <summary>
    /// Executes all its children in parralel. Aborts if one child fails and finishes when all children finish
    /// </summary>
    public class ParralelNode : CompositeNode, ISerializable
    {
        private List<ITask> unfinishedChildren;

        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            unfinishedChildren = Children;
        }

        public override TaskState EvaluateTaskState()
        {
            List<ITask> unfinishedChildren = new List<ITask>();
            foreach(ITask child in this.unfinishedChildren)
            {
                TaskState childState = child.Tick(executingAgent);
                if(childState == TaskState.Failure)
                {
                    // If one child fails, fail too
                    return TaskState.Failure;
                }
                else if(childState == TaskState.Running)
                {
                    unfinishedChildren.Add(child);
                }
            }
            if(unfinishedChildren.Count == 0)
            {
                // If all children succeeded, succeed too
                return TaskState.Success;
            }
            else
            {
                this.unfinishedChildren = unfinishedChildren;
                return TaskState.Running;
            }
        }

        public void Serialize(SerializationDataContainer serializer)
        { }

        public void Deserialize(SerializationDataContainer serializer)
        { }
    }
}
