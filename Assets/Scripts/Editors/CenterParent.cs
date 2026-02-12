using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CenterParent : MonoBehaviour
{
  
    [ContextMenu("Center Parent Around Children")]
    private void CenterParentAroundChildrenContextMenu()
    {
        // call the core functionality
        CenterParentAroundChildren();
    }

    // core functionality to center the parent around its children
    private void CenterParentAroundChildren()
    {
        // makesure to record undo only in edit mode
#if UNITY_EDITOR
        // only record undo in edit mode
        if (!Application.isPlaying)
        {
            // record the parent transform for undo
            Undo.RecordObject(transform, "Center Parent Around Children");
            
            // record each child's transform to maintain their world positions
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Undo.RecordObject(child, "Center Parent Around Children");
                }
            }
        }
#endif
        
        if (transform.childCount == 0)
        {
            Debug.LogWarning("parent " + gameObject.name + " no children to calculate center point.");
            return;
        }

        Vector3 sumPosition = Vector3.zero;
        int childCount = 0;

        // 1. calculate the sum of positions of all active children
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy) 
            {
                sumPosition += child.position;
                childCount++;
            }
        }

        if (childCount == 0)
        {
            Debug.LogWarning("parent " + gameObject.name + " no active children to calculate center point.");
            return;
        }

        // 2. calculate the center position
        Vector3 centerPosition = sumPosition / childCount;

        // 3.calculate the offset between the current parent position and the center position
        Vector3 offset = centerPosition - transform.position;

        // 4. Make the parent object move to the center position
        transform.position = centerPosition;

        // 5. maintain the world positions of the children by moving them in the opposite direction
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                child.position -= offset;
            }
        }

        Debug.Log("parent " + gameObject.name + " center position is " + centerPosition);
        
#if UNITY_EDITOR
        // Mark the scene as dirty to ensure changes are saved
        if (!Application.isPlaying)
        {
            // Only mark the scene dirty in edit mode
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}