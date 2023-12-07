using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Util
{
    public class AnimationTrigger : Trigger
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string animatorTrigger;
        [SerializeField] private string animationName;
        [SerializeField] private AnimationClip animationClip;
        
        protected override void Start()
        {
            base.Start();
            
            var aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            var clip = Array.Find(aoc.animationClips, item => item.name == animationName);
            overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, animationClip));
            aoc.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = aoc;
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
            
            if (other.isTrigger)
            {
                return;
            }
            
            animator.SetTrigger(animatorTrigger);
        }
    }
}