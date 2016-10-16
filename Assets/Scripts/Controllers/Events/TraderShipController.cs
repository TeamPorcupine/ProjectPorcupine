#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using Animation;
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
    public SpritenameAnimation AnimationIdle;
    public SpritenameAnimation AnimationFlying;
    public SpriteRenderer Renderer;

    public void FixedUpdate()
    {
        if (GameController.Instance.IsPaused)
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

        if (distance > DestinationReachedThreshold * TimeManager.Instance.TimeScale)
        {
            // rotate the model
            Vector3 vectorToTarget = destination - transform.position;
            float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * Speed * TimeManager.Instance.TimeScale);

            // Direction to the next waypoint
            Vector3 dir = (destination - transform.position).normalized;
            dir *= Speed * Time.fixedDeltaTime * TimeManager.Instance.TimeScale;

            transform.position = transform.position + dir;
            AnimationFlying.Update(Time.fixedDeltaTime);
            ShowSprite(AnimationFlying.CurrentFrameName);
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
                AnimationIdle.Update(Time.fixedDeltaTime);
                ShowSprite(AnimationIdle.CurrentFrameName);
            }
        }
    }

    private void ShowSprite(string spriteName)
    {
        if (Renderer != null)
        {
            Renderer.sprite = SpriteManager.GetSprite("Trader", spriteName);
        }
    }
}