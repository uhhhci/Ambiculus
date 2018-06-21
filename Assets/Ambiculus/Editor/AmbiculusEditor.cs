using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomEditor(typeof(Ambiculus))]
public class AmbiculusEditor : Editor
{

    //public GUISkin skin;
    GUISkin skin;
    int[,] pixelVisualization;
    int ledIndex = 0;
    public Ambiculus _target;

    float blockWidth = 28f;
    private bool listVisibility = true;
    void OnEnable()
    {
        skin = Resources.Load("AmbiculusSkin") as GUISkin;
        _target = (Ambiculus)target;
        ResetPixelVisualization(_target.width, _target.height);
        ledIndex = _target.pixels.Count;
        for (int i = 0; i < _target.pixels.Count; i++)
        {
            Ambiculus.LEDPixel pix = _target.pixels[i];
            pixelVisualization[_target.height - 1 - pix.r, pix.c] = i;
        }
    }
    public void ResetPixelVisualization(int w, int h, int v = -1)
    {
        pixelVisualization = new int[h, w];
        for (int r = 0; r < h; r++)
        {
            for (int c = 0; c < w; c++)
            {
                pixelVisualization[r, c] = v;
            }
        }
    }
    public void ListIterator(string propertyPath, ref bool visible)
    {
        SerializedProperty listProperty = serializedObject.FindProperty(propertyPath);
        visible = EditorGUILayout.Foldout(visible, listProperty.name);
        if (visible)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                Rect drawZone = GUILayoutUtility.GetRect(0f, 16f);
                bool showChildren = EditorGUI.PropertyField(drawZone, elementProperty);
                SerializedProperty pix_r = elementProperty.FindPropertyRelative("r");
                SerializedProperty pix_c = elementProperty.FindPropertyRelative("c");
                drawZone.x += 3 * blockWidth;
                EditorGUI.LabelField(drawZone, new GUIContent("r: " + pix_r.intValue));
                drawZone.x += 2 * blockWidth;
                EditorGUI.LabelField(drawZone, new GUIContent("c: " + pix_c.intValue));
            }
            EditorGUI.indentLevel--;
        }
    }

    public override void OnInspectorGUI()
    {
        //GUI.skin = skin;
        serializedObject.Update();
        bool changed = false;
        int nw = _target.width;
        nw = EditorGUILayout.IntField("Width", nw);
        if (nw != _target.width)
        {
            _target.width = nw % 2 == 0 ? nw : nw + 1; // ensure width can be divided by 2
            changed = true;
        }
        int nh = _target.height;
        nh = EditorGUILayout.IntField("Height", nh);
        if (nh != _target.height)
        {
            _target.height = nh;
            changed = true;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ambiculusImageEffect"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("myTexture2D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("comPort"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("multiplier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sendSignals"));
        SerializedProperty listProperty = serializedObject.FindProperty("pixels");

        if (pixelVisualization == null || changed)
        {
            ResetPixelVisualization(_target.width, _target.height);
            _target.pixels.Clear();
            listProperty.ClearArray();
            _target.ambiculusImageEffect.left = new RenderTexture(_target.width, _target.height, 24);
        }
        Rect rect = EditorGUILayout.GetControlRect();
        EditorGUI.PrefixLabel(rect, new GUIContent("On, off"));

        for (int r = 0; r < _target.height; r++)
        {
            Rect next = EditorGUILayout.GetControlRect();
            next.width = blockWidth;
            EditorGUIUtility.labelWidth = blockWidth / 2;


            for (int c = 0; c < _target.width; c++)
            {
                bool oldVal = false;
                if (pixelVisualization[r, c] >= 0)
                {
                    oldVal = true;
                }
                //bool newVal = EditorGUI.Toggle(next,""+b[r,c],oldVal,skin.toggle);
                bool newVal = EditorGUI.Toggle(next, oldVal, skin.toggle);
                EditorGUI.DropShadowLabel(next, "" + pixelVisualization[r, c]);

                next.x += blockWidth;
                if (newVal != oldVal)
                {
                    if (newVal)
                    {
                        pixelVisualization[r, c] = listProperty.arraySize;

                        listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                        SerializedProperty pix = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                        SerializedProperty pix_r = pix.FindPropertyRelative("r");
                        SerializedProperty pix_c = pix.FindPropertyRelative("c");
                        pix_r.intValue = _target.height - 1 - r; pix_c.intValue = c;
                    }
                    else
                    {
                        int index = pixelVisualization[r, c];
                        pixelVisualization[r, c] = -1;
                        listProperty.DeleteArrayElementAtIndex(index);
                        for (int i = 0; i < listProperty.arraySize; i++)
                        {
                            SerializedProperty pix = listProperty.GetArrayElementAtIndex(i);
                            SerializedProperty pix_r = pix.FindPropertyRelative("r");
                            SerializedProperty pix_c = pix.FindPropertyRelative("c");
                            if (pixelVisualization[_target.height - 1 - pix_r.intValue, pix_c.intValue] > index)
                            {
                                pixelVisualization[_target.height - 1 - pix_r.intValue, pix_c.intValue]--;
                            }
                        }
                    }
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
        ListIterator("pixels", ref listVisibility);
    }
}
