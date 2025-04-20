using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VisualNovel.Mechanics
{
    public class TextBuilder
    {
        private TextMeshProUGUI _targetDialogueText;
        private string _completeText = "";
        private float _buildSpeed = 1f;
        private int _charactersPerCycle = 2;
        private Coroutine _buildProcess = null;
        public bool IsBuilding { get { return _buildProcess != null; } }

        public TextBuilder(TextMeshProUGUI targetDialogueText)
        {
            _targetDialogueText = targetDialogueText;
        }

        public void Build(string text)
        {
            _completeText = text;
            Stop();
            _buildProcess = _targetDialogueText.StartCoroutine(BuildingText());
        }

        public void ForceComplete()
        {
            _targetDialogueText.maxVisibleCharacters = _targetDialogueText.textInfo.characterCount;
            Stop();
            OnComplete();
        }

        private IEnumerator BuildingText()
        {
            PrepareToShowText();
            yield return GraduallyShowText();
            OnComplete();
        }

        private void OnComplete()
        {
            _buildProcess = null;
        }

        private IEnumerator GraduallyShowText()
        {
            while (_targetDialogueText.maxVisibleCharacters < _targetDialogueText.textInfo.characterCount)
            {
                _targetDialogueText.maxVisibleCharacters += _charactersPerCycle;
                yield return new WaitForSeconds(0.025f / _buildSpeed);
            }
        }


        private void PrepareToShowText()
        {
            _targetDialogueText.text = _completeText;
            _targetDialogueText.maxVisibleCharacters = 0;
            _targetDialogueText.ForceMeshUpdate();
        }

        private void Stop()
        {
            if (!IsBuilding)
            {
                return;
            }
            _targetDialogueText.StopCoroutine(_buildProcess);
            OnComplete();
        }
    }
}