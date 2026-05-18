using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FernGames
{
    [CustomEditor(typeof(TableZoneBehaviour))]
    public class TableZoneBehaviourEditor : CustomInspector
    {
        private readonly string[] TABS = new string[] { "Purchasing", "Refs" };
        private const int TAB_PURCHASING = 0;
        private const int TAB_REFS = 1;

        private int tabIndex;

        private SerializedProperty scriptsProperty;
        private SerializedProperty idProperty;

        private SerializedProperty priceProperty;
        private SerializedProperty priceAmountProperty;

        private SerializedProperty tableBehavioursProperty;

        private IEnumerable<SerializedProperty> purchasingProperties;
        private IEnumerable<SerializedProperty> refsProperties;

        private TableZoneBehaviour tableZoneBehaviour;

        protected override void OnEnable()
        {
            base.OnEnable();

            scriptsProperty = serializedObject.FindProperty("m_Script");
            idProperty = serializedObject.FindProperty("id");

            priceProperty = serializedObject.FindProperty("price");
            priceAmountProperty = priceProperty.FindPropertyRelative("amount");

            tableBehavioursProperty = serializedObject.FindProperty("tableBehaviours");

            purchasingProperties = serializedObject.GetPropertiesByGroup("Purchasing");
            refsProperties = serializedObject.GetPropertiesByGroup("Refs");

            tableZoneBehaviour = (TableZoneBehaviour)target;
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

            if (tabIndex == TAB_PURCHASING)
            {
                EditorGUILayout.BeginVertical(EditorCustomStyles.box);

                EditorGUILayout.PropertyField(priceProperty);

                if (priceAmountProperty.intValue == 0)
                {
                    EditorGUILayout.HelpBox("Purchase price is 0, this zone will be automatically opened.", MessageType.Info);
                }

                foreach (SerializedProperty prop in purchasingProperties)
                {
                    EditorGUILayout.PropertyField(prop);
                }

                EditorGUILayout.EndVertical();
            }
            else if (tabIndex == TAB_REFS)
            {
                EditorGUILayout.BeginVertical(EditorCustomStyles.box);

                using (new EditorGUI.DisabledScope(disabled: true))
                {
                    EditorGUILayout.PropertyField(tableBehavioursProperty);
                }

                foreach (SerializedProperty prop in refsProperties)
                {
                    EditorGUILayout.PropertyField(prop);
                }

                if (GUILayout.Button("Link Refs"))
                {
                    tableZoneBehaviour.OnSceneSaving();
                }

                EditorGUILayout.BeginHorizontal(EditorCustomStyles.box);
                if (GUILayout.Button("Locked"))
                {
                    tableZoneBehaviour.SetLockedState();
                }
                if (GUILayout.Button("Unlocked"))
                {
                    tableZoneBehaviour.SetUnlockedState();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
