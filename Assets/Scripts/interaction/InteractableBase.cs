using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E;

    protected bool playerInRange = false;

    // 玩家进入检测范围
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            OnPlayerEnterRange();
        }
    }

    // 玩家离开检测范围
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            OnPlayerExitRange();
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (playerInRange || CanInteractOutsideRange())
            {
                Interact();
            }
        }
    }

    // 是否允许在范围外交互
    protected virtual bool CanInteractOutsideRange()
    {
        return false;
    }

    // 统一交互入口
    public void Interact()
    {
        OnInteract();
    }

    // 子类提供具体交互内容
    protected abstract void OnInteract();

    // 玩家进入范围时的额外处理（可选）
    protected virtual void OnPlayerEnterRange()
    {
    }

    // 玩家离开范围时的额外处理（可选）
    protected virtual void OnPlayerExitRange()
    {
    }
}