using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Audio;
using Utility.Core;
using Utility.Dialogue;
using Utility.JsonLoader;
using Utility.Player;
using Utility.SaveSystem;

namespace Utility.Interaction
{
    public abstract class Interaction : MonoBehaviour
    {
        [Header("동일한 Scene 내의 Interaction 중 유니크 Value를 넣으세요.")]
        public string id;

        public InteractionData[] interactionData;

        // for debugging
        [SerializeField] public int interactionIndex;

        protected Action OnEndInteraction;
        private static readonly int State = Animator.StringToHash("State");


        private void OnValidate()
        {
            if (interactionData == null)
            {
                return;
            }

            var dataArray = interactionData.Where(item => item.interactType != InteractType.Dialogue).ToArray();
            foreach (var data in dataArray)
            {
                data.dialogueData = null;
                data.jsonAsset = null;
                data.animator = null;
                data.state = 0;
            }
        }

        protected virtual void Awake()
        {
            GameManager.Instance.AddInteraction(this);

            UpdateId();
        }

        protected virtual void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        public void UpdateId()
        {
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];
                interaction.serializedInteractionData.id = index;
            }
        }

        public void OnAwakeInteraction()
        {
            Debug.LogWarning(interactionIndex);
            Debug.LogWarning(interactionData.Length);
            Debug.LogWarning(gameObject.activeSelf);
            Debug.LogWarning(interactionIndex >= interactionData.Length || !gameObject.activeSelf);
            if (interactionIndex >= interactionData.Length || !gameObject.activeSelf)
            {
                return;
            }

            var data = interactionData[interactionIndex];
            if (data.isOnAwake)
            {
                Debug.Log($"OnAwake - {gameObject.name}, Index - {interactionIndex}, Order - {data.order}");
                StartInteraction();
            }
        }

        public void InitializeWait(WaitInteraction waitInteraction, Action onClearAction)
        {
            waitInteraction.isWaitClear = false;

            if (waitInteraction.isInteraction)
            {
                interactionIndex = waitInteraction.startIndex;


                var data = GetInteractionData();
                data.serializedInteractionData.isInteracted = false;

                if (!waitInteraction.isCustom)
                {
                    data.serializedInteractionData.isInteractable = true;
                }

                var targetData = GetInteractionData(waitInteraction.targetIndex);
                targetData.serializedInteractionData.isInteracted = false;

                targetData.OnEndAction += () =>
                {
                    waitInteraction.Clear();

                    onClearAction?.Invoke();
                };
            }
            else if (waitInteraction.isPortal)
            {
                var portal = waitInteraction.interaction as Portal.Portal;
                if (!portal)
                {
                    Debug.LogError("Wait Interactions - Portal 오류");
                }

                portal.onEndTeleport += () =>
                {
                    // 나중에 코드 수정해야됨
                    // 이거 말고
                    // waitInteraction의 포탈의 TargetIndex가 portal이 이동한 결과인지
                    if (waitInteraction.targetMapIndex != portal.MapIndex)
                    {
                        return;
                    }

                    waitInteraction.Clear();

                    onClearAction?.Invoke();
                    onClearAction = () => { };
                };
            }

            Debug.Log(
                $"인터랙션 대기 초기화, Object: {gameObject} Start Index: {waitInteraction.startIndex}, Target Index: {waitInteraction.targetIndex}");
        }

        public virtual void StartInteraction(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            if (!IsInteractable(index))
            {
                return;
            }

            var interaction = GetInteractionData(index);

            Debug.Log($"Start Interaction 이름: {gameObject.name} {interaction.interactType}");

            if (interaction.serializedInteractionData.isPauseBgm)
            {
                AudioManager.Instance.ReduceBgmVolume();
            }

            if (interaction.isMove)
            {
                StartCoroutine(MoveTo(index));
            }
            else
            {
                switch (interaction.interactType)
                {
                    case InteractType.Dialogue:
                    {
                        PlayUIManager.Instance.quickSlotManager.SetQuickSlot(false);
                        if (interaction.dialogueData.dialogueElements.Length == 0)
                        {
                            PlayUIManager.Instance.dialogueController.StartDialogue(interaction.jsonAsset.text,
                                nextIndex => { EndInteraction(index, nextIndex); });
                            Debug.LogWarning("CutScene, Wait 세팅 안되어있을수도 주의");
                        }
                        else
                        {
                            interaction.dialogueData.OnDialogueEnd = nextIndex => { EndInteraction(index, nextIndex); };
                            PlayUIManager.Instance.dialogueController.StartDialogue(interaction.dialogueData);
                        }

                        break;
                    }
                    case InteractType.Animator:
                    {
                        if (!interaction.animator)
                        {
                            Debug.LogWarning("Animator가 없음");
                        }
                        else
                        {
                            interaction.animator.SetInteger(State, interaction.state);
                            EndInteraction();
                        }

                        break;
                    }
                    case InteractType.OneOff:
                    {
                        EndInteraction(index);
                        break;
                    }
                    case InteractType.Item:
                    {

                        // Item List가 있고 없고 2가지 경우만 따짐

                        // interaction.itemInteractionType.itemTypes
                        // ItemManager.Instance.GetItem<ItemManager.ItemType>()
                        // 중복 비교 
                        // 중복 개수

                        var hold = interaction.itemInteractionData.itemData
                            .Where(item => item.itemUseType == ItemUseType.HoldHand);

                        var holdItemData = hold as ItemData[] ?? hold.ToArray();
                        if (holdItemData.Any())
                        {
                            if (holdItemData.All(item => item.itemType == ItemManager.Instance.GetHoldingItem()))
                            {
                                var items = ItemManager.Instance.GetItem<ItemManager.ItemType>();

                                // 보유 중인 아이템 중복 제외
                                var count = interaction.itemInteractionData.itemData
                                    .Where(item => item.itemUseType == ItemUseType.Possess)
                                    .Select(item => item.itemType)
                                    .Except(items).Count();
                                if (count == 0)
                                {
                                    // 성공
                                    Debug.Log("아이템 전부 있음");
                                    foreach (var itemData in interaction.itemInteractionData.itemData)
                                    {
                                        if (itemData.isDestroyItem)
                                        {
                                            Debug.Log($"아이템 제거 - {itemData.itemType}");
                                            ItemManager.Instance.RemoveItem(itemData.itemType);
                                        }
                                    }

                                    StartInteraction(interaction.itemInteractionData.targetIndex);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var items = ItemManager.Instance.GetItem<ItemManager.ItemType>();

                            // 보유 중인 아이템 중복 제외
                            var count = interaction.itemInteractionData.itemData
                                .Where(item => item.itemUseType == ItemUseType.Possess).Select(item => item.itemType)
                                .Except(items).Count();
                            if (count == 0)
                            {
                                // 성공
                                Debug.Log("아이템 전부 있음");
                                foreach (var itemData in interaction.itemInteractionData.itemData)
                                {
                                    if (itemData.isDestroyItem)
                                    {
                                        Debug.Log($"아이템 제거 - {itemData.itemType}");
                                        ItemManager.Instance.RemoveItem(itemData.itemType);
                                    }
                                }

                                StartInteraction(interaction.itemInteractionData.targetIndex);
                                break;
                            }
                        }

                        // 아이템 없음
                        Debug.Log(
                            $"아이템 목록 - {string.Join(", ", interaction.itemInteractionData.itemData.Select(item => $"{item.itemType}, {item.itemUseType}"))}");

                        // isLoopDefault 에 따라 변경
                        // isNextInteract인 경우 -> 알아서 실행이 되다가 돌아와? -> 다 실행되고 돌아와야됨
                        var tmpInteractionIndex = interactionIndex;
                        if (interaction.itemInteractionData.isLoopDefault)
                        {
                            interaction.OnCompletelyEndAction += () => { interactionIndex = tmpInteractionIndex; };
                        }

                        StartInteraction(interaction.itemInteractionData.defaultInteractionIndex);

                        break;
                }
                    case InteractType.Tutorial:
                    {
                        PlayUIManager.Instance.quickSlotManager.SetQuickSlot(false);
                        PlayUIManager.Instance.tutorialManager.StartTutorial(interaction.tutorialHelper,
                            () => { EndInteraction(); });
                        break;
                    }
                    case InteractType.Audio:
                    {
                        if (interaction.isBgm)
                        {
                            if (interaction.isAudioClip)
                            {
                                AudioManager.Instance.PlayBgmWithFade(interaction.audioClip);
                            }
                            else if (interaction.isTimelineAudio)
                            {
                                AudioManager.Instance.PlayBgmWithFade(interaction.audioTimeline);
                            }
                            else
                            {
                                Debug.LogError("오디오 세팅 오류 - Clip, Timeline 구분");
                            }
                        }
                        else if (interaction.isSfx)
                        {
                            if (interaction.isAudioClip)
                            {
                                AudioManager.Instance.PlaySfx(interaction.audioClip);
                            }
                            else if (interaction.isTimelineAudio)
                            {
                                AudioManager.Instance.PlaySfx(interaction.audioTimeline);
                            }
                            else
                            {
                                Debug.LogError("오디오 세팅 오류 - Clip, Timeline 구분");
                            }
                        }

                        EndInteraction();
                        break;
                    }
                }
            }
        }

        protected virtual void EndInteraction(int index = -1, int nextIndex = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            var interaction = GetInteractionData(index);
            var data = GetInteractionData(index).serializedInteractionData;
            data.isInteracted = true;

            Debug.Log($"{gameObject.name} 종료 인터랙션 - {index}, {nextIndex}");

            if (nextIndex == -1)
            {
                nextIndex = (index + 1) % interactionData.Length;
            }

            Debug.Log($"목표 -> {nextIndex}");

            var nextInteraction = GetInteractionData(nextIndex).serializedInteractionData;

            if (data.isNextInteractable)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
            }

            if (data.interactNextIndex)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
            }

            if (!data.interactNextIndex)
            {
                AudioManager.Instance.ReturnBgmVolume();
            }

            interaction.OnEndAction?.Invoke();
            interaction.OnEndAction = () => { };
            OnEndInteraction?.Invoke();
            OnEndInteraction = () => { };

            if (data.interactNextIndex)
            {
                StartInteraction(nextIndex);
            }
            else
            {
                interaction.OnCompletelyEndAction.Invoke();
                interaction.OnCompletelyEndAction = () => { };
            }
        }

        private IEnumerator MoveTo(int index)
        {
            var interaction = GetInteractionData(index);
            var player = PlayerManager.Instance.Player;
            var startPos = player.transform.position;
            var targetPos = interaction.targetTransform.position;

            var origin = player.IsCharacterControllable;
            player.IsCharacterControllable = false;
            var t = 0f;

            player.SetCharacterAnimator(true);
            while (t < 1)
            {
                player.RotateCharacter(targetPos.x - player.transform.position.x > 0 ? Vector2.right : Vector2.left);
                player.transform.position = Vector3.Lerp(startPos, targetPos, t);
                t += Time.deltaTime * interaction.moveSpeed;
                yield return null;
            }

            player.transform.position = targetPos;
            player.IsCharacterControllable = origin;

            player.SetCharacterAnimator(false);
            player.SetScale(interaction.targetTransform.localScale);


            switch (interaction.interactType)
            {
                case InteractType.Dialogue:
                    if (interaction.dialogueData.dialogueElements.Length == 0)
                    {
                        PlayUIManager.Instance.dialogueController.StartDialogue(interaction.jsonAsset.text,
                            nextIndex => { EndInteraction(index, nextIndex); });
                        Debug.LogWarning("CutScene, Wait 세팅 안되어있을수도 주의");
                    }
                    else
                    {
                        interaction.dialogueData.OnDialogueEnd = nextIndex => { EndInteraction(index, nextIndex); };
                        PlayUIManager.Instance.dialogueController.StartDialogue(interaction.dialogueData);
                    }

                    break;

                case InteractType.Animator:
                    if (!interaction.animator)
                    {
                        Debug.LogWarning("Animator가 없음");
                    }
                    else
                    {
                        interaction.animator.SetInteger(State, interaction.state);
                        EndInteraction();
                    }

                    break;
                case InteractType.OneOff:
                    EndInteraction(index);
                    break;
                case InteractType.Item:
                    break;
            }
        }

        protected virtual bool IsInteractable(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            if (index >= interactionData.Length)
            {
                return false;
            }

            var interaction = interactionData[index].serializedInteractionData;
            return (interaction.isLoop || !interaction.isInteracted) && interaction.isInteractable &&
                   gameObject.activeSelf;
        }

        public InteractionData GetInteractionData(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            return interactionData[index];
        }

        public InteractionSaveData GetInteractionSaveData()
        {
            var interactionSaveData = new InteractionSaveData
            {
                name = gameObject.name,
                id = id,
                interactionIndex = interactionIndex,
                serializedInteractionData = new List<SerializedInteractionData>()
            };
            foreach (var interaction in interactionData)
            {
                interactionSaveData.serializedInteractionData.Add(interaction.serializedInteractionData);
            }

            return interactionSaveData;
        }

