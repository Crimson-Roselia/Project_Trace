using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VisualNovel.IO;
using VisualNovel.Mechanics.Data;

namespace VisualNovel.Mechanics
{
    public class DialogueSystem : MonoBehaviour
    {
        public RectTransform LeftBorderMarker;
        public RectTransform RightBorderMarker;
        [SerializeField] public TextMeshProUGUI nameText;
        [SerializeField] public TextMeshProUGUI dialogueText;
        [SerializeField] private RectTransform characterPanel;
        [SerializeField] private CanvasGroup dialoguePanel;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image backBubble;
        [SerializeField] private Sprite samSprite;
        [SerializeField] private Sprite miaSprite;
        [SerializeField] private CanvasGroup characterIntroPanel;
        [SerializeField] private Image introCharacterBackImage;
        [SerializeField] private Image introCharacterFrontImage;
        [SerializeField] private TextMeshProUGUI introCharacterUpperText;
        [SerializeField] private TextMeshProUGUI introCharacterLowerText1;
        [SerializeField] private TextMeshProUGUI introCharacterLowerText2;
        [SerializeField] private Image cinematicCurtain;
        private TextBuilder _textBuilder;
        private TextFileReader _textFileReader;
        private int _playIndex = 0;
        private List<DialogueLine> _dialogueLines;
        private readonly string _prefabFolderInResources = "Characters";
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();
        private bool _isPlayingDialogue;

        public static DialogueSystem Instance { get; private set; }

        public bool IsDialoguePanelVisible { get { return dialoguePanel.alpha != 0; } }

        private void Awake()
        {
            Instance = this;
        }


