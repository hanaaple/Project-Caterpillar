using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Core;
using Utility.Dialogue;
using Utility.JsonLoader;
using Utility.Player;
using Utility.SaveSystem;
using Utility.Scene;

namespace Utility.Interaction
{
    public abstract class Interaction : MonoBehaviour
    {
        [Header("동일한 Scene 내의 Interaction 중 유니크 Value를 넣으세요.")]
        public string id;

        public InteractionData[] interactionData;

        [SerializeField] protected bool isOnLoadScene;

        [SerializeField] protected bool isOnAwake;

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

            if (isOnAwake)
            {
                StartInteraction();
            }
            else if (isOnLoadScene)
            {
                SceneLoader.Instance.onLoadSceneEnd += () => { StartInteraction(); };
            }
        }

        protected virtual void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
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

                targetData.onEndAction += () =>
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

            PlayUIManager.Instance.quickSlotManager.SetQuickSlot(false);

            Debug.Log($"Start Interaction 이름: {gameObject.name}");

            var interaction = GetInteractionData(index);

            if (interaction.isMove)
            {
                StartCoroutine(MoveTo(index));
            }
            else
            {
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
                        var join = interaction.itemInteractionTypes.Join(
                            ItemManager.Instance.GetItem<ItemManager.ItemType>(),
                            inner => inner.itemType,
                            item => item, (inner, item) => inner.itemType).ToArray();
                        if (join.Length == 1)
                        {
                            var targetItemInteraction = Array.Find(interaction.itemInteractionTypes,
                                item => item.itemType == join[0]);
                            StartInteraction(targetItemInteraction.targetIndex);
                        }
                        else
                        {
                            Debug.LogWarning($"아이템 조심\n" +
                                             $"{ItemManager.Instance.GetItem<string>().Select(item => $"{item}, ")}\n" +
                                             $"{interaction.itemInteractionTypes.Select(item => $"{item}, ")}");
                            var targetItemInteraction = Array.Find(interaction.itemInteractionTypes,
                                item => item.itemType == ItemManager.ItemType.None);
                            StartInteraction(targetItemInteraction.targetIndex);
                        }

                        break;
                    case InteractType.Tutorial:
                    {
                        PlayUIManager.Instance.tutorialManager.StartTutorial(interaction.tutorialHelper, () =>
                        {
                            EndInteraction();
                        });
                        break;
                    }
                }
            }
        }

        protected virtual void EndInteraction(int index = -1, int nextIndex = -1)
        {
            Debug.LogWarning("왜 실행 안해");
            if (index == -1)
            {
                index = interactionIndex;
            }

            Debug.Log($"{gameObject.name} 인터랙션 종료");

            var interaction = GetInteractionData(index);
            var data = GetInteractionData(index).serializedInteractionData;
            data.isInteracted = true;

            if (nextIndex == -1)
            {
                nextIndex = (index + 1) % interactionData.Length;
            }

            var nextInteraction = GetInteractionData(nextIndex).serializedInteractionData;

            // var collider2d = GetComponent<Collider2D>();
            //collider2d.enabled = false;

            if (data.isNextInteractable)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
                // collider2d.enabled = true;
            }

            if (data.isLoop)
            {
                // collider2d.enabled = true;
            }

            if (data.interactNextIndex)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;

                StartInteraction(nextIndex);
            }

            interaction.onEndAction?.Invoke();
            interaction.onEndAction = () => { };
            OnEndInteraction?.Invoke();
            OnEndInteraction = () => { };
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

            var interaction = interactionData[index].serializedInteractionData;
            return (interaction.isLoop || !interaction.isInteracted) && interaction.isInteractable;
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
                id = id,
                interactionIndex = interactionIndex,
                serializedInteractionData = new List<SerializedInteractionData>()
            };
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];
                interaction.serializedInteractionData.id = index;
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

                for (var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];

                    switch (dialogueElement.dialogueType)
                    {
                        case DialogueType.MiniGame or DialogueType.WaitInteract or DialogueType.MoveMap:
                        {
                            Debug.LogWarning(
                                $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함. {dialogueElement.playableAsset}");
                            break;
                        }

                        case DialogueType.CutScene:
                        {
                            var timelineAsset = (TimelineAsset) dialogueElement.playableAsset;

                            if (timelineAsset != null)
                            {
                                var tracks = timelineAsset.GetOutputTracks();
                                foreach (var temp in tracks.Where(item => item is AnimationTrack))
                                {
                                    // Debug.Log(temp.name);
                                }
                            }

                            if (dialogueElement.option is {Length: > 0})
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

                    // if (dialogueElement.dialogueType == DialogueType.WaitInteract && idx != interaction.dialogueData.dialogueElements.Length - 1)
                    // {
                    //     var newArray = new InteractionData[interactionData.Length + 1];
                    //     // 0 ~ index - 1
                    //     // index
                    //     // index ~ interactionData.Length - 1   ->    index + 1 ~ newArray.Length - 1
                    //
                    //     
                    //     // 0 ~ index까지 앞에 저장
                    //     // index + 1 ~ interactionData.Length 까지 앞에 저장
                    //     
                    //     var interactionDataIndex = index + 1;
                    //     
                    //     Array.Copy(interactionData, 0, newArray, 0, interactionDataIndex);
                    //     Array.Copy(interactionData, interactionDataIndex, newArray, interactionDataIndex + 1, interactionData.Length - interactionDataIndex);
                    //     
                    //     // newArray[index].dialogueData.dialogueElements =
                    //     
                    //     newArray[index].
                    //     
                    //     newArray[index + 1] = newArray[index].DeepCopy();
                    //     newArray[index + 1].jsonAsset = null;
                    //     
                    //     newArray[index + 1].dialogueData
                    //     
                    //     newArray[index].dialogueData.
                    //     
                    //     Debug.Log(newArray.Select(item => $"{item.interactType}, {item.dialogueData.dialogueElements.Length}"));
                    //     // dialogueElement[idx + 1] ~ dialogueElement[interaction.dialogueData.dialogueElement.Length - 1]
                    //         
                    //     index++;
                    // }

                    switch (dialogueElement.dialogueType)
                    {
                        case DialogueType.MiniGame or DialogueType.WaitInteract or DialogueType.MoveMap:
                        {
                            Debug.LogWarning(
                                $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                            break;
                        }

                        case DialogueType.CutScene:
                        {
                            if (dialogueElement.option is {Length: > 0})
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
                    }
                }
            }
        }
#endif
    }
}