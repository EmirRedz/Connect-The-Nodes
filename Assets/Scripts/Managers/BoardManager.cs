using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;


public class EmptySpotData
{
    public Vector2 position;
    public int row;
    public int col;
}
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

        numberOfTerms = PlayerPrefs.GetInt(PlayerPrefsManager.CURRENT_NUMBER_OF_TERMS, 3);
    }

    private void Start()
    {
        GenerateNodes();
    }

    private void OnEnable()
    {
        GameManager.OnLevelUp += OnLevelUp;

    }

    private void OnDisable()
    {
        GameManager.OnLevelUp -= OnLevelUp;
    }

    private void GenerateNodes()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(i * nodeOffset, j * nodeOffset);

                var node = SpawnNode(position, i, j,true);
            }
        }
    }

    private Node SpawnNode(Vector2 position, int i, int j, bool isFirstSpawn = false)
    {
        var node = LeanPool.Spawn(nodePrefab, nodeContainer);
        node.name = "Node " + i + "," + j;

        node.transform.localPosition = position;

        node.termIndex = PlayerPrefs.GetInt(PlayerPrefsManager.TERM_INDEX + node.name,-1);

        int nodeTermIndex = 0;
        if (isFirstSpawn)
        {
            nodeTermIndex = node.termIndex < 0 ? Random.Range(0, numberOfTerms) : node.termIndex;
        }
        else
        {
            nodeTermIndex = Random.Range(0, numberOfTerms);
        }
        
        int nodeValue = Mathf.RoundToInt(CalculateGeometricNumber(nodeTermIndex));
            
        node.Init(nodeValue, nodeTermIndex,nodeColors[nodeTermIndex]);
        node.termIndex = nodeTermIndex;
        activeNodes.Add(node);
        return node;
    }

    float CalculateGeometricNumber(int index)
    {
        float result = firstTerm * Mathf.Pow(commonRatio, index);
        return result;
    }

    public int CalculateIndexFromResult(float result)
    {
        float index = Mathf.Log(result / firstTerm) / Mathf.Log(commonRatio);
        return Mathf.RoundToInt(index);
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
    
    private void MoveNode(Node node, int col, int row)
    {
        Vector3 newPosition = new Vector3(row * nodeOffset, col * nodeOffset, 0);
        
        node.transform.DOMove(newPosition, 0.05f).SetEase(Ease.Linear);
        node.name = "Node " + row + "," + col;
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
        
        var emptySpots = FindEmptyPositions();
        emptySpots.Sort((v1, v2) => v1.position.y.CompareTo(v2.position.y));
        
        foreach (EmptySpotData emptySpot in emptySpots)
        {
            var node = SpawnNode(emptySpot.position, emptySpot.row, emptySpot.col);
            node.transform.localScale = Vector3.zero;
            node.transform.DOScale(nodePrefab.transform.localScale, 0.06f);
        }

        foreach (Node node in activeNodes)
        {
            node.SaveValue();
        }
    }
    
    private List<EmptySpotData> FindEmptyPositions()
    {
        List<EmptySpotData> emptyPositions = new List<EmptySpotData>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(i * nodeOffset, j * nodeOffset);

                var colliders = Physics2D.OverlapCircleAll(position, 0.35f);
                if (colliders.Length <= 0)
                {
                    EmptySpotData data = new EmptySpotData();
                    data.position = position;
                    data.row = i;
                    data.col = j;
                    emptyPositions.Add(data);
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
                closestGeometricNumber = geometricNumber;
                termIndex = i;
            }
        }
        return closestGeometricNumber;
    }
    
    public Color GetTermColorByTermIndex()
    {
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
            lr.SetPosition(i, points[i].position);
        }
    }

    private void OnLevelUp()
    {
        numberOfTerms++;
        if (numberOfTerms > maxNumberOfTerms)
        {
            numberOfTerms = maxNumberOfTerms;
        }

        PlayerPrefs.SetInt(PlayerPrefsManager.CURRENT_NUMBER_OF_TERMS, numberOfTerms);
        PlayerPrefs.Save();
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
