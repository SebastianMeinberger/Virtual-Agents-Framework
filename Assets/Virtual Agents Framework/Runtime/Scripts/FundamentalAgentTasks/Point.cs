using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.VirtualAgents.AgentTasks
{
    public class Point : AgentAnimationTask
    {
        public Vector3 position;
        public bool pointRight = true;


        public override void StartExecution(Agent agent)
        {
            if (pointRight)
            {
                startTrigger = "PointingRight";
                layer = "Right Arm";
            }
            else
            {
                startTrigger = "PointingLeft";
                layer = "Left Arm";
            }
            playTime = 20;

            if (aimTarget == null)
            {
                aimTarget = new GameObject();
                aimTarget.transform.position = position;
            }

            base.StartExecution(agent);
        }

        public override void Deserialize(SerializationDataContainer serializer)
        {
            position = serializer.GetSerializedVector("Position");
            aimTarget = serializer.GetSerializedGameobjects("Target");
        }

        public override void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Position", position);
            serializer.AddSerializedData("Target", aimTarget);
        }
    }
}
