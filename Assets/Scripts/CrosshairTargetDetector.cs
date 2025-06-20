using UnityEngine;

public class CrosshairTargetDetector : MonoBehaviour
{
    public float detectionRange = 100f;
    public LayerMask enemyLayerMask;
    public CodeManager codeManager;

    private GameObject lastDetectedEnemy = null;

    void Update()
    {
        DetectEnemyInCrosshair();
    }

    void DetectEnemyInCrosshair()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * detectionRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))
        {
            GameObject hitObject = hit.collider.gameObject;
            string tag = hitObject.tag;
            int layer = hitObject.layer;
            string layerName = LayerMask.LayerToName(layer);

            // Only log and process if it's an enemy
            if (tag == "Warrior" || tag == "Armored Warrior" || tag == "Fire Skeleton" || tag == "Ice Skeleton")
            {
                if (hitObject != lastDetectedEnemy)
                {
                    lastDetectedEnemy = hitObject;
                    codeManager?.OnEnemyDetected(tag);
                }
            }
            else
            {
                lastDetectedEnemy = null;
            }
        }
        else
        {
            lastDetectedEnemy = null;
        }
    }

    

}
