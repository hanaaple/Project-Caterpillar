using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Utility.Core;
using Utility.Dialogue;
using Utility.JsonLoader;
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

        // for debugging
        [SerializeField] protected int interactionIndex;

        [NonSerialized] public bool IsClear;

        private Action _onEndInteraction;
        private static readonly int State = Animator.StringToHash("State");

        protected virtual void Awake()
        {
            GameManager.Instance.AddInteraction(this);

            if (isOnLoadScene)
            {
                SceneLoader.Instance.onLoadSceneEnd += () => { StartInteraction(); };
            }
        }

        protected virtual void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        public void InitializeWait(Action onClearAction)
        {
            foreach (var interaction in interactionData)
            {
                interaction.serializedInteractionData.isInteracted = false;
            }

            IsClear = false;
            _onEndInteraction = onClearAction;
            GetComponent<Collider2D>().enabled = true;
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

            Debug.Log($"Start Interaction 이름: {gameObject.name}");

            var interaction = GetInteractionData(index);

            switch (interaction.interactType)
            {
                case InteractType.Dialogue:
                    if (interaction.dialogueData.dialogueElements.Length == 0)
                    {
                        DialogueController.Instance.StartDialogue(interaction.jsonAsset.text,
                            () => { EndInteraction(index); });
                        Debug.LogWarning("CutScene, Wait 세팅 안되어있을수도 주의");
                    }
                    else
                    {
                        interaction.dialogueData.OnDialogueEnd = () => { EndInteraction(index); };
                        DialogueController.Instance.StartDialogue(interaction.dialogueData);
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
            }
        }

        protected virtual void EndInteraction(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            Debug.Log($"{gameObject.name} 인터랙션 종료");

            var interaction = GetInteractionData(index).serializedInteractionData;
            interaction.isInteracted = true;
            // interaction.onInteractionEnd?.Invoke();

            var nextIndex = (index + 1) % interactionData.Length;
            var nextInteraction = GetInteractionData(nextIndex).serializedInteractionData;

            var collider2d = GetComponent<Collider2D>();
            collider2d.enabled = false;

            if (interaction.isContinuable)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
                collider2d.enabled = true;
            }

            if (interaction.isLoop)
            {
                collider2d.enabled = true;
            }

            if (interaction.interactNextIndex)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;

                StartInteraction(nextIndex);
            }

            if (IsInteractionClear())
            {
                IsClear = true;
            }

            _onEndInteraction?.Invoke();
        }

        protected virtual bool IsInteractionClear()
        {
            return interactionData.All(item => item.serializedInteractionData.isInteracted);
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

        private InteractionData GetInteractionData(int index = -1)
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
        public void Debugg()
        {
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];

                for (var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];

                    switch (dialogueElement.dialogueType)
                    {
                        case DialogueType.Interact or DialogueType.WaitInteract or DialogueType.MoveMap:
                        {
                            Debug.LogWarning(
                                $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                            break;
                        }

                        case DialogueType.CutScene:
                        {
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

        public void SetDialogue()
        {
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];
                interaction.dialogueData.dialogueElements =
                    JsonHelper.GetJsonArray<DialogueElement>(interaction.jsonAsset.text);

                for (var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];

                    switch (dialogueElement.dialogueType)
                    {
                        case DialogueType.Interact or DialogueType.WaitInteract or DialogueType.MoveMap:
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
                                    continue;
                                }
                                
                                if (dialogueElement.option.Contains("Hold", StringComparer.OrdinalIgnoreCase))
                                {
                                    interaction.dialogueData.dialogueElements[idx].extrapolationMode =
                                        DirectorWrapMode.Hold;
                                }
                                else
                                {
                                    interaction.dialogueData.dialogueElements[idx].extrapolationMode =
                                        DirectorWrapMode.None;
                                }

                                var digitOptions = Array.FindAll(dialogueElement.option,
                                    item => item.Any(char.IsDigit));
                                var floatOptions = digitOptions.Select(float.Parse);
                                var floats = floatOptions as float[] ?? floatOptions.ToArray();

                                interaction.dialogueData.dialogueElements[idx].waitSec =
                                    floats.Length == 1 ? floats[0] : 0f;

                                if (dialogueElement.option.Contains("name=", StringComparer.OrdinalIgnoreCase))
                                {
                                    var timelinePath = Array
                                        .Find(dialogueElement.option, item => item.Contains("name="))
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