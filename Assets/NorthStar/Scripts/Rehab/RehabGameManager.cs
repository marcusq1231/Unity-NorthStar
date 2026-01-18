// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Drives story beats and objective progression for the rehab experience.
    /// </summary>
    public class RehabGameManager : MonoBehaviour
    {
        [SerializeField] private HandGestureSensor m_handSensor;
        [SerializeField] private List<RehabLevelDefinition> m_levels = new();
        [SerializeField, Min(0f)] private float m_objectiveStartDelay = 0.5f;

        public UnityEvent<string> OnLevelTitle;
        public UnityEvent<string> OnStoryIntro;
        public UnityEvent<string> OnStoryOutro;
        public UnityEvent<string> OnObjectiveName;
        public UnityEvent<int, int> OnObjectiveProgress;
        public UnityEvent OnAllLevelsComplete;

        private int m_levelIndex;
        private int m_objectiveIndex;
        private float m_delayTimer;
        private RehabObjectiveRuntime m_activeObjective;
        private bool m_waitingForObjective;

        private void Start()
        {
            if (m_levels.Count > 0)
            {
                StartLevel(0);
            }
        }

        private void Update()
        {
            if (m_activeObjective == null || m_waitingForObjective)
            {
                if (m_waitingForObjective)
                {
                    m_delayTimer -= Time.deltaTime;
                    if (m_delayTimer <= 0f)
                    {
                        m_waitingForObjective = false;
                    }
                }
                return;
            }

            if (m_handSensor != null && !m_handSensor.IsTracked)
            {
                return;
            }

            m_activeObjective.Update(Time.deltaTime);
            OnObjectiveProgress?.Invoke(m_activeObjective.CompletedRepetitions, m_activeObjective.TargetRepetitions);

            if (m_activeObjective.IsComplete)
            {
                AdvanceObjective();
            }
        }

        public void StartLevel(int index)
        {
            if (index < 0 || index >= m_levels.Count)
            {
                return;
            }

            m_levelIndex = index;
            m_objectiveIndex = 0;
            var level = m_levels[m_levelIndex];
            OnLevelTitle?.Invoke(level.LevelTitle);
            OnStoryIntro?.Invoke(level.StoryIntro);
            StartObjective(level);
        }

        public void AdvanceObjective()
        {
            var level = m_levels[m_levelIndex];
            m_objectiveIndex++;
            if (m_objectiveIndex >= level.Objectives.Count)
            {
                OnStoryOutro?.Invoke(level.StoryOutro);
                AdvanceLevel();
                return;
            }

            StartObjective(level);
        }

        private void StartObjective(RehabLevelDefinition level)
        {
            if (m_objectiveIndex < 0 || m_objectiveIndex >= level.Objectives.Count)
            {
                return;
            }

            m_activeObjective = new RehabObjectiveRuntime(level.Objectives[m_objectiveIndex], m_handSensor);
            OnObjectiveName?.Invoke(m_activeObjective.Name);
            OnObjectiveProgress?.Invoke(0, m_activeObjective.TargetRepetitions);
            m_delayTimer = m_objectiveStartDelay;
            m_waitingForObjective = m_objectiveStartDelay > 0f;
        }

        private void AdvanceLevel()
        {
            m_levelIndex++;
            if (m_levelIndex >= m_levels.Count)
            {
                OnAllLevelsComplete?.Invoke();
                return;
            }

            StartLevel(m_levelIndex);
        }

        private class RehabObjectiveRuntime
        {
            private readonly RehabObjectiveDefinition m_definition;
            private readonly HandGestureSensor m_handSensor;
            private int m_sequenceIndex;
            private float m_holdTimer;
            private bool m_waitingForRelease;
            private bool m_wasOpen;
            private bool m_wasClosed;
            private bool m_wasPinching;

            public string Name => string.IsNullOrWhiteSpace(m_definition.Name)
                ? m_definition.ObjectiveType.ToString()
                : m_definition.Name;

            public int TargetRepetitions => m_definition.TargetRepetitions;
            public int CompletedRepetitions { get; private set; }
            public bool IsComplete => CompletedRepetitions >= TargetRepetitions;

            public RehabObjectiveRuntime(RehabObjectiveDefinition definition, HandGestureSensor handSensor)
            {
                m_definition = definition;
                m_handSensor = handSensor;
            }

            public void Update(float deltaTime)
            {
                if (IsComplete || m_handSensor == null)
                {
                    return;
                }

                switch (m_definition.ObjectiveType)
                {
                    case RehabObjectiveType.FlexExtend:
                        UpdateFlexExtend();
                        break;
                    case RehabObjectiveType.GraspHold:
                        UpdateGraspHold(deltaTime);
                        break;
                    case RehabObjectiveType.FingerSequence:
                        UpdateFingerSequence();
                        break;
                }
            }

            private void UpdateFlexExtend()
            {
                var average = m_handSensor.GetAveragePinchStrength();
                bool isOpen = average <= m_definition.OpenThreshold;
                bool isClosed = average >= m_definition.ClosedThreshold;

                if (!m_wasOpen && isOpen)
                {
                    m_wasOpen = true;
                }

                if (m_wasOpen && !m_wasClosed && isClosed)
                {
                    m_wasClosed = true;
                }

                if (m_wasOpen && m_wasClosed && isOpen)
                {
                    CompletedRepetitions++;
                    m_wasOpen = false;
                    m_wasClosed = false;
                }
            }

            private void UpdateGraspHold(float deltaTime)
            {
                float average = m_handSensor.GetAveragePinchStrength();
                if (average >= m_definition.PinchThreshold && !m_waitingForRelease)
                {
                    m_holdTimer += deltaTime;
                    if (m_holdTimer >= m_definition.HoldSeconds)
                    {
                        CompletedRepetitions++;
                        m_holdTimer = 0f;
                        m_waitingForRelease = true;
                    }
                }
                else
                {
                    if (average <= m_definition.OpenThreshold)
                    {
                        m_waitingForRelease = false;
                    }
                    m_holdTimer = 0f;
                }
            }

            private void UpdateFingerSequence()
            {
                if (m_definition.SequenceFingers == null || m_definition.SequenceFingers.Count == 0)
                {
                    return;
                }

                var finger = m_definition.SequenceFingers[m_sequenceIndex];
                bool isPinching = m_handSensor.IsFingerPinching(finger, m_definition.PinchThreshold);

                if (!m_wasPinching && isPinching)
                {
                    m_sequenceIndex++;
                    m_wasPinching = true;
                }

                if (m_wasPinching && !isPinching)
                {
                    m_wasPinching = false;
                }

                if (m_sequenceIndex >= m_definition.SequenceFingers.Count)
                {
                    CompletedRepetitions++;
                    m_sequenceIndex = 0;
                    m_wasPinching = false;
                }
            }
        }
    }
}
