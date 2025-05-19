using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.VirtualAgents.AgentTasks
{
    public class ParseLLMOutput : AgentBaseTask, ISerializable
    {
        public string input;

        public string textStack;
        public GameObject pointTargetStack;

        public string pointTargetName;

        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            pointTargetStack = null;
            textStack = "";
            pointTargetName = "";
            bool pointTarget = false;
            int length = input.Length;

            if(length == 0)
            {
                StopAsFailed();
                return;
            }

            for (int i = 0; i < length;i++)
            {
                char c = input[0];
                input = input[1..];
                if(c == '<')
                {
                    pointTarget = true;
                    continue;
                }
                else if (c == '>')
                {
                    pointTarget = false;
                    continue;
                }
                else if (c == '.' && !pointTarget)
                {
                    textStack += c;
                    break;
                }

                if(pointTarget)
                {
                    pointTargetName += c;
                }
                else
                {
                    textStack += c;
                }
            }

            pointTargetStack = GameObject.Find(pointTargetName);

            StopAsSucceeded();
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Input", input);
            serializer.AddSerializedData("Text Stack", textStack);
            serializer.AddSerializedData("Point Target Stack", pointTargetStack);
            serializer.AddSerializedData("Point Target Name Stack", pointTargetName);
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
            input = serializer.GetSerializedString("Input");
        }
    }
}
