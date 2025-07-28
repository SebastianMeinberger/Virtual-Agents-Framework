using System.Collections.Generic;
using UnityEditor;
using i5.VirtualAgents.BehaviourTrees.Visual;
using i5.VirtualAgents.AgentTasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace i5.VirtualAgents.Editor.BehaviourTrees
{
    /// <summary>
    /// Exposes the data that was serialized via the ISerializable interface in the original serialization order
    /// </summary>
    [CustomEditor(typeof(VisualNode))]
    public class VisualNodeInspector : UnityEditor.Editor
    {
        VisualElement inspector;
        List<PropertyField> fields = new();

        private void OnTreeChange(BehaviourTreeAsset tree, VisualElement element, VisualNode targetNode, SerializedObject serializedObject)
        {
            // TODO Clear old data
            
            targetNode.GetCopyOfSerializedInterface().Serialize(targetNode.Data);
            /*
            foreach (string key in targetNode.Data.GetKeysInSerializationOrder())
            {
                targetNode.exposeToLLM.Add(key, false);
            }
            */
            serializedObject.Update();
            
        }


        public void OnInspectorGUI(VisualElement element)
        {
            VisualNode targetNode = target as VisualNode;
            bool isBehaviorTreeTask = targetNode.DeserializeType() is BehaviourTreeTask behaviourTreeTask;
            int counter = 0;
            // Creates a property field of the provided type for the serialized data saved in the array with the name propertyName
            int CreatePropertyField(SerializableType type, int index)
            {
                VisualNode targetNode = target as VisualNode;
                // Retrieve the serialized array
                
                string propertyPath = SerializationDataContainer.TypeToPath(type);
                SerializedProperty baseProperty = serializedObject.FindProperty("Data." + propertyPath + ".data");
                PropertyField field = new(baseProperty.GetArrayElementAtIndex(index).FindPropertyRelative("Value"));
                string key = targetNode.Data.GetKeyByIndex(index, type);
                field.label = key;
                field.BindProperty(serializedObject);
                element.Add(field);
                

                if (isBehaviorTreeTask)
                {
                    if (type == SerializableType.TREE && index == 0)
                    {
                        field.RegisterValueChangeCallback((x) => OnTreeChange(x.changedProperty.objectReferenceValue as BehaviourTreeAsset, element, targetNode, serializedObject));
                    }
                    else
                    {
                        //int boolIndex = 0;
                        //for (; targetNode.exposeToLLM.data[boolIndex].Key != key && boolIndex < targetNode.exposeToLLM.data.Count; boolIndex++) { }
                        SerializedProperty boolPropery = serializedObject.FindProperty("Data.exposeToLLM").GetArrayElementAtIndex(counter);
                        PropertyField fieldExposeToLLM = new(boolPropery);
                        fieldExposeToLLM.label = "LLM Function";
                        fieldExposeToLLM.BindProperty(serializedObject);
                        element.Add(fieldExposeToLLM);
                    }
                }
                counter++;
                return 0;
            }

            if (!(targetNode.GetCopyOfSerializedInterface() is BehaviourTreeTask) && !targetNode.CheckIntegrity())
            {
                targetNode.ReSerialize();
            }

            targetNode.Data.MapOverData(CreatePropertyField);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
