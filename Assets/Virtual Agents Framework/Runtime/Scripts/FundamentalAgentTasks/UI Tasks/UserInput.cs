
using UnityEngine;
using TMPro;

namespace i5.VirtualAgents.AgentTasks
{
    public class UserInput : AgentBaseTask, ISerializable
    {
        public string input = null;
        private GameObject inputFieldObject;
        private TMP_InputField inputField;
        public override void StartExecution(Agent executingAgent)
        {
            base.StartExecution(executingAgent);
            inputFieldObject = Object.FindObjectOfType<UIManager>().userInput;
            inputFieldObject.SetActive(true);
            inputField = inputFieldObject.GetComponent<TMP_InputField>();
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.Select();
        }

        public override void StopExecution()
        {
            base.StopExecution();
            inputField.text = "";
            inputFieldObject.SetActive(false);
        }

        public override TaskState EvaluateTaskState()
        {
            if(input == null)
            {
                return TaskState.Running;
            }
            else
            {
                return TaskState.Success;
            }
        }

        private void OnSubmit(string text)
        {
            input = text;
        }

        public void Deserialize(SerializationDataContainer serializer)
        {
        }

        public void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Input",input);
        }
    }
}
