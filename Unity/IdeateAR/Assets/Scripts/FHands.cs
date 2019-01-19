using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class FilterSettings
{
    public float BounceTime = 0.25f;
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
                //SocketClient.log("Pose duration: " + PoseDuration);
                lastPoseTime = Time.time;
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

public class FVector
{
    public Vector3 Value;

    public FVector()
    {

    }

    public FVector(Vector3 initial)
    {
        Value = initial;
    }

    public void Push(Vector3 input)
    {
        Value = Vector3.Lerp(Value, input, 0.5f);
        //Value = input;
    }
}
