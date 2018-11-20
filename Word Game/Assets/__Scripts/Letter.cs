using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set Dynamically")]
    public TextMesh tMesh;
    public Renderer tRend;

    public bool big = false;
    private char _c;
    private Renderer rend;

    void Awake()
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();
        visible = false;
    }

	// Property to get or set the _c and the Letter shown by 3D Text
	public char c
    {
        get { return _c; }
        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }

	public string str
    {
        get { return _c.ToString(); }
        set { _c = value[0]; }
    }

	// Enables/disables the renderer for 3D Text, which causes the char to show/hide
	public bool visible
    {
        get { return tRend.enabled; }
        set { tRend.enabled = value; }
    }

	// The color of the rounded rectangle
	public Color color
    {
        get { return rend.material.color; }
        set { rend.material.color = value; }
    }

	// Sets the position of the Letter's gameObject
	public Vector3 pos
    {
        set
        {
            transform.position = value;
        }
    }
}
