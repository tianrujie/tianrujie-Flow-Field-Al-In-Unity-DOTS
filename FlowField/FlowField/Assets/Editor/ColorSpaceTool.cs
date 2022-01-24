using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using KG.TA;

public static class  ColorSpaceTool
{
    [MenuItem("Assets/Gama2Liner")]
    public static void Gama2Liner()
    {
        var assets = Selection.assetGUIDs;
        for (int i = 0; i < assets.Length; i++)
        {
            string p = AssetDatabase.GUIDToAssetPath(assets[i]);
            Texture2D sp = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            
            SRGB_Converter.ConvertGammaToLinear(sp);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(p);
        }
    } 
}
