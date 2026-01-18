# VR Hand Rehab Game (Meta Quest 2)

This guide outlines how to wire up the 3–4 level rehab experience focused on finger flex/extend and grasp for post-stroke recovery.

## Design Goals
- **Primary motions:** finger flex/extend, grasp hold, thumb-to-finger sequencing.
- **Session length:** 10–15 minutes with short objectives and fast feedback.
- **Narrative framing:** gentle, optimistic story beats to encourage adherence.

## Story Arc (Example 4 Levels)
1. **Lighthouse Warm-Up**
   - Story: You wake the lighthouse by opening and closing your hand to power it on.
   - Objectives: Flex/extend cycles.
2. **Garden Harvest**
   - Story: Pick glowing fruit to help a neighbor restore their garden.
   - Objectives: Grasp holds on objects.
3. **Workshop Repairs**
   - Story: Tighten bolts and restore machinery in the village workshop.
   - Objectives: Thumb-to-finger sequence (index → middle → ring → pinky).
4. **Harbor Celebration**
   - Story: Signal ships with controlled hand movements during the sunset ceremony.
   - Objectives: Mixed flex/extend + grasp holds.

## Unity Setup
1. **Scene Setup**
   - Add a `HandGestureSensor` component and assign the Quest 2 `OVRHand` reference.
   - Add a `RehabGameManager` component to an empty GameObject.
   - Create `RehabLevelDefinition` assets for each level (Create → NorthStar → Rehab Level).
   - Assign the level assets to `RehabGameManager`.

2. **Objective Configuration**
   - For each level, add 1–3 `RehabObjectiveDefinition` entries.
   - Recommended defaults:
     - **Flex/Extend:** TargetRepetitions 6–8, OpenThreshold 0.2, ClosedThreshold 0.7
     - **Grasp Hold:** TargetRepetitions 4–6, HoldSeconds 1.5, PinchThreshold 0.7
     - **Finger Sequence:** TargetRepetitions 3–5, PinchThreshold 0.65, Sequence (Index → Middle → Ring → Pinky)

3. **UI Hookup**
   - Bind `RehabGameManager` UnityEvents (`OnLevelTitle`, `OnStoryIntro`, `OnObjectiveName`, `OnObjectiveProgress`, `OnStoryOutro`).
   - Show current story beat and objective counters in VR.

## Notes
- These scripts rely on `OVRHand` pinch strength to approximate finger flexion.
- For higher fidelity, use `OVRSkeleton` bone angles to compute per-joint range of motion.
