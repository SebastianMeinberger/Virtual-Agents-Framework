using Codice.Client.Common.Connection.ServerAlias;
using Codice.Client.Common.TreeGrouper;
using i5.VirtualAgents.BehaviourTrees;
using i5.VirtualAgents.BehaviourTrees.Visual;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.VirtualAgents.AgentTasks
{
    /// <summary>
    /// Executes a given Behaviour Tree
    /// </summary>
    public class BehaviourTreeTask : AgentBaseTask, ISerializable
    {
        public BehaviourTreeAsset tree;
        private ITask root;
        public NodesOverwriteData overwriteData = new();
        

        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            root = tree.GetExecutableTree(executingAgent, overwriteData);
        }

        public override TaskState EvaluateTaskState()
        {
            return root.Tick(executingAgent);
        }

        private void addToContainer(SerializationDataContainer to, SerializationDataContainer from, SerializableType type, string readKey, string writeKey)
        {
            string description;
            switch (type)
            {
                case SerializableType.INT:
                    description = from.serializedInts.GetDescription(readKey);
                    if (to.AddSerializedData(writeKey, from.GetSerializedInt(readKey), description))
                    {
                        to.SetLLMExposure(writeKey, from.GetLLMExposure(readKey));
                    }
                    break;
                case SerializableType.VECTOR3:
                    description = from.serializedVectors.GetDescription(readKey);
                    if (to.AddSerializedData(writeKey, from.GetSerializedVector(readKey), description))
                    {
                        to.SetLLMExposure(writeKey, from.GetLLMExposure(readKey));
                    }
                    break;
                case SerializableType.GAMEOBJECT:
                    description = from.serializedGameobjects.GetDescription(readKey);
                    if (to.AddSerializedData(writeKey, from.GetSerializedGameobjects(readKey), description))
                    {
                        to.SetLLMExposure(writeKey, from.GetLLMExposure(readKey));
                    }
                    break;
                case SerializableType.STRING:
                    description = from.serializedStrings.GetDescription(readKey);
                    if (to.AddSerializedData(writeKey, from.GetSerializedString(readKey), description))
                    {
                        to.SetLLMExposure(writeKey, from.GetLLMExposure(readKey));
                    }
                    break;
            }
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
            tree = serializer.GetSerializedTrees("Tree");

            if (tree == null)
            {
                return;
            }

            foreach (VisualNode node in tree.Nodes)
            {
                SerializationDataContainer nodeData = new();
                string guid = node.Guid;
                //node.GetCopyOfSerializedInterface().Serialize(nodeData);

                int copy(SerializableType type, int index)
                {
                    string guidKey = "";
                    guidKey = serializer.GetKeyByIndex(index, type);
                    string[] guidKeyArr = guidKey.Split(".");
                    if (guidKeyArr.Length > 1)
                    {
                        string nodeGuid = guidKeyArr[0];
                        string key = guidKeyArr[1];
                        if (guid == nodeGuid)
                        {
                            addToContainer(nodeData, serializer, type, guidKey, key);
                        }
                    }
                    return 0;
                }
                serializer.MapOverData(copy);

                overwriteData.Add(guid, nodeData);
            }
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Tree", tree);
            if (tree == null)
            {
                return;
            }

            foreach (VisualNode node in tree.Nodes)
            {
                string guid = node.Guid;
                int copy(SerializableType type, int index)
                {
                    string key = node.Data.GetKeyByIndex(index, type);
                    addToContainer(serializer, node.Data, type, key, guid + "." + key);
                    return 0;
                }
                node.Data.MapOverData(copy);
            }
        }
    }
}
