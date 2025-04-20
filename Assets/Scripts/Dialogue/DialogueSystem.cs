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
        [SerializeField] private Image introCharacterBackImagel;
        [SerializeField] private Image introCharacterFrontImage;
        [SerializeField] private TextMeshProUGUI introCharacterUpperText;
        [SerializeField] private TextMeshProUGUI introCharacterLowerText1;
        [SerializeField] private TextMeshProUGUI introCharacterLowerText2;
        private TextBuilder _textBuilder;
        private TextFileReader _textFileReader;
        private int _playIndex = 0;
        private List<DialogueLine> _dialogueLines;
        private readonly string _prefabFolderInResources = "Characters";
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        public static DialogueSystem Instance { get; private set; }


        private void Start()
        {
            _textBuilder = new TextBuilder(dialogueText);
            _textFileReader = new TextFileReader();

            CommandManager.Instance.RegisterCommand("CreateCharacter", new Action<string>(CreateCharacter));
        }

        private void Update()
        {
            if (_playIndex < _dialogueLines.Count)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(PlayNextLine());
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

            OpenDialogueUI();
            StartCoroutine(PlayNextLine());
        }

        private IEnumerator PlayNextLine()
        {
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

                    }
                    else if (nextLine.SpeakerName == "MIA")
                    {

                    }
                }
                else
                {
                    nameText.text = "";
                    characterImage.sprite = null;
                }

                if (nextLine.HasDialogue())
                {
                    _textBuilder.Build(nextLine.DialogueText);
                }
                else
                {
                    dialogueText.text = "";
                }
            }
            else
            {
                CloseDialogueUI();
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
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.Append(dialoguePanel.DOFade(0, 0.3f));
            introCharacterFrontImage.sprite = samSprite;
            introCharacterBackImagel.sprite = samSprite;
            introCharacterUpperText.text = "OCT少尉·第三部队队长";
            s.Append(characterIntroPanel.DOFade(1, 0.6f));
        }

        public void ShowMiaIntroduction()
        {

        }

        public void HideIntroduction()
        {
            
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
    }
}