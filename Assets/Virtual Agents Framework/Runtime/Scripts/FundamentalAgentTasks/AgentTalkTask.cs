using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace i5.VirtualAgents.AgentTasks
{
	/// <summary>
	/// Spawns a billboard speech bubble and prints the specified text.
	/// If a blackbox address is supplied, text is taken from blackbox
	/// </summary>/
    public class AgentTalkTask : AgentBaseTask, ISerializable
    {
		public string text;
		public bool letterByLetter;
		
		private int letterIndex = 0;
		private TextMeshProUGUI textMesh;

		public AgentTalkTask(){}

        public override void StartExecution(Agent executingAgent)
        {
			letterIndex = 0;
			textMesh = Object.FindObjectOfType<TextMeshProUGUI>();
			if(!letterByLetter)
			{
				textMesh.text = text;
				StopAsSucceeded();
			}
			base.StartExecution(executingAgent);
        }

        public override TaskState EvaluateTaskState()
        {
            textMesh.text += text[letterIndex];
			letterIndex++;
			if(letterIndex >= text.Length)
			{
				return TaskState.Success;
			}
			else
			{
				return TaskState.Running;
			}
        }

        

		public void Serialize(SerializationDataContainer serializer)
		{
			serializer.AddSerializedData("Text",text);
			serializer.AddSerializedData("Letter by letter",letterByLetter);
		}

		public void Deserialize(SerializationDataContainer serializer)
		{
			text = serializer.GetSerializedString("Text");
			letterByLetter = serializer.GetSerializedBool("Letter by letter");
		}
    }
}
