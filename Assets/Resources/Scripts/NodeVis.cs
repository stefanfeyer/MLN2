using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NodeVis : MonoBehaviour
{
    private bool init = false;
    public Node node;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!init && node != null)
        {
            init = true;
            initialise();
            createLabel();
        }
    }

    private void initialise()
    {
        node.nodeVis = this;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        this.gameObject.AddComponent<MeshFilter>().mesh = go.GetComponent<MeshFilter>().mesh;
        Material[] mat = new Material[] { Resources.Load<Material>("Materials/nodeMaterial") };
        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/nodeMaterial");
        this.gameObject.AddComponent<SphereCollider>();
        Destroy(go);
        this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Vector3 randomPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        this.transform.position = randomPosition;
    }

    private void createLabel()
    {
        GameObject go = new GameObject();
        go.transform.SetParent(this.transform);
        go.name = "node label";
        go.AddComponent<TextMeshPro>().text = node.label;
        go.GetComponent<TextMeshPro>().horizontalAlignment = HorizontalAlignmentOptions.Center;
        go.GetComponent<TextMeshPro>().verticalAlignment = VerticalAlignmentOptions.Middle;
        go.GetComponent<TextMeshPro>().rectTransform.sizeDelta = new Vector2(1f, 0.1f);
        go.GetComponent<TextMeshPro>().rectTransform.localScale = new Vector3(1,1,1);
        go.GetComponent<TextMeshPro>().rectTransform.localPosition = new Vector3(0.5f, 0, 0);
        go.GetComponent<TextMeshPro>().fontSize = 3;
        go.GetComponent<TextMeshPro>().transform.LookAt(GameObject.FindGameObjectWithTag("Main Camera").transform);
    }
}
