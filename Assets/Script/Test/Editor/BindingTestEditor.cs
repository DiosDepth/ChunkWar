using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

//[CustomEditor(typeof(BindingTest))]
public class BindingTestEditor : Editor
{
    private GUIContent m_BindingLabel = new GUIContent("Binding");
    private int m_SelectedBindingOption = 0;
    private GUIContent[] m_BindingOptions;

    private SerializedProperty _action;
    private SerializedProperty _overlay;
    private SerializedProperty _bindingIndex;
    private SerializedProperty _bindingid;

    private InputAction action;

    public void OnEnable()
    {


        _action = serializedObject.FindProperty("targetAction");
        _overlay = serializedObject.FindProperty("overlay");
        _bindingIndex = serializedObject.FindProperty("bindingIndex");
        _bindingid = serializedObject.FindProperty("bindingid");

        InputActionReference actionref =(InputActionReference)_action.objectReferenceValue;
        action = actionref?.action;

        ReadOnlyArray<InputBinding> bindings = action.bindings;
        int count = bindings.Count;

        m_BindingOptions = new GUIContent[count];
        


        
        for (int i = 0; i < count; i++)
        {
            InputBinding binding = bindings[i];
            string bindingid = binding.id.ToString();

            InputBinding.DisplayStringOptions displayOptions = InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
            var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);
            var isComposite = binding.isComposite;

            if(!haveBindingGroups)
            {
                displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;
            }

            string displayString = action.GetBindingDisplayString(i, displayOptions);

            displayString = ObjectNames.NicifyVariableName(displayString);
            displayString = displayString.Replace('/', '\\');


            m_BindingOptions[i] = new GUIContent(displayString); 
        }

        _bindingIndex.intValue = m_SelectedBindingOption;
        _bindingid.stringValue = action.bindings[m_SelectedBindingOption].id.ToString();
    }


    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_action);
        EditorGUILayout.PropertyField(_overlay);
        int newSelectedBindingOption = EditorGUILayout.Popup(m_BindingLabel, m_SelectedBindingOption, m_BindingOptions);
        if(newSelectedBindingOption != m_SelectedBindingOption)
        {
            m_SelectedBindingOption = newSelectedBindingOption;
            Debug.Log("Selected binding option is : " + m_BindingOptions[m_SelectedBindingOption].text);
            _bindingIndex.intValue = m_SelectedBindingOption;
            _bindingid.stringValue = action.bindings[m_SelectedBindingOption].id.ToString();

        }

        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

    }
}
