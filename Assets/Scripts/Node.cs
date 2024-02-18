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
    public bool isSelected ;
    public bool isMerged;
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
    public void Init(int value, Color colorToSet)
    {
        isDragging = false;
        isMerged = false;
        isSelected = false;
        currentConnectedNode = null;
        
        nodeValue = value;
        numberText.SetText(nodeValue.ToString());
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
    }

    private void StopDrag()
    {
        UnselectNode();
        isDragging = false;
        CheckForMerge(connectedNodes);
        GameManager.Instance.SetCurrentBonusText(0);
        BoardManager.Instance.ClearLineRenderer();

    }

    public void UnselectNode()
    {
        transform.DOKill(true);
        transform.DOScale(normalScale, 0.25f);
        isSelected = false;
    }

    public void CheckIfGridBelowIsEmpty()
    {
        Vector2 belowNeighborPosition = transform.position + ((Vector3)Vector2.down * BoardManager.Instance.nodeOffset);
        RaycastHit2D hit = Physics2D.Raycast(belowNeighborPosition, Vector2.zero);

        if (hit.collider != null || transform.position.y <= 0)
        {
            return;
        }
        transform.DOKill(true);
        transform.DOMove(transform.position + (Vector3)(Vector2.down * BoardManager.Instance.nodeOffset), 0.25f);

    }

    private void OnContinueDrag()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            Node targetNode = hit.collider.GetComponent<Node>();

            if (targetNode != null && !targetNode.isSelected && targetNode.nodeValue == nodeValue && IsTargetNodeNeighbor(targetNode))
            {
                AddNodeToList(targetNode);
                targetNode.SelectNode();
                currentConnectedNode = targetNode;
                BoardManager.Instance.GenerateLine(targetNode.transform,gradient);
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
        
        int currentNodeValue = nodeValue * (connectedNodes.Count + 1);
        
        Debug.Log("Current node value:"  + currentNodeValue);
        if (BoardManager.Instance.IsCurrentValueEqualToGeometricNumber(currentNodeValue))
        {
            connectValue = currentNodeValue;
            GameManager.Instance.SetCurrentBonusText(connectValue);
        }
    }

    void CheckForMerge(List<Node> nodesToMerge)
    {
        if (nodesToMerge.Count > 0)
        {
            MergePops(nodesToMerge);
        }
    }
    
    void MergePops(List<Node> nodesToMerge)
    {
        isMerged = true;
        
        BoardManager.Instance.activeNodes.Remove(nodesToMerge[^1]);
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
            nodesToMerge[i].transform.DOMove(nodesToMerge[^1].transform.position, 0.75f).SetEase(Ease.Linear).OnComplete((() =>
            {
                LeanPool.Despawn(nodesToMerge[index].gameObject);
            }));
            BoardManager.Instance.activeNodes.Remove(nodesToMerge[index]);

        }

        transform.DOMove(nodesToMerge[^1].transform.position, 0.75f).SetEase(Ease.Linear);

        DOVirtual.DelayedCall(1f, () =>
        {
            LeanPool.Despawn(gameObject);
            
            BoardManager.Instance.MoveNodesDown();
            
            nodesToMerge[^1].Init(connectValue, BoardManager.Instance.GetTermColorByTermIndex());
            connectedNodes.Clear();

        });
    }
    
}
