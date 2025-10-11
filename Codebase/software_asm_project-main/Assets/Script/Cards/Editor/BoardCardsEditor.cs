using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(BoardCards))]
public class BoardCardsEditor : Editor
{
    SerializedProperty _cardID;
    SerializedProperty _cardDesc;
    SerializedProperty _moveToBoard;
    SerializedProperty _isMoveFixStep;
    SerializedProperty _moneyPaid;
    SerializedProperty _cardType;
    SerializedProperty _moveSteps;
    SerializedProperty _isPassGo;
    SerializedProperty _moveType;
    SerializedProperty _payFrom;
    SerializedProperty _payTo;
    SerializedProperty _cardSprite;
    SerializedProperty _houseRepairPrice;
    SerializedProperty _hotelRepairPrice;

    private void OnEnable()
    {
        _cardID = serializedObject.FindProperty("_cardID");
        _cardDesc = serializedObject.FindProperty("_cardDesc");
        _cardType = serializedObject.FindProperty("_cardType");
        _isMoveFixStep = serializedObject.FindProperty("_isMoveFixStep");
        _moveToBoard = serializedObject.FindProperty("_moveToBoard");
        _moneyPaid = serializedObject.FindProperty("_moneyPaid");
        _moveSteps = serializedObject.FindProperty("_moveSteps");
        _isPassGo = serializedObject.FindProperty("_isPassGo");
        _moveType = serializedObject.FindProperty("_moveType");
        _payFrom = serializedObject.FindProperty("_payFrom");
        _payTo = serializedObject.FindProperty("_payTo");
        _cardSprite = serializedObject.FindProperty("_cardSprite");
        _houseRepairPrice = serializedObject.FindProperty("_houseRepairPrice");
        _hotelRepairPrice = serializedObject.FindProperty("_hotelRepairPrice");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        //base.CreateInspectorGUI();
        EditorGUILayout.LabelField("General Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_cardID, new GUIContent("Card ID"));
        EditorGUILayout.PropertyField(_cardSprite, new GUIContent("Card Sprite"));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Card Description", EditorStyles.boldLabel);
        _cardDesc.stringValue = EditorGUILayout.TextArea(_cardDesc.stringValue, GUILayout.Height(100));
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Customize Field", EditorStyles.boldLabel);
        //if(_cardType.enumValueIndex == 0)
        //{
        //    _cardType.enumValueIndex = 1;
        //}
        EditorGUILayout.PropertyField(_cardType, new GUIContent("Card Type"));
        EditorGUILayout.Space(10);

        switch (_cardType.enumValueIndex) {
            case 0:
                EditorGUILayout.PropertyField(_isMoveFixStep, new GUIContent("  Is moving fix steps"));
                if (_isMoveFixStep.boolValue)
                {
                    EditorGUILayout.PropertyField(_moveSteps, new GUIContent("  Moving steps"));
                }
                else
                {
                    EditorGUILayout.PropertyField(_moveToBoard, new GUIContent("    Move to Board"));
                }
                EditorGUILayout.PropertyField(_isPassGo, new GUIContent("  Is passing GO"));
                EditorGUILayout.PropertyField(_moveType, new GUIContent("  Way of Moving"));
                break;
            case 1 or 4:
                EditorGUILayout.PropertyField(_payFrom, new GUIContent("    Pay money from"));
                EditorGUILayout.PropertyField(_payTo, new GUIContent("    Pay money to"));
                EditorGUILayout.PropertyField(_moneyPaid, new GUIContent("    Pay money amount"));
                break;
            case 3:
                EditorGUILayout.PropertyField(_houseRepairPrice, new GUIContent("    House Repair price"));
                EditorGUILayout.PropertyField(_hotelRepairPrice, new GUIContent("    Hotel Repair price"));
                break;

        }
        serializedObject.ApplyModifiedProperties();
    }
}
