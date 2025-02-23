using System;

using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Scriptable Objects/Theme")]
[Serializable]
public class Theme : ScriptableObject
{
    public Sprite[] tiles = new Sprite[24];
}