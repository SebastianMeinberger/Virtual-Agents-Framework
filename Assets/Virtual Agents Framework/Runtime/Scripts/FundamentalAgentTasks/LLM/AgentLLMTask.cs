using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.OpenAI;
using System.Reflection;
using System;
using System.Linq;
using i5.VirtualAgents.BehaviourTrees;
using Newtonsoft.Json.Linq;

namespace i5.VirtualAgents.AgentTasks
{
	public class AgentLLMTask : CompositeNode, ISerializable
	{
		public string message;
		public string answer;
		public string instructions;
		public string previous_response_id;
		public string own_id;
		public List<ILLMFunction> callbacks = new List<ILLMFunction>();
		public List<BehaviourTreeTask> executingChildren = new List<BehaviourTreeTask>();

		public override void StartExecution(Agent executingAgent)
		{
			this.executingAgent = executingAgent;
			Request request = previous_response_id != null && previous_response_id != "" ?
				new StatefullRequest(previous_response_id) : new Request();
			TextInput input = new TextInput();

			TextContent text = new TextContent();
			text.text = message;
			input.content = new Content[] { text };//, image };


			request.input = request.input.Append(input).ToArray();
			//request.instructions = instructions;
			request.instructions = @"Your purpose is to explain how to use OpenProject. When the user first greets you, you start by explaining your purpose and how to create a new project. When the user confirms that they were successful, you explain how to create the first work packages. Next, you explain how to set the status of workpackges and how to assing them to a person.
Things you should do:
- Only use the filesearch for information about OpenProject.
- When you talk about a paragraph from a file and that paragraph contains the name of an image, use the point_at_picture function via a tool call to point out relevant points on the picture.
- Use toolcalls without asking for confirmation first.
- Use toolcalls via the API
Things you should not do:
- Never make up names of pictures, use only the image names found through file search.
- Never reference images directly, always use the point_at_picture function via a toolcall.
- Never include toolcalls in the text output, only use the API to call tools.";
			var fileSearch = new FileSearch(new string[] { "vs_6835d94e62fc8191a99b8f84e9dd7d1d" });

			request.tools = new Tool[] { fileSearch };
			request.tools = request.tools.Concat(CreateFunctionCalls()).ToArray();
			Debug.Log(request.ToJson());
			executingAgent.StartCoroutine(WaitForRequest(request));
		}

		public List<Tool> CreateFunctionCalls()
		{
			var tools = new List<Tool>();
			foreach (ITask child in Children)
			{
				if (child is BehaviourTreeTask treeTask)
				{
					FunctionCall functionCall = new()
					{
						description = treeTask.tree.description,
						name = treeTask.tree.name
					};
					foreach (SerializationEntry<SerializationDataContainer> node in treeTask.overwriteData.data)
					{
						SerializationDataContainer serializer = node.Value;

						//SerializationDataContainer serializer = treeTask.overwriteData;
						int counter = 0;
						int wrapper(SerializableType type, int index)
						{
							if (serializer.exposeToLLM[counter]) {
								switch (type)
								{
									case SerializableType.INT:
										var paramInt = serializer.serializedInts.data[index];
										functionCall.parameters.properties.Add(new PropertieTemplate(paramInt.Key, "number", paramInt.Description));
										break;
									case SerializableType.FLOAT:
										var paramFloat = serializer.serializedFloats.data[index];
										functionCall.parameters.properties.Add(new PropertieTemplate(paramFloat.Key, "number", paramFloat.Description));
										break;
									case SerializableType.STRING:
										var paramString = serializer.serializedStrings.data[index];
										functionCall.parameters.properties.Add(new PropertieTemplate(paramString.Key, "string", paramString.Description));
										break;
								}
							}
							counter++;
							return 0;
						}
						serializer.MapOverData(wrapper);
					}
					tools.Add(functionCall);
					callbacks.Add(new FunctionCallCallback(treeTask.overwriteData, treeTask, executingChildren, treeTask.tree.name));
				}
			}
			return tools;
		}

		public override TaskState EvaluateTaskState()
		{
			foreach (BehaviourTreeTask child in executingChildren)
			{
				child.Tick(executingAgent);
			}
			return TaskState.Running;
        }

		IEnumerator WaitForRequest(Request request)
		{
			//yield return RequestHandler.Upload(request,new ILLMFunction[]{ new point_at_picture() });
			yield return RequestHandler.Upload(request,callbacks.ToArray());
			answer = request.answer;
			own_id = request.fullAnswer.id;
			Debug.Log(answer);
			StopAsSucceeded();
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

	public class FunctionCallCallback : ILLMFunction
	{
		NodesOverwriteData overwriteData;
		BehaviourTreeTask task;
		List<BehaviourTreeTask> executingTasks;

		public FunctionCallCallback(NodesOverwriteData overwriteData, BehaviourTreeTask task, List<BehaviourTreeTask> executingTasks, string name)
		{
			this.overwriteData = overwriteData;
			this.task = task;
			this.executingTasks = executingTasks;
			functionName = name;
		}


		public override void Populate(string json)
		{
			JObject jsonToken = JObject.Parse(json);
			foreach (SerializationEntry<SerializationDataContainer> node in overwriteData.data)
			{ 
				int wrapper(SerializableType type, int index)
				{
					//string keyWithGUID = serializer.GetKeyByIndex(index, type);
					//string[] keySplitted = keyWithGUID.Split(".");
					SerializationDataContainer serializer = node.Value;
					if (true)//keySplitted.Length > 1)
					{
						//string key = keySplitted.Last();
						string key = serializer.GetKeyByIndex(index, type);
						switch (type)
						{
							case SerializableType.INT:
								{
									int? value = jsonToken[key]?.Value<int>();
									if (value != null)
									{
										serializer.SetSerializedData(key, (int)value);
									}
									break;
								}
							case SerializableType.FLOAT:
								{
									float? value = jsonToken[key]?.Value<float>();
									if (value != null)
									{
										serializer.SetSerializedData(key, (float)value);
									}
									break;
								}
							case SerializableType.STRING:
								{
									string value = jsonToken[key]?.Value<string>();
									if (value != null)
									{
										serializer.SetSerializedData(key, value);
									}
									break;
								}
						}
					}
					return 0;
				}
				node.Value.MapOverData(wrapper);
			}
		}

        public override string Work()
        {
			executingTasks.Add(task);
			return "sucess";
        }
		
	}
}
