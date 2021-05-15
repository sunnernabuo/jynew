﻿using System;
using Animancer;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jyx2
{
    /// <summary>
    /// 动画播放者
    /// </summary>
    public abstract class Jyx2AnimationBattleRole : MonoBehaviour
    {
        public abstract Animator GetAnimator();

        private HybridAnimancerComponent _animancer;
        public HybridAnimancerComponent GetAnimancer()
        {
            if (_animancer == null)
            {
                var animator = GetAnimator();
                _animancer = GameUtil.GetOrAddComponent<HybridAnimancerComponent>(animator.transform);
                _animancer.Animator = animator;
                _animancer.Controller = animator.runtimeAnimatorController;
            }
            return _animancer;
        }
        
        /// <summary>
        /// 当前的技能播放
        /// </summary>
        public Jyx2SkillDisplay CurDisplay { get; set; }

        bool IsStandardModelAvata()
        {
            var animator = GetAnimator();
            var controller = animator.runtimeAnimatorController;
            return controller.name == "jyx2humanoidController.controller";
        }
        
        public virtual void Idle()
        {
            string code = CurDisplay.IdleAnim;
            if (!PlayScriptAnimation(code))
            {
                var animator = GetAnimator();
                if (animator != null)
                {
                    animator.SetBool("InBattle", true);
                    animator.SetFloat("PosCode", float.Parse(code));
                    animator.SetTrigger("battle_idle");    
                }
            }
        }
        
        public virtual void BeHit()
        {
            string code = CurDisplay.GetBeHitAnimationCode();
            if (!PlayScriptAnimation(code, Idle, 0.25f))
            {
                var animator = GetAnimator();
                if (animator != null)
                {
                    animator.SetTrigger("hit");
                }
            }
        }

        public virtual void Attack()
        {
            string code = CurDisplay.AttackAnim;
            if (!PlayScriptAnimation(code, Idle, 0.25f))
            {
                var animator = GetAnimator();
                if (animator != null)
                {
                    animator.SetFloat("AttackCode", float.Parse(code));
                    animator.SetTrigger("attack");
                }
            }
        }

        public virtual void Run()
        {
            string code = CurDisplay.RunAnim; //TODO
            if (!PlayScriptAnimation(code))
            {
                var animator = GetAnimator();
                if (animator != null)
                {
                    animator.SetBool("InBattle", true);
                    animator.SetFloat("PosCode", float.Parse(code));
                    animator.ResetTrigger("battle_idle");
                    animator.SetTrigger("move");
                }
            }
        }

        public virtual void ShowDamage()
        {
            //DONOTHING
        }

        private bool PlayScriptAnimation(string animCode, Action callback = null, float fadeDuration = 0f)
        {
            var animancer = GetAnimancer();
            
            if (animCode.StartsWith("@"))
            {
                string path = animCode.TrimStart('@');
                //load and play AnimationClip
                Addressables.LoadAssetAsync<AnimationClip>(path).Completed += r =>
                {
                    var state = animancer.Play(r.Result);

                    
                    //callback if needed
                    if (callback != null)
                    {
                        if (fadeDuration > 0)
                        {
                            GameUtil.CallWithDelay(state.Duration - fadeDuration, callback);
                        }
                        else
                        {
                            state.Events.OnEnd = () =>
                            {
                                state.Stop();
                                callback();
                            };    
                        }
                    }
                };
                return true;
            }
            else
            {
                //animancer.Stop(); //force switch to AnimationController
                animancer.PlayController(); //fade to AnimationController
                return false;
            }   
        }
    }
}
    
