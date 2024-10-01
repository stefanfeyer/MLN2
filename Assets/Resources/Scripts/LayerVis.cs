using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LayerVis : MonoBehaviour
{
    public Layer layer;
    private bool init = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!init && layer != null)
        {
            init = true;
            initialise();
            createLabel();
        }
    }

    private void initialise()
    {
        layer.layerVis = this;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        this.gameObject.AddComponent<MeshFilter>().mesh = go.GetComponent<MeshFilter>().mesh;
        this.gameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/layerMaterial");
        this.gameObject.AddComponent<BoxCollider>();
        Destroy(go);
        this.transform.SetParent(this.transform);
        this.name = layer.label;
    }

    public void setPosition(Vector3 _position)
    {
        this.transform.position = _position;
    }

    private void createLabel()
    {
        GameObject go = new GameObject();
        go.transform.SetParent(this.transform);
        go.name = "layer label";
        go.AddComponent<TextMeshPro>().text = layer.label;
        go.GetComponent<TextMeshPro>().horizontalAlignment = HorizontalAlignmentOptions.Center;
        go.GetComponent<TextMeshPro>().verticalAlignment = VerticalAlignmentOptions.Middle;
        go.GetComponent<TextMeshPro>().rectTransform.sizeDelta = new Vector2(1f, 0.1f);
        go.GetComponent<TextMeshPro>().rectTransform.localScale = new Vector3(1, 1, 1);
        go.GetComponent<TextMeshPro>().rectTransform.localPosition = new Vector3(0, 0.5f, 0);
        go.GetComponent<TextMeshPro>().fontSize = 0.3f;
        go.GetComponent<TextMeshPro>().transform.LookAt(GameObject.FindGameObjectWithTag("Main Camera").transform);
    }
}
