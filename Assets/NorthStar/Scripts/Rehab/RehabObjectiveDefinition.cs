// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    [Serializable]
    public class RehabObjectiveDefinition
    {
        public string Name;
        public RehabObjectiveType ObjectiveType = RehabObjectiveType.FlexExtend;
        [Min(1)] public int TargetRepetitions = 5;
        [Min(0f)] public float HoldSeconds = 1.5f;
        [Range(0f, 1f)] public float OpenThreshold = 0.2f;
        [Range(0f, 1f)] public float ClosedThreshold = 0.7f;
        [Range(0f, 1f)] public float PinchThreshold = 0.7f;
        public List<OVRHand.HandFinger> SequenceFingers = new()
        {
            OVRHand.HandFinger.Index,
            OVRHand.HandFinger.Middle,
            OVRHand.HandFinger.Ring,
            OVRHand.HandFinger.Pinky
        };
    }
}
