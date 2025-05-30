using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.VirtualAgents.Examples;

namespace i5.VirtualAgents.AgentTasks
{
    public class Whiteboard : AgentBaseTask, ISerializable
    {
        public string spriteName;

        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            var whiteboardManager = Object.FindObjectOfType<WhiteboardManager>();
            whiteboardManager.ChangeSprite(spriteName);
            StopAsSucceeded();
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Sprite Name",spriteName);
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
            spriteName = serializer.GetSerializedString("Sprite Name");
        }
    }
}
