// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    [CreateAssetMenu(fileName = "RehabLevel", menuName = "NorthStar/Rehab Level")]
    public class RehabLevelDefinition : ScriptableObject
    {
        public string LevelTitle;
        [TextArea(2, 6)] public string StoryIntro;
        [TextArea(2, 6)] public string StoryOutro;
        public List<RehabObjectiveDefinition> Objectives = new();
    }
}
