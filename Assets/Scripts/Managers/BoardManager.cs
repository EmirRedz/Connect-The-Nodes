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

    
    public  float nodeOffset = 0.875f;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 5;
    
    [Space(5)]
    
    [SerializeField] private float firstTerm = 2f;
    [SerializeField] private float commonRatio = 2f;
    [SerializeField] private int numberOfTerms = 5;

    public List<Node> activeNodes;

    private void Awake()
    {
        Instance = this;
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
                int geomatricNumber = Mathf.RoundToInt(CalculateGeometricNumber(randomTermIndex));
                node.Init(geomatricNumber, randomTermIndex);
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
        for (int i = 0; i < numberOfTerms; i++)
        {
            if (currentValue == Mathf.RoundToInt(CalculateGeometricNumber(i)))
            {
                return true;
                break;
            }
        }

        return false;
    }
}
