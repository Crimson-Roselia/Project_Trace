using HLH.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HLH.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private Image _redBarImage;
        [SerializeField] private Image _orangeImage;
        private PlayerController _player;
        private bool _needReduceOrangeBar = false;

        private void Awake()
        {
            _player = FindObjectOfType<PlayerController>().GetComponent<PlayerController>();
        }

        private void Update()
        {
            UpdateHealthBars();
        }

        private void UpdateHealthBars()
        {
            _redBarImage.fillAmount = _player.PlayerHealthPropotion;

            if (_orangeImage.fillAmount > _redBarImage.fillAmount)
            {
                if (_needReduceOrangeBar)
                {
                    float dropSpeed = 4f;
                    _orangeImage.fillAmount = Mathf.Lerp(_orangeImage.fillAmount, _redBarImage.fillAmount, dropSpeed * Time.deltaTime);
                }
            }
        }

        public IEnumerator StartReduceOrangeBar()
        {
            yield return new WaitForSeconds(0.5f);

            _needReduceOrangeBar = true;

            yield return new WaitForSeconds(1f);

            _needReduceOrangeBar = false;
            _orangeImage.fillAmount = _redBarImage.fillAmount;
        }
    }
}