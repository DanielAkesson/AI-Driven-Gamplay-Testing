using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasController : MonoBehaviour
{
    public RawImage Minimap;
    public RawImage Nearmap;
   
    void Start()
    {
        Arena a = FindObjectOfType<Arena>();
        Nearmap.texture = a.NearMap;
        Minimap.texture = a.Minimap;
    }
}