#if UNITY_EDITOR
        public void DebugInteractionData()
        {
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];

                if (interaction.dialogueData?.dialogueElements == null ||
                    interaction.dialogueData.dialogueElements.Length == 0)
                {
                    continue;
                }

                for (var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];

                    switch (dialogueElement.dialogueType)
                    {
                        case DialogueType.MiniGame or DialogueType.WaitInteract or DialogueType.MoveMap
                            or DialogueType.DialogueEnd:
                        {
                            Debug.LogWarning(
                                $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함. {dialogueElement.playableAsset}");
                            break;
                        }

                        case DialogueType.CutScene:
                        {
                            var timelineAsset = (TimelineAsset)dialogueElement.playableAsset;

                            if (timelineAsset != null)
                            {
                                var tracks = timelineAsset.GetOutputTracks();
                                foreach (var temp in tracks.Where(item => item is AnimationTrack))
                                {
                                    // Debug.Log(temp.name);
                                }
                            }

                            if (dialogueElement.option is { Length: > 0 })
                            {
                                if (dialogueElement.option.Contains("name=", StringComparer.OrdinalIgnoreCase))
                                {
                                    var timelinePath = Array
                                        .Find(dialogueElement.option, item => item.Contains("name="))
                                        .Split("=")[1];
                                    var playableAsset = Resources.Load<PlayableAsset>($"Timeline/{timelinePath}");
                                    if (!playableAsset)
                                    {
                                        Debug.LogWarning(
                                            $"interaction: {index}번, {idx}번 대화, timeline - {timelinePath} 없음, {dialogueElement.dialogueType} 세팅해야함.");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning(
                                        $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함. {dialogueElement.playableAsset}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning(
                                    $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함. {dialogueElement.playableAsset}");
                            }

                            break;
                        }
                        case DialogueType.Audio:
                        {
                            Debug.LogWarning(
                                $"{index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함. {dialogueElement.audioClip}, {dialogueElement.audioTimeline}");
                            break;
                        }
                    }
                }
            }
        }

        public void SetDialogue()
        {
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];

                if (interaction.interactType != InteractType.Dialogue || !interaction.jsonAsset)
                {
                    continue;
                }

                interaction.dialogueData.dialogueElements =
                    JsonHelper.GetJsonArray<DialogueElement>(interaction.jsonAsset.text);

                // Wait Interaction이 있는 경우, 나누기

                for (var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];

                    switch (dialogueElement.dialogueType)
                    {
                        case DialogueType.MiniGame or DialogueType.WaitInteract or DialogueType.MoveMap
                            or DialogueType.DialogueEnd:
                        {
                            Debug.LogWarning(
                                $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                            break;
                        }

                        case DialogueType.CutScene:
                        {
                            if (dialogueElement.option is { Length: > 0 })
                            {
                                if (dialogueElement.option.Contains("Reset"))
                                {
                                    interaction.dialogueData.dialogueElements[idx].playableAsset =
                                        Resources.Load<PlayableAsset>("Timeline/Reset");

                                    interaction.dialogueData.dialogueElements[idx].waitSec = -1;
                                    continue;
                                }

                                interaction.dialogueData.dialogueElements[idx].extrapolationMode =
                                    dialogueElement.option.Contains("Hold", StringComparer.OrdinalIgnoreCase)
                                        ? DirectorWrapMode.Hold
                                        : DirectorWrapMode.None;


                                //  var digitOptions = Array.FindAll(dialogueElement.option,
                                //      item => item.Any(char.IsDigit));
                                // var floats = digitOptions.Select(float.Parse).ToArray();
                                var floats = dialogueElement.option.Where(item => float.TryParse(item, out _))
                                    .Select(float.Parse).ToArray();
                                interaction.dialogueData.dialogueElements[idx].waitSec =
                                    floats.Length == 1 ? floats[0] : 0f;

                                if (dialogueElement.option.Any(item =>
                                        item.Contains("director=", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var directorName =
                                        Array.Find(dialogueElement.option, item => item.Contains("director="))
                                            .Split("=")[1];
                                    interaction.dialogueData.dialogueElements[idx].playableDirectorName = directorName;
                                }

                                if (dialogueElement.option.Any(item =>
                                        item.Contains("name=", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var timelinePath =
                                        Array.Find(dialogueElement.option, item => item.Contains("name="))
                                            .Split("=")[1];
                                    var playableAsset = Resources.Load<PlayableAsset>($"Timeline/{timelinePath}");
                                    if (playableAsset)
                                    {
                                        interaction.dialogueData.dialogueElements[idx].playableAsset = playableAsset;
                                    }
                                    else
                                    {
                                        Debug.LogWarning(
                                            $"interaction: {index}번, {idx}번 대화, timeline - {timelinePath} 없음, {dialogueElement.dialogueType} 세팅해야함.");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning(
                                        $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                                }

                            }
                            else
                            {
                                Debug.LogWarning(
                                    $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                            }

                            break;
                        }

                        case DialogueType.Audio:
                        {
                            Debug.LogWarning(
                                $"{index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함. {dialogueElement.audioClip}, {dialogueElement.audioTimeline}");
                            break;
                        }
                    }
                }
            }
        }
#endif
    }
}