using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TestView))]
public class TestViewEditor : Editor {

    TestView testView;


    public bool testMode
    {
        get
        {

            bool _bool = testView.testMode;

            return _bool;
        }

        set
        {

            bool _bool = testView.testMode;

            if (_bool == value)
                return;

            testView.testMode = value;

            SetScriptingSymbols("TEST_MODE", value);
        }
    }

    void SetScriptingSymbols(string symbol, bool isActivate)
    {
        SetScriptingSymbol(symbol, BuildTargetGroup.Android, isActivate);
        SetScriptingSymbol(symbol, BuildTargetGroup.iOS, isActivate);
        SetScriptingSymbol(symbol, BuildTargetGroup.WSA, isActivate);
        SetScriptingSymbol(symbol, BuildTargetGroup.Standalone, isActivate);
        SetScriptingSymbol(symbol, BuildTargetGroup.WebGL, isActivate);
        SetScriptingSymbol(symbol, BuildTargetGroup.tvOS, isActivate);
    }

    void SetScriptingSymbol(string symbol, BuildTargetGroup target, bool isActivate)
    {

        var s = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

        if (isActivate && (s.Contains(symbol) || s.Contains(symbol + ";")))
            return;
        			
        s = s.Replace(symbol + ";", "");

        s = s.Replace(symbol, "");

        if (isActivate)
            s = symbol + ";" + s;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, s);
    }

    public override void OnInspectorGUI()
    {
        testView = (TestView)target;


        testMode = EditorGUILayout.BeginToggleGroup(new GUIContent("testMode   [?]", "Activate if you want test mode"), testMode);
        EditorGUILayout.EndToggleGroup();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(testView);

        }
    }
}

