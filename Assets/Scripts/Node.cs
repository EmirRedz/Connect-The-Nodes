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
    [SerializeField] private List<Color> nodeColor;
    [SerializeField] private SpriteRenderer sr;
    public int nodeValue;
    public bool isSelected ;
    public bool isMerged;
    private bool isDragging;

    private List<Node> connectedNodes = new List<Node>();
  
    private Vector3 selectedScale;
    private Vector3 normalScale;
    public void Init(int value, int termIndex)
    {
        nodeValue = value;
        numberText.SetText(nodeValue.ToString());
        normalScale = transform.localScale;
        selectedScale = transform.localScale * 1.2f;

        sr.color = nodeColor[termIndex];
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
        isDragging = true;
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

        // foreach (Node activeNode in BoardManager.Instance.activeNodes)
        // {
        //     activeNode.CheckIfGridBelowIsEmpty();
        // }
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

        if (hit.collider == null)
        {
            Debug.Log("Neighbor spot of: "+ gameObject.name + " is empty!!");
        }
        else
        {
            Debug.Log("Neighbor spot of: "+ gameObject.name + " is full!!");
        }
    }
    private void OnContinueDrag()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            Node targetNode = hit.collider.GetComponent<Node>();

            if (targetNode != null && !targetNode.isSelected && targetNode.nodeValue == nodeValue)
            {
                AddNodeToList(targetNode);
                targetNode.SelectNode();
            }
        }
    }

    private void AddNodeToList(Node targetNode)
    {
        if (connectedNodes.Contains(targetNode))
        {
            return;
        }

        connectedNodes.Add(targetNode);
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

        transform.DOMove(nodesToMerge[^1].transform.position, 0.75f).SetEase(Ease.Linear).OnComplete((() =>
        {
            LeanPool.Despawn(nodesToMerge[^1].gameObject);
            LeanPool.Despawn(gameObject);
        }));
        connectedNodes.Clear();
    }

    [Button]
    private void SetAlpha()
    {
        for (int i = 0; i < nodeColor.Count; i++)
        {
            var color = nodeColor[i];
            color.a = 1;
            nodeColor[i] = color;
        }
    }
}
