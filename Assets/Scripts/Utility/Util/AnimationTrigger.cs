using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Util
{
    public class AnimationTrigger : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string animatorTrigger;
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            animator.SetTrigger(animatorTrigger);
        }
    }
}