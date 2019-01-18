using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class FilterSettings
{
    public float BounceTime = 1.5f;
    public float MinConfidence = 0.5f;
}

public class FHands
{
    public static FHand Left, Right;

    static FilterSettings settings = new FilterSettings();


    static FHands()
    {
        if (!MLHands.IsStarted) return;
        Left = new FHand(MLHands.Left, settings);
        Right = new FHand(MLHands.Right, settings);
    }
}

public class FHand
{
    FilterSettings settings;
    float lastPoseTime;

    MLHand hand;
    MLHandKeyPose currentPose;
    MLHandKeyPose lastPose;

    public MLHandKeyPose KeyPose
    {
        get
        {
            var newPose = hand.KeyPose;
            if (hand.KeyPoseConfidence > settings.MinConfidence && newPose != currentPose && PoseDuration > settings.BounceTime)
            {
               
                currentPose = newPose;
            }

            return currentPose;
        }
    }

    public float PoseDuration
    {
        get
        {
            return Time.time - lastPoseTime;
        }
    }

    public FHand(MLHand hand, FilterSettings settings)
    {
        this.hand = hand;
        this.settings = settings;
    }
}
