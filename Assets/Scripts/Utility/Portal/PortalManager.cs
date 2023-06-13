using UnityEngine;
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

        [SerializeField] private Transform leftTarget;
        [SerializeField] private Transform rightTarget;

        [SerializeField] private GameObject[] maps;

        private int _index;

        // Map의 종류 None, Loop
        // Loop인데 맵 내 Object는 바꿔줘야됨.
        // 어떻게 하는게 좋으려나
        // 흠흠

        private void Start()
        {
            leftPortal.onPortal = () =>
            {
                if (IsTeleportEnable(false))
                {
                    Teleport(false);
                }
            };

            rightPortal.onPortal = () =>
            {
                if (IsTeleportEnable(true))
                {
                    Teleport(true);
                }
            };
        }

        private void SetActive(bool isActive)
        {
            maps[_index].SetActive(isActive);
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

            PlayUIManager.Instance.FadeOut(() =>
            {
                SetActive(false);
                Transform target;
                Portal targetPortal;
                if (isPositive)
                {
                    targetPortal = rightPortal;
                    target = leftTarget;
                    _index = (_index + 1) % maps.Length;
                }
                else
                {
                    targetPortal = leftPortal;
                    target = rightTarget;
                    _index = (_index + maps.Length - 1) % maps.Length;
                }

                leftPortal.MapIndex = _index;
                rightPortal.MapIndex = _index;
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