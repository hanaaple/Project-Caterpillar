using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Util
{
    public class AnimationSetter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string animationName;
        [SerializeField] private AnimationClip animationClip;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");

            var aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            var clip = Array.Find(aoc.animationClips, item => item.name == animationName);
            overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, animationClip));
            aoc.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = aoc;
        }
    }
}