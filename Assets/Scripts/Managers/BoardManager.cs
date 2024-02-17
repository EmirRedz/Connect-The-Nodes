using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [SerializeField] private Transform nodeContainer;
    [SerializeField] private Node nodePrefab;


    public float nodeOffset = 0.875f;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 5;

    [Space(5)] [SerializeField] private int firstTerm = 2;
    [SerializeField] private int commonRatio = 2;
    [SerializeField] private int maxNumberOfTerms;
    [SerializeField] private int numberOfTerms = 5;
    [SerializeField] private List<Color> nodeColors;
    private int termIndex;

    public List<Node> activeNodes;

    private LineRenderer lr;
    private List<Transform> points = new List<Transform>();
    private Transform lastPoint;

    private void Awake()
    {
        Instance = this;
        lr = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        GenerateNodes();
    }

    private void GenerateNodes()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(i * nodeOffset, j * nodeOffset);

                // GameObject nodeHolder = new GameObject("Node Holder " + i + "," + j);
                // nodeHolder.transform.SetParent(nodeContainer);
                // nodeHolder.transform.localPosition = position;

                var node = LeanPool.Spawn(nodePrefab, nodeContainer);
                node.transform.localPosition = position;

                int randomTermIndex = Random.Range(0, numberOfTerms);
                //int randomTermIndex = Random.Range(numberOfTerms,maxNumberOfTerms);
                int geomatricNumber = Mathf.RoundToInt(CalculateGeometricNumber(randomTermIndex));
                node.Init(geomatricNumber, nodeColors[randomTermIndex]);
                node.name = "Node " + i + "," + j;
                activeNodes.Add(node);
            }
        }
    }

    float CalculateGeometricNumber(int index)
    {
        float result = firstTerm * Mathf.Pow(commonRatio, index);
        return result;
    }

    [Button]
    public void MoveNodesDown()
    {
        StartCoroutine(MoveNodesDownCO());
    }

    private IEnumerator MoveNodesDownCO()
    {
        for (int i = 0; i < columns; i++)
        {
            foreach (Node activeNode in activeNodes)
            {
                if (activeNode.transform.position.y <= 0)
                {
                    continue;
                }

                activeNode.CheckIfGridBelowIsEmpty();
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    public bool IsCurrentValueEqualToGeometricNumber(int currentValue)
    {
        for (int i = 0; i < maxNumberOfTerms; i++)
        {
            if (currentValue == Mathf.RoundToInt(CalculateGeometricNumber(i)))
            {
                termIndex = i;
                return true;
            }
        }

        return false;
    }

    public Color GetTermColorByTermIndex()
    {
        Debug.Log("Term Index: " + termIndex);
        return nodeColors[termIndex];
    }

    public void GenerateLine(Transform finalPoint, Gradient lrColor)
    {
        lr.colorGradient = lrColor;
        if (lastPoint == null)
        {
            lastPoint = finalPoint;
            points.Add(lastPoint);
        }
        else
        {
            points.Add(finalPoint);
            lr.enabled = true;
            SetUpLine();
        }
    }

    public void ClearLineRenderer()
    {
        points.Clear();
        lr.positionCount = 0;
    }

    private void SetUpLine()
    {
        int length = points.Count;
        lr.positionCount = length;

        for (int i = 0; i < length; i++)
        {
            // var pointPos = points[i].position;
            // pointPos.z = 0.5f;
            // points[i].position = pointPos;

            lr.SetPosition(i, points[i].position);
        }
    }

    [Button]
    private void SetAlpha()
    {
        for (int i = 0; i < nodeColors.Count; i++)
        {
            var color = nodeColors[i];
            color.a = 1;
            nodeColors[i] = color;
        }
    }
}
