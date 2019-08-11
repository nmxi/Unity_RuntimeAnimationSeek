using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace dev.kemomimi.runtimeAnimationSeek
{
    public class RuntimeAnimationSeek : MonoBehaviour
    {
        [SerializeField] private Slider _seekSlider;
        [SerializeField] private Text _frameShowText;
        [SerializeField] private Text _maxFrameShowText;

        [SerializeField, Space(15f)] private Animator _targetAnimator;
        [SerializeField] private RuntimeAnimatorController _animatorController;

        private AnimationClip targetClip;
        private int frameNum;
        private bool isUserControlNow;
        private Coroutine readTimeCoroutine;

        private void Awake()
        {
            isUserControlNow = false;

            _targetAnimator.runtimeAnimatorController = _animatorController;

            Initialize();

            OnPlayButtonDown();
        }

        public void Initialize()
        {
            var clipInfoList = _targetAnimator.GetCurrentAnimatorClipInfo(0);
            targetClip = clipInfoList[0].clip;

            frameNum = (int)(targetClip.length * targetClip.frameRate);    //フレームの数を算出
            _seekSlider.maxValue = frameNum;
            _maxFrameShowText.text = frameNum.ToString();

            _targetAnimator.enabled = false;
        }

        public void OnPlayButtonDown()
        {
            isUserControlNow = false;
            _targetAnimator.enabled = true;

            _frameShowText.text = _seekSlider.value.ToString();

            var time = _seekSlider.value / frameNum;  //Normalize
            var StateInfo = _targetAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = StateInfo.shortNameHash;

            _targetAnimator.Play(animationHash, 0, time);

            readTimeCoroutine = StartCoroutine(ReadTime());
        }

        public void OnPauseButtonDown()
        {
            if(readTimeCoroutine != null)
            {
                StopCoroutine(readTimeCoroutine);
                readTimeCoroutine = null;
            }

            if (_targetAnimator.enabled)
            {
                _targetAnimator.enabled = false;
            }
        }

        public void OnSliderValueChanged()
        {
            _frameShowText.text = _seekSlider.value.ToString();

            if (isUserControlNow)
            {
                _targetAnimator.enabled = true;

                var time = _seekSlider.value / frameNum;  //Normalize
                var StateInfo = _targetAnimator.GetCurrentAnimatorStateInfo(0);
                var animationHash = StateInfo.shortNameHash;

                _targetAnimator.Play(animationHash, 0, time);

                StartCoroutine(WaitPaseAnimation());
            }
        }

        private IEnumerator WaitPaseAnimation()
        {
            yield return new WaitForEndOfFrame();

            _targetAnimator.enabled = false;
        }

        private IEnumerator ReadTime()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                var currentFrame = _targetAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * frameNum;
                //Debug.Log(currentFrame);

                if(currentFrame > frameNum)
                {
                    var StateInfo = _targetAnimator.GetCurrentAnimatorStateInfo(0);
                    var animationHash = StateInfo.shortNameHash;
                    _targetAnimator.Play(animationHash, 0, 0);
                }

                _seekSlider.value = currentFrame;
            }
        }

        public void OnBeginUserControl()
        {
            isUserControlNow = true;
            OnPauseButtonDown();
            OnSliderValueChanged();
        }
    }
}