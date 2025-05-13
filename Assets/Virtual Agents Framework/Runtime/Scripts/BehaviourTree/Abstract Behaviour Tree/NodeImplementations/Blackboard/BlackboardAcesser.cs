using i5.VirtualAgents.BehaviourTrees;
using i5.VirtualAgents.AgentTasks;
using UnityEngine;

namespace i5.VirtualAgents
{
    public abstract class BlackboardAcesser : DecoratorNode, ISerializable
    {
        public string target;
        public string source;
        private ITask nonBlackboardAncestor;

        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            nonBlackboardAncestor = findAncestor(Child);
        }

        private ITask findAncestor(ITask child)
        {
            if(child is BlackboardAcesser)
            {
                return findAncestor((child as DecoratorNode).Child);
            }
            return child;
        }

        protected SerializationDataContainer getAncestorData()
        {
            SerializationDataContainer serializer = new SerializationDataContainer();
            (nonBlackboardAncestor as ISerializable).Serialize(serializer);
            return serializer;
        }

        protected void writeAncestorData(SerializationDataContainer data)
        {
            (nonBlackboardAncestor as ISerializable).Deserialize(data);
        }

        protected void moveBetweenDataContainers(SerializationDataContainer sourceData, SerializationDataContainer targetData, string sourceKey, string targetKey)
        {
            // Find the type of the value
            int targetIndex = sourceData.GetKeysInSerializationOrder().FindIndex(o1 => o1 == sourceKey);
            SerializableType type = sourceData.serializationOrder[targetIndex];

            // Set or add the value
            switch (type)
            {
                case SerializableType.STRING:
                {
                    string value = sourceData.GetSerializedString(sourceKey);
                    if(!targetData.AddSerializedData(targetKey,value))
                    {
                        targetData.serializedStrings.SetValue(targetKey,value);
                    }
                    break;
                }

                case SerializableType.GAMEOBJECT:
                {
                    GameObject value = sourceData.GetSerializedGameobjects(sourceKey);
                    if(!targetData.AddSerializedData(targetKey,value))
                    {
                        targetData.serializedGameobjects.SetValue(targetKey,value);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Just pass the updates down to the child
        /// </summary>
        /// <returns></returns>
        public override TaskState EvaluateTaskState()
        {
            return Child.Tick(executingAgent);
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Source", source);
            serializer.AddSerializedData("Target", target);
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
            source = serializer.GetSerializedString("Source");
            target = serializer.GetSerializedString("Target");
        }
    }
}
