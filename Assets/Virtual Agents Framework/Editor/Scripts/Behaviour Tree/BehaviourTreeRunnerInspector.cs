using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.BehaviourTrees;
using i5.VirtualAgents.BehaviourTrees.Visual;
using i5.VirtualAgents.Editor.BehaviourTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace i5.VirtualAgents.Editor
{
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerInspector : UnityEditor.Editor
    {
        // Root node of the inspector
        private VisualElement inspector;

        // The property fields used to display the properties of the currently selected node
        private List<PropertyField> propertyFieldsForCurrentNode = new List<PropertyField>();

        private NodeView currentlySelectedNode = null;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of the inspector UI
            inspector = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetManager.Load<VisualTreeAsset>("BehaviourTreeRunnerInspector.uxml");
            visualTree.CloneTree(inspector);

            // Setup the Behaviour Tree view
            BehaviourTreeView behaviourTreeView = inspector.Query<BehaviourTreeView>();
            behaviourTreeView.SetupManipulators(true);
            behaviourTreeView.OnNodeSelect = OnNodeSelectionChanged; // Register callback on node select in order to display the corresponding property fields for the node
            BehaviourTreeAsset tree = (target as BehaviourTreeRunner).Tree;

            void SetupNewTree(BehaviourTreeAsset tree)
            {
                if (tree != null)
                {
                    behaviourTreeView.Tree = tree;
                    behaviourTreeView.PopulateView(tree);
                }
            }

            SetupNewTree(tree);


            // Setup tree when a new one is selected
            PropertyField treePropertyField = inspector.Query<PropertyField>("tree");
            treePropertyField.RegisterValueChangeCallback((x) => SetupNewTree(x.changedProperty.objectReferenceValue as BehaviourTreeAsset));
            
            // Reset overwrite data on button press
            UnityEngine.UIElements.Button resetButton = inspector.Query<UnityEngine.UIElements.Button>("reset");
            resetButton.clicked += () => {
                if(currentlySelectedNode != null)
                {
                    var property = SearchValidOverwriteData(currentlySelectedNode,true);
                    CreatePropertyFields(property,currentlySelectedNode);
                }
            };
            

            // Return the finished inspector UI
            return inspector;
        }


        private void CreatePropertyFields(SerializedProperty serializedNodeOverwriteData, NodeView view)
        {
            // Clear old property fields
            foreach (var propertyField in propertyFieldsForCurrentNode)
            {
                propertyField.RemoveFromHierarchy();
            }
            propertyFieldsForCurrentNode.Clear();

            // Just a wrapper to pass targetNode and serializedNodeOverwriteData to CreatePropertyField, while having a valid signature for MapOverData
            int wrapper(SerializableType type, int index)
            {
                return CreatePropertyField(type,index,view.node,serializedNodeOverwriteData);
            }

            view.node.Data.MapOverData(wrapper);
        }

        private SerializedProperty SearchValidOverwriteData(NodeView view, bool forceReset)
        {
            BehaviourTreeRunner runner = target as BehaviourTreeRunner;
            var nodesData = runner.nodesOverwriteData.data;
            SerializedProperty serializedNodeOverwriteData = null;
            int entryIndex = nodesData.FindIndex((SerializationEntry<SerializationDataContainer> o) => o.Key == view.node.Guid);
            if(entryIndex >= 0)
            {
                SerializedProperty serializedArray = serializedObject.FindProperty("nodesOverwriteData.data");
                serializedNodeOverwriteData = serializedArray.GetArrayElementAtIndex(entryIndex).FindPropertyRelative("Value");
                // Check integrity
                if(!view.node.CheckIntegrity(nodesData[entryIndex].Value) || forceReset)
                {
                    serializedArray.DeleteArrayElementAtIndex(entryIndex);
                    serializedObject.ApplyModifiedProperties();
                    return CreateNodeOverwriteData(view);
                }
                return serializedNodeOverwriteData;
            }
            return CreateNodeOverwriteData(view);
        }

        private void OnNodeSelectionChanged(NodeView view)
        {
            currentlySelectedNode = view;
            SerializedProperty property = SearchValidOverwriteData(view, false);
            CreatePropertyFields(property,view);
            serializedObject.ApplyModifiedProperties();
        }


        private SerializedProperty CreateNodeOverwriteData(NodeView view)
        {
            BehaviourTreeRunner runner = target as BehaviourTreeRunner;
            SerializationDataContainer data = view.node.Data;
            SerializationDataContainer serializer = new()
            {
                serializationOrder = new(data.serializationOrder),
                exposeToLLM = new(data.exposeToLLM),
                serializedAudioClips = new(data.serializedAudioClips.data),
                serializedStrings = new(data.serializedStrings.data),
                serializedGameobjects = new(data.serializedGameobjects.data),
                serializedTrees = new(data.serializedTrees.data),
                serializedBools = new(data.serializedBools.data),
                serializedAudioSources = new(data.serializedAudioSources.data),
                serializedFloats = new(data.serializedFloats.data),
                serializedInts = new(data.serializedInts.data),
                serializedListFloats = new(data.serializedListFloats.data),
                serializedQuaternions = new(data.serializedQuaternions.data),
                serializedVectors = new(data.serializedVectors.data)
            };

            runner.nodesOverwriteData.Add(view.node.Guid, serializer);
            serializedObject.Update();

            int size = runner.nodesOverwriteData.data.Count;
            return serializedObject.FindProperty("nodesOverwriteData.data").GetArrayElementAtIndex(size - 1).FindPropertyRelative("Value");
        }

        // Creates a property field of the provided type for the serialized data saved in the array with the name propertyName
        private int CreatePropertyField(SerializableType type, int counter, VisualNode targetNode, SerializedProperty nodeOverwriteData)
        {
            string propertyName = SerializationDataContainer.TypeToPath(type);
            // Retrieve the serialized array
            SerializedProperty propertyArray = nodeOverwriteData.FindPropertyRelative(propertyName + ".data");
            if(propertyArray != null && counter < propertyArray.arraySize)
            {
                SerializedProperty propertyValue = propertyArray.GetArrayElementAtIndex(counter).FindPropertyRelative("Value");
                // Create the property field for the element with index counter
                PropertyField field = new PropertyField(propertyValue);
                field.label = targetNode.Data.GetKeyByIndex(counter, type);
                field.BindProperty(serializedObject);

                // Insert the field at the beginning of the inspector's children list
                inspector.Insert(inspector.childCount - 2, field); // Use the Insert method with index 0 to add the field above existing tree

                propertyFieldsForCurrentNode.Add(field);
            }
            else
            {
                Debug.LogWarning("Serialized property not found");
            }
            return 0;
        }
    }
}
