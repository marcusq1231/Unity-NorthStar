// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Provides simplified hand gesture metrics based on OVR hand tracking.
    /// </summary>
    public class HandGestureSensor : MonoBehaviour
    {
        [SerializeField] private OVRHand m_hand;
        [SerializeField, Range(0f, 1f)] private float m_openThreshold = 0.2f;
        [SerializeField, Range(0f, 1f)] private float m_closedThreshold = 0.7f;

        private static readonly OVRHand.HandFinger[] s_fingers =
        {
            OVRHand.HandFinger.Thumb,
            OVRHand.HandFinger.Index,
            OVRHand.HandFinger.Middle,
            OVRHand.HandFinger.Ring,
            OVRHand.HandFinger.Pinky
        };

        public bool IsTracked => m_hand != null && m_hand.IsTracked;

        public float GetFingerPinchStrength(OVRHand.HandFinger finger)
        {
            if (m_hand == null)
            {
                return 0f;
            }

            return m_hand.GetFingerPinchStrength(finger);
        }

        public float GetAveragePinchStrength()
        {
            if (m_hand == null)
            {
                return 0f;
            }

            float total = 0f;
            foreach (var finger in s_fingers)
            {
                total += m_hand.GetFingerPinchStrength(finger);
            }

            return total / s_fingers.Length;
        }

        public bool IsHandOpen()
        {
            return GetAveragePinchStrength() <= m_openThreshold;
        }

        public bool IsHandClosed()
        {
            return GetAveragePinchStrength() >= m_closedThreshold;
        }

        public bool IsFingerPinching(OVRHand.HandFinger finger, float threshold)
        {
            return GetFingerPinchStrength(finger) >= threshold;
        }

        public IReadOnlyList<OVRHand.HandFinger> GetFingers()
        {
            return s_fingers;
        }
    }
}
