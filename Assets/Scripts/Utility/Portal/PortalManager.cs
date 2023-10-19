using System;
using UnityEngine;
using Utility.Audio;
using Utility.Core;
using Utility.Player;

namespace Utility.Portal
{
    public class PortalManager : MonoBehaviour
    {
        // [Serializable]
        // private class Map
        // {
        //     public GameObject map;
        // }

        [SerializeField] private Portal leftPortal;
        [SerializeField] private Portal rightPortal;
        
        [SerializeField] private Portal[] leftPortals;
        [SerializeField] private Portal[] rightPortals;

        [SerializeField] private Transform leftTarget;
        [SerializeField] private Transform rightTarget;

        [SerializeField] private GameObject[] maps;
        
        [SerializeField] private AudioClip portalAudioClip;

        private int _index;

        // Map의 종류 None, Loop
        // Loop인데 맵 내 Object는 바꿔줘야됨.
        // 어떻게 하는게 좋으려나
        // 흠흠

        private void Start()
        {
            leftPortal.onPortal = () =>
            {
                if (IsInteraction(false, out var targetPortal))
                {
                    targetPortal.StartInteraction();
                }
                else if (IsTeleportEnable(false))
                {
                    Teleport(false);
                }
            };

            rightPortal.onPortal = () =>
            {
                if (IsInteraction(true, out var targetPortal))
                {
                    targetPortal.StartInteraction();
                }
                else if (IsTeleportEnable(true))
                {
                    Teleport(true);
                }
            };
        }

        private void SetActive(bool isActive)
        {
            maps[_index].SetActive(isActive);
        }

        private bool IsInteraction(bool isPositive, out Portal targetPortal)
        {
            if (isPositive)
            {
                var t = Array.Find(rightPortals, item => item.portalIndex == _index);
                targetPortal = t ? t : rightPortal;
            }
            else
            {
                var t = Array.Find(leftPortals, item => item.portalIndex == _index);
                targetPortal = t ? t : leftPortal;
            }
            Debug.LogWarning(targetPortal.gameObject + "   " + !targetPortal.IsWaitClear());
            return !targetPortal.IsWaitClear();
        }

        private bool IsTeleportEnable(bool isPositive)
        {
            if (PlayUIManager.Instance.IsFade())
            {
                return false;
            }

            if (isPositive)
            {
                if (_index == maps.Length - 1)
                {
                    return false;
                }
            }
            else
            {
                if (_index == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void Teleport(bool isPositive)
        {
            var player = PlayerManager.Instance.Player;
            player.IsCharacterControllable = false;
            player.SetCharacterAnimator(false);

            AudioManager.Instance.PlaySfx(portalAudioClip);

            PlayUIManager.Instance.FadeOut(() =>
            {
                SetActive(false);
                Transform target;
                Portal targetPortal;
                if (isPositive)
                {
                    var t = Array.Find(rightPortals, item => item.portalIndex == _index);
                    if (t)
                    {
                        targetPortal = t;
                    }
                    else
                    {
                        targetPortal = rightPortal;
                    }

                    target = leftTarget;
                    _index = (_index + 1) % maps.Length;
                }
                else
                {
                    var t = Array.Find(leftPortals, item => item.portalIndex == _index);
                    if (t)
                    {
                        targetPortal = t;
                    }
                    else
                    {
                        targetPortal = leftPortal;
                    }

                    target = rightTarget;
                    _index = (_index + maps.Length - 1) % maps.Length;
                }
                
                leftPortal.MapIndex = _index;
                rightPortal.MapIndex = _index;
                targetPortal.MapIndex = _index;
                SetActive(true);

                player.transform.localScale = target.localScale;
                player.transform.position = target.position;
                PlayUIManager.Instance.FadeIn(() =>
                {
                    targetPortal.onEndTeleport?.Invoke();
                    
                    player.IsCharacterControllable = true;
                });
            });
        }
    }
}