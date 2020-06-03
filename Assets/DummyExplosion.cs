using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyExplosion : MonoBehaviour
{
    public Renderer Renderer;
    private Color StartColor;
    void Start()
    {
        StartColor = Renderer.material.GetColor("_Color");
    }

    public void SetFade(float factor)
    {
        Color newC = StartColor;
        newC.a *= factor;
        Renderer.material.SetColor("_Color", newC);
    }
}
