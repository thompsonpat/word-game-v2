using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDuration = 0.5f;
    public string easingCuve = Easing.InOut;

    [Header("Set Dynamically")]
    public TextMesh tMesh;
    public Renderer tRend;

    public bool big = false;
    public List<Vector3> pts = null;
    public float timeStart = -1;
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
            Vector3 mid = (transform.position + value) / 2f;

            // Line from the actual midpoint
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            // Create a List<Vector3> of Bezier points
            pts = new List<Vector3>() { transform.position, mid, value };

            // If timeStart is at the default -1, then set it
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    // Moves immediately to the new position
    public Vector3 posImmediate
    {
        set { transform.position = value; }
    }

    // Interpolation code
    void Update()
    {
        if (timeStart == -1) return;

        // Standard linear interpolation code
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        // If the interpolation is done, set timeStart back to -1
        if (u == 1) timeStart = -1;
    }
}
