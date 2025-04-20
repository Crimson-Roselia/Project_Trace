using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.Mechanics
{
    public class Character
    {
        public string UnifiedID = "";
        public RectTransform Root;
        public Image Image;
        private Color _highlightedColor = Color.white;
        private Color _unhighlightedColor { get { return new Color(_highlightedColor.r * 0.65f, _highlightedColor.g * 0.65f, _highlightedColor.b * 0.65f, _highlightedColor.a); } }
        public bool IsHighlight = false;
        private DialogueSystem _dialogueSystem;

        private Coroutine _showingProcess, _hidingProcess, _movingProcess;
        public bool IsShowing { get { return _showingProcess != null; } }
        public bool IsHiding { get { return _hidingProcess != null; } }
        public bool IsMoving { get { return _movingProcess != null; } }

        public Character(string unifiedID, GameObject characterObj, DialogueSystem dialogueSystem)
        {
            UnifiedID = unifiedID;
            _dialogueSystem = dialogueSystem;
            if (characterObj != null)
            {
                characterObj.SetActive(true);
                Root = characterObj.GetComponent<RectTransform>();
                Image = characterObj.GetComponentInChildren<Image>();
            }

            CommandManager.Instance.RegisterCommand(UnifiedID + "_" + "Show", new Func<Coroutine>(Show));
            CommandManager.Instance.RegisterCommand(UnifiedID + "_" + "Hide", new Func<Coroutine>(Hide));
            CommandManager.Instance.RegisterCommand(UnifiedID + "_" + "SetPosition", new Action<float>(SetPosition));
            CommandManager.Instance.RegisterCommand(UnifiedID + "_" + "MoveToPosition", new Func<float, float, Coroutine>(MoveToPosition));
        }

        public Coroutine Show()
        {
            if (IsShowing)
            {
                return _showingProcess;
            }
            else if (IsHiding)
            {
                _dialogueSystem.StopCoroutine(_hidingProcess);
            }
            Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 0);
            return _dialogueSystem.StartCoroutine(Showing());
        }


        public Coroutine Hide()
        {
            if (IsHiding)
            {
                return _hidingProcess;
            }
            else if (IsShowing)
            {
                _dialogueSystem.StopCoroutine(_showingProcess);
            }
            Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 1f);
            return _dialogueSystem.StartCoroutine(Hiding());
        }

        private IEnumerator Showing()
        {
            while (Image.color.a < 1f)
            {
                float alpha = Mathf.MoveTowards(Image.color.a, 1f, 3f * Time.deltaTime);
                Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, alpha);
                yield return null;
            }
        }

        private IEnumerator Hiding()
        {
            while (Image.color.a > 0f)
            {
                float alpha = Mathf.MoveTowards(Image.color.a, 0f, 3f * Time.deltaTime);
                Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, alpha);
                yield return null;
            }
        }

        public void Highlight()
        {
            Image.color = _highlightedColor;
            IsHighlight = true;
        }

        public void Unhighlight()
        {
            Image.color = _unhighlightedColor;
            IsHighlight = false;
        }

        private float GetUIPositionXFromNormalizedValue(float value)
        {
            float normalizedValue = Mathf.Clamp(value, 0f, 1f);
            float posX = _dialogueSystem.LeftBorderMarker.localPosition.x + (_dialogueSystem.RightBorderMarker.localPosition.x - _dialogueSystem.LeftBorderMarker.localPosition.x) * normalizedValue;
            posX = Mathf.Clamp(posX, _dialogueSystem.LeftBorderMarker.localPosition.x + Root.rect.width / 2, _dialogueSystem.RightBorderMarker.localPosition.x - Root.rect.width / 2);
            return posX;
        }

        public void SetPosition(float value)
        {
            Root.anchoredPosition = new Vector2(GetUIPositionXFromNormalizedValue(value), Root.anchoredPosition.y);
        }

        public Coroutine Jump(float height)
        {
            return _dialogueSystem.StartCoroutine(Jumping(height));
        }

        public IEnumerator Jumping(float height)
        {
            yield return null;
        }

        public Coroutine MoveToPosition(float value, float speed)
        {
            if (IsMoving)
            {
                _dialogueSystem.StopCoroutine(_movingProcess);
            }
            return _dialogueSystem.StartCoroutine(MovingToPosition(value, speed));
        }

        private IEnumerator MovingToPosition(float value, float speed)
        {
            while (Root.anchoredPosition.x != GetUIPositionXFromNormalizedValue(value))
            {
                float posX = Mathf.MoveTowards(Root.anchoredPosition.x, GetUIPositionXFromNormalizedValue(value), speed * Time.deltaTime);
                Root.anchoredPosition = new Vector2(posX, Root.anchoredPosition.y);
                yield return null;
            }
            _movingProcess = null;
        }
    }
}