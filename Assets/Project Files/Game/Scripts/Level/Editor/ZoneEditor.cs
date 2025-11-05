using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(Zone))]
    public class ZoneEditor : CustomInspector
    {
        private readonly string[] TABS = new string[] { "Purchasing", "Auto Refs", "Containers" };
        private const int TAB_PURCHASING = 0;
        private const int TAB_REFS = 1;
        private const int TAB_CONTAINERS = 2;

        private int tabIndex = 0;

        private SerializedProperty scriptsProperty;
        private SerializedProperty idProperty;

        private SerializedProperty priceProperty;
        private SerializedProperty priceAmountProperty;

        private IEnumerable<SerializedProperty> purchasingProperties;
        private IEnumerable<SerializedProperty> refsProperties;
        private IEnumerable<SerializedProperty> containersProperties;

        private Zone targetZone;

        protected override void OnEnable()
        {
            base.OnEnable();

            scriptsProperty = serializedObject.FindProperty("m_Script");
            idProperty = serializedObject.FindProperty("id");

            priceProperty = serializedObject.FindProperty("price");
            priceAmountProperty = priceProperty.FindPropertyRelative("amount");

            purchasingProperties = serializedObject.GetPropertiesByGroup("Purchasing");
            refsProperties = serializedObject.GetPropertiesByGroup("Refs");
            containersProperties = serializedObject.GetPropertiesByGroup("Containers");

            targetZone = (Zone)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (scriptsProperty != null)
            {
                using (new EditorGUI.DisabledScope(disabled: true))
                {
                    EditorGUILayout.PropertyField(scriptsProperty);
                }
            }

            EditorGUILayout.PropertyField(idProperty);

            tabIndex = GUILayout.Toolbar(tabIndex, TABS);

            if(tabIndex == TAB_PURCHASING)
            {
                EditorGUILayout.BeginVertical(EditorCustomStyles.box);

                EditorGUILayout.PropertyField(priceProperty);

                if(priceAmountProperty.intValue == 0)
                {
                    EditorGUILayout.HelpBox("Purchase price is 0, this zone will be automatically opened.", MessageType.Info);
                }

                foreach (SerializedProperty prop in purchasingProperties)
                {
                    EditorGUILayout.PropertyField(prop);
                }

                EditorGUILayout.EndVertical();
            }
            else if(tabIndex == TAB_REFS)
            {
                EditorGUILayout.BeginVertical(EditorCustomStyles.box);

                using (new EditorGUI.DisabledScope(disabled: true))
                {
                    foreach (SerializedProperty prop in refsProperties)
                    {
                        EditorGUILayout.PropertyField(prop);
                    }
                }

                if(GUILayout.Button("Link Refs"))
                {
                    targetZone.OnSceneSaving();
                }

                EditorGUILayout.EndVertical();
            }
            else if(tabIndex == TAB_CONTAINERS)
            {
                EditorGUILayout.BeginVertical(EditorCustomStyles.box);

                foreach (SerializedProperty prop in containersProperties)
                {
                    EditorGUILayout.PropertyField(prop);
                }

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
