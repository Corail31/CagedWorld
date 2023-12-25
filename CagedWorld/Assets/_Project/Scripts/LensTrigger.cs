using UnityEngine;

public class LensTrigger : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            PortalManager.Instance.OnLensTriggeringPortal(this.transform, other, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            PortalManager.Instance.OnLensTriggeringPortal(this.transform, other, false);
        }
    }
}
