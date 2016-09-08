#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public class TraderShipController : MonoBehaviour
{
    public Vector3 LeavingCoordinates;
    public Vector3 LandingCoordinates;
    public float Speed;
    public float DestinationReachedThreshold = 0.1f;
    public bool DestinationReached;
    public bool TradeCompleted;
    public Trader Trader;

    public void FixedUpdate()
    {
        if (WorldController.Instance.IsPaused)
        {
            return;
        }

        Vector3 destination = LandingCoordinates;

        if (DestinationReached && !TradeCompleted)
        {
            return;
        }

        if (TradeCompleted)
        {
            destination = LeavingCoordinates;
        }

        float distance = Vector3.Distance(transform.position, destination);

        if (distance > DestinationReachedThreshold * WorldController.Instance.TimeScale)
        {
            // rotate the model
            Vector3 vectorToTarget = destination - transform.position;
            float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * Speed * WorldController.Instance.TimeScale);

            // Direction to the next waypoint
            Vector3 dir = (destination - transform.position).normalized;
            dir *= Speed * Time.fixedDeltaTime * WorldController.Instance.TimeScale;

            transform.position = transform.position + dir;
        }
        else
        {
            DestinationReached = true;
            if (TradeCompleted)
            {
                Destroy(this.gameObject);
            }
            else
            {
                WorldController.Instance.TradeController.ShowTradeDialogBox(this);
            }
        }
    }
}