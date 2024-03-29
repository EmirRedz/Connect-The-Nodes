using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private SpriteRenderer sr;
    public int nodeValue;
    public int termIndex;
    
    public bool isSelected ;
    public bool isMerged;
    [SerializeField] private float mergeTime = 0.15f;
    private bool isDragging;

    private List<Node> connectedNodes = new List<Node>();
  
    private Vector3 selectedScale;
    private Vector3 normalScale;

    private int connectValue;

    private Node currentConnectedNode;
    
    
    #region Line Renderer Variables
    private Gradient gradient;
    private GradientColorKey[] colorKeys;
    private GradientAlphaKey[] alphaKeys;
    #endregion

    public void Init(int value, int index, Color colorToSet)
    {
        isDragging = false;
        isMerged = false;
        isSelected = false;
        currentConnectedNode = null;
        connectValue = 0;
        termIndex = index;
        nodeValue = value;
        if (nodeValue >= 1000)
        {
            string nodeValueK = (nodeValue / 1000) + "K";
            numberText.SetText(nodeValueK);

        }
        else
        {
            numberText.SetText(nodeValue.ToString());
        }
        normalScale = transform.localScale;
        selectedScale = transform.localScale * 1.2f;

        sr.color = colorToSet;

        #region Line Renderer Color Set Up

        gradient = new Gradient();
        colorKeys = new GradientColorKey[2];
        alphaKeys = new GradientAlphaKey[2];

        colorKeys[0].color = sr.color;
        colorKeys[0].time = 0.0f;
        alphaKeys[0].alpha = sr.color.a;
        alphaKeys[0].time = 0.0f;

        colorKeys[1].color = sr.color;
        colorKeys[1].time = 1.0f;
        alphaKeys[1].alpha = sr.color.a;
        alphaKeys[1].time = 1.0f;

        gradient.SetKeys(colorKeys, alphaKeys);

        for (float t = 0.0f; t <= 1.0f; t += 1.0f / 2)
        {
            Color interpolatedColor = gradient.Evaluate(t);
        }

        #endregion
    }

    private void OnMouseDown()
    {
        StartDrag();
    }

    private void OnMouseUp()
    {
        StopDrag();
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            OnContinueDrag();
        }
    }

    private void StartDrag()
    {
        SelectNode();
        BoardManager.Instance.GenerateLine(transform, gradient);
        isDragging = true;
        currentConnectedNode = this;
    }

    public void SelectNode()
    {
        transform.DOKill(true);
        transform.DOScale(selectedScale, 0.25f).SetEase(Ease.Linear);
        isSelected = true;
        AudioManager.Instance.PlaySound2D("SelectNode");
    }

    private void StopDrag()
    {
        UnselectNode();
        isDragging = false;
        CheckForMerge(connectedNodes);
        GameManager.Instance.SetCurrentBonusText(0, Color.clear);
        BoardManager.Instance.ClearLineRenderer();

    }

    public void UnselectNode()
    {
        transform.DOKill(true);
        transform.DOScale(normalScale, 0.25f);
        isSelected = false;
        if (!isMerged)
        {
            AudioManager.Instance.PlaySound2D("UnselectNode");
        }
    }

    private void OnContinueDrag()
    {
        RaycastHit2D hit = Physics2D.Raycast(GameManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            Node targetNode = hit.collider.GetComponent<Node>();
            
            if (targetNode != null && IsTargetNodeNeighbor(targetNode))
            {
                if (targetNode == this)
                {
                    currentConnectedNode = this;
                    connectedNodes[0].UnselectNode();
                    RemoveNodeFromList(connectedNodes[0]);
                    BoardManager.Instance.UnselectLineRendererUpdate();

                }
                else if (connectedNodes.Contains(targetNode) && connectedNodes.IndexOf(targetNode) != connectedNodes.Count - 1)
                {
                    var nodeToRemove = connectedNodes[connectedNodes.IndexOf(targetNode) + 1];
                    currentConnectedNode = targetNode;
                    RemoveNodeFromList(nodeToRemove);
                    nodeToRemove.UnselectNode();
                    BoardManager.Instance.UnselectLineRendererUpdate();
                    
                }
                else if (!targetNode.isSelected && targetNode.nodeValue == nodeValue)
                {
                    AddNodeToList(targetNode);
                    targetNode.SelectNode();
                    currentConnectedNode = targetNode;
                    BoardManager.Instance.GenerateLine(targetNode.transform,gradient);
                }
               
            }
            
        }
    }

    private bool IsTargetNodeNeighbor(Node targetNode)
    {
        Vector2[] directions =
        {
            Vector2.up * BoardManager.Instance.nodeOffset, 
            Vector2.down * BoardManager.Instance.nodeOffset,
            Vector2.left * BoardManager.Instance.nodeOffset, 
            Vector2.right * BoardManager.Instance.nodeOffset,
            new Vector2(-1, 1) * BoardManager.Instance.nodeOffset,
            new Vector2(1, 1) * BoardManager.Instance.nodeOffset,
            new Vector2(-1, -1) * BoardManager.Instance.nodeOffset,
            new Vector2(1, -1) * BoardManager.Instance.nodeOffset
        };

        foreach (Vector2 direction in directions)
        {
            Vector2 neighborPosition = currentConnectedNode.transform.position + (Vector3)direction;
            RaycastHit2D hit = Physics2D.Raycast(neighborPosition, Vector2.zero);

            if (hit.collider != null)
            {
                Node neighborNode = hit.collider.GetComponent<Node>();
                if (targetNode == neighborNode)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void AddNodeToList(Node targetNode)
    {
        if (connectedNodes.Contains(targetNode))
        {
            return;
        }

        connectedNodes.Add(targetNode);
        SetCurrentBonusAmount();
    }
    
    private void RemoveNodeFromList(Node targetNode)
    {
        if (!connectedNodes.Contains(targetNode))
        {
            return;
        }

        connectedNodes.Remove(targetNode);
        SetCurrentBonusAmount();
    }
    
    private void SetCurrentBonusAmount()
    {
        int currentNodeValue = nodeValue * (connectedNodes.Count + 1);

        if (currentNodeValue < connectValue)
        {
            var lowestGeometricNumber = BoardManager.Instance.GetClosestGeometricNumber(currentNodeValue);
            connectValue = (int)lowestGeometricNumber;
            GameManager.Instance.SetCurrentBonusText(connectValue, BoardManager.Instance.GetTermColorByTermIndex());

        }
        if (BoardManager.Instance.IsCurrentValueEqualToGeometricNumber(currentNodeValue))
        {
            connectValue = currentNodeValue;
            GameManager.Instance.SetCurrentBonusText(connectValue, BoardManager.Instance.GetTermColorByTermIndex());
        }
    }

    void CheckForMerge(List<Node> nodesToMerge)
    {
        if (nodesToMerge.Count > 0)
        {
            MergeNodes(nodesToMerge);
        }
    }
    
    void MergeNodes(List<Node> nodesToMerge)
    {
        isMerged = true;
        
        BoardManager.Instance.activeNodes.Remove(this);

        for (var i = 0; i < nodesToMerge.Count; i++)
        {
            nodesToMerge[i].isMerged = true;
            nodesToMerge[i].UnselectNode();

            if (i == nodesToMerge.Count - 1)
            {
                continue;
            }

            var index = i;
            nodesToMerge[i].transform.DOMove(nodesToMerge[^1].transform.position, mergeTime).SetEase(Ease.Linear).OnComplete((() =>
            {
                LeanPool.Despawn(nodesToMerge[index].gameObject);
            }));
            BoardManager.Instance.activeNodes.Remove(nodesToMerge[index]);

        }

        transform.DOMove(nodesToMerge[^1].transform.position, mergeTime).SetEase(Ease.Linear).OnComplete((() =>
        {
            LeanPool.Despawn(gameObject);
            
            BoardManager.Instance.MoveNodesDown();
            
            nodesToMerge[^1].Init(connectValue, BoardManager.Instance.CalculateIndexFromResult(connectValue), BoardManager.Instance.GetTermColorByTermIndex());
            connectedNodes.Clear();
            GameManager.Instance.AddXP(connectValue);

        }));
    }

    public void SaveValue()
    {
        PlayerPrefs.SetInt(PlayerPrefsManager.TERM_INDEX + gameObject.name, termIndex);
        PlayerPrefs.Save();
    }
}