        private void Start()
        {
            _textBuilder = new TextBuilder(dialogueText);
            _textFileReader = new TextFileReader();
            _dialogueLines = new List<DialogueLine>();

            CommandManager.Instance.RegisterCommand("CreateCharacter", new Action<string>(CreateCharacter));
            CommandManager.Instance.RegisterCommand("ShowSamIntroduction", new Action(ShowSamIntroduction));
            CommandManager.Instance.RegisterCommand("ShowMiaIntroduction", new Action(ShowMiaIntroduction));
            CommandManager.Instance.RegisterCommand("HideIntroduction", new Action(HideIntroduction));
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!_isPlayingDialogue)
                {
                    if (_playIndex < _dialogueLines.Count)
                    {
                        StartCoroutine(PlayNextLine(0));
                    }
                    else
                    {
                        if (IsDialoguePanelVisible)
                        {
                            CloseDialogueUI();
                            CrossfadeIntoCombat();
                        }
                    }
                }
            }
        }

        public void StartDialogue(string textFileName)
        {
            List<string> rawLines = _textFileReader.ReadTextAsset(textFileName);
            _dialogueLines = new List<DialogueLine>();
            _playIndex = 0;
            for (int i = 0; i < rawLines.Count; i++)
            {
                DialogueLine nextLine = new DialogueLine(rawLines[i]);
                if (nextLine.HasCommand() || nextLine.HasDialogue())
                {
                    _dialogueLines.Add(nextLine);
                }
            }

            CrossfadeIntoNarration();
        }

        private IEnumerator PlayNextLine(float waitSec)
        {
            _isPlayingDialogue = true;

            if (waitSec != 0)
            {
                yield return new WaitForSeconds(waitSec);
            }

            if (_playIndex < _dialogueLines.Count)
            {
                DialogueLine nextLine = _dialogueLines[_playIndex];
                _playIndex++;

                if (nextLine.HasCommand())
                {
                    foreach (CommandDataContainer command in nextLine.CommandsData)
                    {
                        yield return CommandManager.Instance.ExecuteCommand(command.Name, command.Arguments);
                    }
                }

                if (nextLine.HasSpeaker())
                {
                    nameText.text = nextLine.SpeakerName;
                    if (nextLine.SpeakerName == "SAM")
                    {
                        characterImage.sprite = samSprite;
                    }
                    else if (nextLine.SpeakerName == "MIA")
                    {
                        characterImage.sprite = miaSprite;
                    }
                }
                else
                {
                    if (!nextLine.HasCommand())
                    {
                        nameText.text = "";
                    }
                }

                if (nextLine.HasDialogue())
                {
                    if (!IsDialoguePanelVisible)
                    {
                        dialoguePanel.DOFade(1, 0.3f);
                    }
                    _textBuilder.Build(nextLine.DialogueText);
                }
                else
                {
                    dialogueText.text = "";
                }

                _isPlayingDialogue = false;
            }
        }

        private void OpenDialogueUI()
        {
            dialoguePanel.DOFade(1, 0.75f);
        }

        private void CloseDialogueUI()
        {
            dialoguePanel.DOFade(0, 0.75f);
        }

        public void ShowSamIntroduction()
        {
            _isPlayingDialogue = true;
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.Append(dialoguePanel.DOFade(0, 0.3f));
            introCharacterFrontImage.sprite = samSprite;
            introCharacterBackImage.sprite = samSprite;
            introCharacterUpperText.text = "OCT少尉·第三部队队长";
            introCharacterLowerText1.text = "流浪的研究者·山姆";
            introCharacterLowerText2.text = "SAM";
            s.AppendCallback(() => introCharacterFrontImage.rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f));
            s.AppendCallback(() => introCharacterFrontImage.rectTransform.DOScale(new Vector3(1, 1, 1), 1.2f));
            s.AppendCallback(() => introCharacterBackImage.rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f));
            s.AppendCallback(() => introCharacterBackImage.rectTransform.DOScale(new Vector3(1, 1, 1), 1.2f));
            s.Append(characterIntroPanel.DOFade(1, 1.5f));
        }

        public void ShowMiaIntroduction()
        {
            _isPlayingDialogue = true;
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.Append(dialoguePanel.DOFade(0, 0.3f));
            introCharacterFrontImage.sprite = miaSprite;
            introCharacterBackImage.sprite = miaSprite;
            introCharacterUpperText.text = "OCT少尉·第三部队队长";
            introCharacterLowerText1.text = "自律战斗人偶·米娅";
            introCharacterLowerText2.text = "AUTOBOT MIA";
            s.AppendCallback(() => introCharacterFrontImage.rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f));
            s.AppendCallback(() => introCharacterFrontImage.rectTransform.DOScale(new Vector3(1, 1, 1), 1.2f));
            s.AppendCallback(() => introCharacterBackImage.rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f));
            s.AppendCallback(() => introCharacterBackImage.rectTransform.DOScale(new Vector3(1, 1, 1), 1.2f));
            s.Append(characterIntroPanel.DOFade(1, 1.5f));
        }

        public void HideIntroduction()
        {
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.Append(characterIntroPanel.DOFade(0, 0.6f));
            s.AppendCallback(() => _isPlayingDialogue = false);
            StartCoroutine(PlayNextLine(0.2f));
        }

        private GameObject GetPrefabForCharacter(string characterID)
        {
            string prefabPath = _prefabFolderInResources + $"/{characterID}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("角色名不正确或角色物体的预制体不存在。错误角色：" + $"{characterID}");
            }
            return prefab;
        }

        public void CreateCharacter(string characterID)
        {
            if (characters.ContainsKey(characterID))
            {
                Debug.LogWarning(characterID + "角色已经存在！请勿重复创建。");
                return;
            }
            GameObject characterObj = Instantiate(GetPrefabForCharacter(characterID), characterPanel);
            Character character = new Character(characterID, characterObj, this);
            characters.Add(characterID, character);
        }

        private void CrossfadeIntoNarration()
        {
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.Append(cinematicCurtain.DOFade(1f, 1f));
            s.AppendInterval(1f);
            s.Append(cinematicCurtain.DOFade(0f, 1f));
            s.AppendCallback(() => GameManager.Instance.EnterGameState(GameState.Narration));
            s.AppendCallback(() => StartCoroutine(PlayNextLine(0)));
        }

        public void CrossfadeIntoCombat()
        {
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.Append(cinematicCurtain.DOFade(1f, 1f));
            s.AppendInterval(1f);
            s.Append(cinematicCurtain.DOFade(0f, 1f));
            s.AppendCallback(() => GameManager.Instance.EnterGameState(GameState.Combat));
        }

        private void TransitionFadeIn()
        {
            cinematicCurtain.DOFade(0f, 2f);
        }
    }
}