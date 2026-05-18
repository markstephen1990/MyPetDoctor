using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FernGames
{
    [CustomEditor(typeof(TableBehaviour))]
    public class TableBehaviourEditor : CustomInspector
    {
        private readonly string[] TABS = new string[] { "Purchasing", "Refs" };
        private const int TAB_PURCHASING = 0;
        private const int TAB_REFS = 1;

        private int tabIndex;

        private SerializedProperty scriptsProperty;

        private SerializedProperty priceProperty;
        private SerializedProperty priceAmountProperty;

        private IEnumerable<SerializedProperty> purchasingProperties;
        private IEnumerable<SerializedProperty> refsProperties;

        private TableBehaviour tableBehaviour;

        protected override void OnEnable()
        {
            base.OnEnable();

            scriptsProperty = serializedObject.FindProperty("m_Script");

            priceProperty = serializedObject.FindProperty("price");
            priceAmountProperty = priceProperty.FindPropertyRelative("amount");

            purchasingProperties = serializedObject.GetPropertiesByGroup("Purchasing");
            refsProperties = serializedObject.GetPropertiesByGroup("Refs");

            tableBehaviour = (TableBehaviour)target;
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

                foreach (SerializedProperty prop in refsProperties)
                {
                    EditorGUILayout.PropertyField(prop);
                }

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
