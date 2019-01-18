using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StickyNote : MonoBehaviour
{
    public TextMeshPro Label;

    public string Text { get { return Label.text; } set { Label.text = value; } }
}
