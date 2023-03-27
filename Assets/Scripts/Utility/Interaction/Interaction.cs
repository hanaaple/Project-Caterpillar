using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Utility.Dialogue;
using Utility.JsonLoader;

namespace Utility.Interaction
{
    public abstract class Interaction : MonoBehaviour
    {
        [SerializeField] protected bool isOnLoadScene;

        [SerializeField] private InteractionData[] interactionData;

        // for debugging
        [SerializeField] protected int interactionIndex;

        [NonSerialized] public bool IsClear;

        private Action _onEndInteraction;
        private static readonly int State = Animator.StringToHash("State");

        protected virtual void Awake()
        {
            if (isOnLoadScene)
            {
                Debug.LogWarning("Awake Interaction, OnLoadScene");
                SceneLoader.SceneLoader.Instance.OnLoadSceneEnd += () => { StartInteraction(); };
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
                interaction.isInteracted = false;
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

            Debug.Log($"이름: {gameObject.name}");

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

            var interaction = GetInteractionData(index);
            interaction.isInteracted = true;
            // interaction.onInteractionEnd?.Invoke();

            var nextIndex = (index + 1) % interactionData.Length;
            var nextInteraction = GetInteractionData(nextIndex);

            GetComponent<Collider2D>().enabled = false;

            if (interaction.isContinuable)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
                GetComponent<Collider2D>().enabled = true;
            }

            if (interaction.isLoop)
            {
                GetComponent<Collider2D>().enabled = true;
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
            return interactionData.All(item => item.isInteracted);
        }

        protected virtual bool IsInteractable(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            var interaction = interactionData[index];
            return (interaction.isLoop || !interaction.isInteracted) && interaction.isInteractable;
        }

        protected InteractionData GetInteractionData(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            return interactionData[index];
        }

#if UNITY_EDITOR
        public void ShowDialogue()
        {
            for (var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];
                interaction.dialogueData.dialogueElements =
                    JsonHelper.GetJsonArray<DialogueElement>(interaction.jsonAsset.text);

                for (var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];
                    if (dialogueElement.dialogueType == DialogueType.CutScene && dialogueElement.option?.Length > 0 &&
                        dialogueElement.option.Contains("Reset"))
                    {
                        interaction.dialogueData.dialogueElements[idx].playableAsset =
                            Resources.Load<PlayableAsset>("Timeline/Reset");
                        continue;
                    }

                    if (dialogueElement.dialogueType is DialogueType.Interact or DialogueType.WaitInteract
                        or DialogueType.MoveMap)
                    {
                        Debug.LogWarning($"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                    }

                    if (dialogueElement.option != null)
                    {
                        if (dialogueElement.option.Contains("Hold", StringComparer.OrdinalIgnoreCase))
                        {
                            interaction.dialogueData.dialogueElements[idx].extrapolationMode = DirectorWrapMode.Hold;
                        }
                        else
                        {
                            interaction.dialogueData.dialogueElements[idx].extrapolationMode = DirectorWrapMode.None;
                        }

                        if (dialogueElement.option.Contains("name=", StringComparer.OrdinalIgnoreCase))
                        {
                            var timelinePath = Array.Find(dialogueElement.option, item => item.Contains("name="))
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
                        else if (dialogueElement.dialogueType == DialogueType.CutScene)
                        {
                            Debug.LogWarning(
                                $"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                        }
                    }
                    else if (dialogueElement.dialogueType is DialogueType.CutScene)
                    {
                        Debug.LogWarning($"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                    }
                }
            }
        }
#endif
    }
}