using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [SerializeField] private Transform nodeContainer;
    [SerializeField] private Node nodePrefab;


    public float nodeOffset = 0.875f; 
    public int rows = 5;
    public int columns = 5;

    [Space(5)] 
    [SerializeField] private int firstTerm = 2;
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

                var node = SpawnNode(position);

                node.name = "Node " + i + "," + j;
            }
        }
    }

    private Node SpawnNode(Vector2 position)
    {
        var node = LeanPool.Spawn(nodePrefab, nodeContainer);
        node.transform.localPosition = position;

        int randomTermIndex = Random.Range(0, numberOfTerms);
        //int randomTermIndex = Random.Range(numberOfTerms,maxNumberOfTerms);
        int geomatricNumber = Mathf.RoundToInt(CalculateGeometricNumber(randomTermIndex));
        node.Init(geomatricNumber, nodeColors[randomTermIndex]);
        activeNodes.Add(node);
        return node;
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
    
    private Node GetNodeAt(int row, int col)
    {
        return activeNodes.Find(nodeTransform => Mathf.RoundToInt(nodeTransform.transform.position.y / nodeOffset) == row && Mathf.RoundToInt(nodeTransform.transform.position.x / nodeOffset) == col);
    }
    
    private void MoveNode(Node node, int newRow, int col)
    {
        Vector3 newPosition = new Vector3(col * nodeOffset, newRow * nodeOffset, 0);
        
        node.transform.DOMove(newPosition, 0.05f).SetEase(Ease.Linear);
    }

    private IEnumerator MoveNodesDownCO()
    {
        for (int col = 0; col < columns; col++)
        {
            int emptySpaces = 0;

            for (int row = 0; row < rows; row++)
            {
                var currentNode = GetNodeAt(row, col);

                if (currentNode == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    MoveNode(currentNode, row - emptySpaces, col);
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        
        var emptyPositions = FindEmptyPositions();
        emptyPositions.Sort((v1, v2) => v1.y.CompareTo(v2.y));
        
        foreach (Vector2 emptyPosition in emptyPositions)
        {
            var node = SpawnNode(emptyPosition);
            node.transform.localScale = Vector3.zero;
            node.transform.DOScale(nodePrefab.transform.localScale, 0.06f);
        }
        
    }
    
    private List<Vector2> FindEmptyPositions()
    {
        List<Vector2> emptyPositions = new List<Vector2>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(i * nodeOffset, j * nodeOffset);

                var colliders = Physics2D.OverlapCircleAll(position, 0.35f);
                if (colliders.Length <= 0)
                {
                    emptyPositions.Add(position);
                }
            }
        }

        return emptyPositions;
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

    public float GetClosestGeometricNumber(int targetValue)
    {
        float closestGeometricNumber = float.MinValue;

        for (int i = 0; i < maxNumberOfTerms; i++)
        {
            float geometricNumber = CalculateGeometricNumber(i);

            if (geometricNumber <= targetValue && geometricNumber > closestGeometricNumber)
            {
                // If the current geometric number is less than or equal to the target value and greater than the current closest, update the closest.
                closestGeometricNumber = geometricNumber;
                termIndex = i;
            }
        }

        // Return the closest geometric number found.
        return closestGeometricNumber;
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
    public void UnselectLineRendererUpdate()
    {
        var positionCount = lr.positionCount;
        positionCount--;
        lr.positionCount = positionCount;
        points.SetLength(positionCount);
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
