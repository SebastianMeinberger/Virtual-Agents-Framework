using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.OpenAI;

namespace i5.VirtualAgents.AgentTasks
{
    public class AgentLLMTask : AgentBaseTask, ISerializable
    {
        public string message;
		public string answer;
		public string instructions;
		public string previous_response_id ;
		public string own_id;
		
        public override void StartExecution(Agent executingAgent)
		{
			TextRequest request = previous_response_id != null && previous_response_id != "" ?
				new StatefullTextRequest(message, previous_response_id) : new TextRequest(message);
			request.instructions = "You are an assististant that instructs students how to use the plannig software Open Project. The first step in working with Open Project is to create a new project with the create project button. Next a project name needs to be entered in the enter name field. \n\n You are capabel of pointing out locations on a whiteboard. For that, you will be handed a semicolon seperated list of tuples, of wich the first element is the name of the subject and the second element is a short description. To point out a location, insert the name from the tuple sourounded by <> into your answer after you mentioned the respective subject. Do this always when you talk about a subject that fits the description of a tuple. Now the tuple list surrounded by paranthesis follows: (create_project,The button that creates a new project;enter_name,The input field where the name of the project is entered).\nAn example of pointing out a location on a whiteboard is: user: How do I create a new project? assistant: Press the create project button<create_project>.";
			executingAgent.StartCoroutine(WaitForRequest(request));
		}

		IEnumerator WaitForRequest(TextRequest request)
		{
			yield return RequestHandler.Upload(request);
			answer = request.answer;
			own_id = request.fullAnswer.id;
			FinishTask();
		}

		public void Serialize(SerializationDataContainer serializer)
		{
			serializer.AddSerializedData("Message", message);
			serializer.AddSerializedData("Answer", answer);
			serializer.AddSerializedData("Instructions", instructions);
			serializer.AddSerializedData("Previous ID", previous_response_id);
			serializer.AddSerializedData("Own ID", own_id);
		}

		public void Deserialize(SerializationDataContainer serializer)
		{
			message = serializer.GetSerializedString("Message");
			instructions = serializer.GetSerializedString("Instructions");
			previous_response_id = serializer.GetSerializedString("Previous ID");
		}
    }
}
