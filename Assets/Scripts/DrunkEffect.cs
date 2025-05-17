using UnityEngine;

public class DrunkEffect : MonoBehaviour
{
    private bool isDrunk = false;
    private float drunkTimer = 0f;
    private float drunkDuration = 30f; // 30 seconds of being drunk

    public void StartDrunkEffect()
    {
        Debug.Log("Starting drunk effect");
        isDrunk = true;
        drunkTimer = drunkDuration;
    }

    private void Update()
    {
        if (isDrunk)
        {
            drunkTimer -= Time.deltaTime;
            if (drunkTimer <= 0f)
            {
                Debug.Log("Drunk effect ended");
                isDrunk = false;
            }
        }
    }

    public bool IsDrunk()
    {
        return isDrunk;
    }
} 