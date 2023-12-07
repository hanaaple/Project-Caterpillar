using System;
using UnityEngine;
using Utility.Audio;
using Utility.Core;
using Utility.Player;

namespace Utility.Portal
{
    public class PortalManager : MonoBehaviour
    {
        private enum PortalDirection
        {
            Left,
            Right
        }

        [SerializeField] private Portal leftPortal;
        [SerializeField] private Portal rightPortal;

        [SerializeField] private Portal[] leftPortals;
        [SerializeField] private Portal[] rightPortals;

        [SerializeField] private Transform leftTarget;
        [SerializeField] private Transform rightTarget;

        [SerializeField] private GameObject[] maps;

        [SerializeField] private AudioClip portalAudioClip;

        private int _curIndex;

        // Map의 종류 None, Loop
        // Loop인데 맵 내 Object는 바꿔줘야됨.
        // 어떻게 하는게 좋으려나
        // 흠흠

        private void Start()
        {
            leftPortal.onTryTeleport = () =>
            {
                if (!IsWaitClear(PortalDirection.Left, out var targetPortal))
                {
                    targetPortal.StartInteraction();
                }
                else if (IsTeleportEnable(PortalDirection.Left))
                {
                    Teleport(PortalDirection.Left);
                }
            };

            rightPortal.onTryTeleport = () =>
            {
                if (!IsWaitClear(PortalDirection.Right, out var targetPortal))
                {
                    targetPortal.StartInteraction();
                }
                else if (IsTeleportEnable(PortalDirection.Right))
                {
                    Teleport(PortalDirection.Right);
                }
            };

            UpdatePortalIndex();
        }

        private bool IsWaitClear(PortalDirection portalDirection, out Portal targetPortal)
        {
            targetPortal = GetPortal(_curIndex, portalDirection);

            Debug.LogWarning($"{targetPortal.gameObject},  {targetPortal.portalIndex},  {targetPortal.IsWaitClear()}");
            
            return targetPortal.IsWaitClear();
        }

        private bool IsTeleportEnable(PortalDirection portalDirection)
        {
            if (PlayUIManager.Instance.IsFade())
            {
                return false;
            }

            switch (portalDirection)
            {
                case PortalDirection.Right when _curIndex == maps.Length - 1:
                case PortalDirection.Left when _curIndex == 0:
                    return false;
                default:
                    return true;
            }
        }

        private void Teleport(PortalDirection portalDirection)
        {
            var player = PlayerManager.Instance.Player;
            player.IsCharacterControllable = false;
            player.SetCharacterAnimator(false);

            AudioManager.Instance.PlaySfx(portalAudioClip);

            var targetPos = portalDirection switch
            {
                PortalDirection.Left => rightTarget,
                PortalDirection.Right => leftTarget,
                _ => null
            };
            
            var targetPortal = GetPortal(_curIndex, portalDirection);

            Debug.LogWarning($"portal {_curIndex} -> {GetNextMapIndex(portalDirection)},  {targetPortal} {targetPortal.TeleportEndIsFadeOut}");

            PlayUIManager.Instance.FadeOut(() =>
            {
                maps[_curIndex].SetActive(false);
                
                _curIndex = GetNextMapIndex(portalDirection);

                UpdatePortalIndex();

                maps[_curIndex].SetActive(true);
                player.transform.localScale = targetPos.localScale;
                player.transform.position = targetPos.position;

                if (targetPortal.TeleportEndIsFadeOut)
                {
                    // 페이드 인 실행시키는게 필요한데 알아서 하라 혀
                    targetPortal.onEndTeleport?.Invoke();

                    player.IsCharacterControllable = true;
                }
                else
                {
                    PlayUIManager.Instance.FadeIn(() =>
                    {
                        targetPortal.onEndTeleport?.Invoke();

                        player.IsCharacterControllable = true;
                    });
                }
            });
        }

        private int GetNextMapIndex(PortalDirection portalDirection)
        {
            var targetMapIndex = portalDirection switch
            {
                PortalDirection.Right => (_curIndex + 1) % maps.Length,
                PortalDirection.Left => (_curIndex + maps.Length - 1) % maps.Length,
                _ => -1
            };

            return targetMapIndex;
        }

        private Portal GetPortal(int index, PortalDirection portalDirection)
        {
            Portal targetPortal;
            switch (portalDirection)
            {
                case PortalDirection.Left:
                {
                    var t = Array.Find(leftPortals, item => item.portalIndex == index);
                    targetPortal = t ? t : leftPortal;
                    break;
                }
                case PortalDirection.Right:
                {
                    var t = Array.Find(rightPortals, item => item.portalIndex == index);
                    targetPortal = t ? t : rightPortal;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(portalDirection), portalDirection, null);
            }

            return targetPortal;
        }

        private void UpdatePortalIndex()
        {
            leftPortal.CurMapIndex = _curIndex;
            rightPortal.CurMapIndex = _curIndex;
            
            foreach (var portal in leftPortals)
            {
                portal.CurMapIndex = _curIndex;
            }
            
            foreach (var portal in rightPortals)
            {
                portal.CurMapIndex = _curIndex;
            }
        }
    }
}