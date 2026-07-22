using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace EasyTweens
{
    public class TweenAnimation : MonoBehaviour
    {
        [SerializeReference] public List<TweenBase> tweens = new();

        public float duration;
        public float timeSpeedMultiplier = 1;
        public bool playOnAwake;
        public bool restartOnEnable;

        float animationDelay;

        public LoopType lootType = LoopType.None;
        public bool ignoreTimeScale;

        public float currentTime;
        public int directionMultiplier = 1;

        public event Action OnPlayForwardFinished;
        public event Action OnPlayBackwardFinished;

        public bool IsInStartState => currentTime < 0.001f && !enabled;
        public bool IsInEndState => currentTime > duration * 0.999f && !enabled;

        public bool muteNonSelectedTweens;
        public bool allowCustomAnimationDuration;

        [NonSerialized] public int editorSubscriptionRetainCount;
        private float editorDeltaTime;
        private double previousEditorTime;
        public int serializationVersion = SerializationVersion.Version;

        private readonly List<TweenBase> _temporaryReorderBuffer = new();

        Action _currentOnCompleteCallback;

        private void Awake()
        {
            if (playOnAwake)
            {
                SetTime(0, -currentTime);
                Play();
            }
            else
            {
                enabled = false;
            }
        }
        
        public TweenAnimation Play(bool animated, bool muteNonSelected)
        {
            if (HasDirtyTweens())
                RecalculateDelays();

            muteNonSelectedTweens = muteNonSelected;
            directionMultiplier = 1;

            if (!animated)
            {
                SortTweensByActionTime(true);
                SetTime(duration, duration - currentTime);

                currentTime = duration;
                enabled = false;
            }
            else
            {
                SortTweensByActionTime(false);
                SetTime(0, -duration);
                currentTime = 0;

                ActivateAnimation();
            }

            RevertSorting();

            return this;
        }

        private void RevertSorting()
        {
            tweens.Clear();
            tweens.AddRange(_temporaryReorderBuffer);
        }

        private void SortTweensByActionTime(bool ascending)
        {
            _temporaryReorderBuffer.Clear();
            _temporaryReorderBuffer.AddRange(tweens);
            tweens.Sort((t, t2) =>
            {
                float diff = t.TotalDelay + t.Duration - t2.TotalDelay - t2.Duration;

                if (Mathf.Approximately(diff, 0))
                    return 0;

                return (ascending ? 1 : -1) * (int)Mathf.Sign(diff);
            });
        }
        
        
        public TweenAnimation Play(bool animated)
        {
            return Play(animated, false);
        }

        public void SimplePlay() // used for UnityEvents
        {
            Play();
        }
        
        public void SimplePlayBackward() // used for UnityEvents
        {
            PlayBackward();
        }
        
        public void SetInitialState()
        {
            PlayBackward(false);
        }
        
        public void SetFinalState()
        {
            Play(false);
        }
        
        public TweenAnimation Play()
        {
            return Play(true);
        }

        
        public TweenAnimation Play(bool animated, Action onComplete)
        {
            _currentOnCompleteCallback = onComplete;
            return Play(animated);
        }
        
        public TweenAnimation Play(Action onComplete)
        {
           return Play(true, onComplete);
        }

        public TweenAnimation PlayBackward(bool animated, bool muteNonSelected)
        {
            if (HasDirtyTweens())
                RecalculateDelays();

            muteNonSelectedTweens = muteNonSelected;
            directionMultiplier = -1;

            SortTweensByActionTime(false);

            if (!animated)
            {
                SetTime(0, -currentTime);

                currentTime = 0;
                enabled = false;
            }
            else
            {
                currentTime = duration;
                for (var i = tweens.Count - 1; i >= 0; i--)
                {
                    var tween = tweens[i];
                    tween.SetFactor(1);
                }

                ActivateAnimation();
            }

            RevertSorting();

            return this;
        }

        public TweenAnimation PlayBackward(bool animated)
        {
            return PlayBackward(animated, false);
        }

        public TweenAnimation PlayBackward()
        {
            return PlayBackward(true);
        }

        public TweenAnimation PlayBackward(bool animated, Action onComplete)
        {
            _currentOnCompleteCallback = onComplete;
            return PlayBackward(animated);
        }
        public TweenAnimation PlayBackward(Action onComplete)
        {
            return PlayBackward(true, onComplete);
        }

        public void Pause()
        {
            enabled = false;
        }

        public void Stop()
        {
            enabled = false;
            SetTime(0, -currentTime);
        }

        public void Resume()
        {
            ActivateAnimation();
        }

        public TweenAnimation SetDelay(float delay)
        {
            animationDelay = delay;
            return this;
        }

        public TweenAnimation WithOnComplete(Action onComplete)
        {
            _currentOnCompleteCallback = onComplete;
            return this;
        }

        void ActivateAnimation()
        {
            enabled = true;
            EnsureDuration();
        }

        public void EnsureDuration()
        {
            float maxDuration = allowCustomAnimationDuration ? duration : 0;
            for (int i = 0; i < tweens.Count; i++)
            {
                if (tweens[i].Duration + tweens[i].TotalDelay > maxDuration)
                {
                    maxDuration = tweens[i].Duration + tweens[i].TotalDelay;
                }
            }

            if (!Mathf.Approximately(duration, maxDuration))
            {
                duration = maxDuration;
            }
        }

        private void Update()
        {
            if (enabled)
            {
                var deltaTime = GetDeltaTime() * timeSpeedMultiplier;
                if (animationDelay > 0)
                {
                    animationDelay -= deltaTime;
                    return;
                }

                deltaTime *= directionMultiplier;
                currentTime += deltaTime;

                if (currentTime < 0 && directionMultiplier < 0)
                {
                    SetTime(0, deltaTime - currentTime);
                    OnPlayBackwardFinished?.Invoke();
                    FinishAnimationCycle();
                    deltaTime = currentTime * directionMultiplier;
                }
                else if (currentTime > duration && directionMultiplier > 0)
                {
                    SetTime(duration, deltaTime - currentTime + duration);
                    OnPlayForwardFinished?.Invoke();
                    FinishAnimationCycle();
                    deltaTime = (duration - currentTime) * directionMultiplier;
                }

                SetTime(currentTime, deltaTime);
            }
        }

        void FinishAnimationCycle()
        {
            _currentOnCompleteCallback?.Invoke();
            _currentOnCompleteCallback = null;

            switch (lootType)
            {
                case LoopType.None:
                    currentTime = Mathf.Clamp(currentTime, 0, duration);
                    enabled = false;
                    break;

                case LoopType.Loop:
                    if (directionMultiplier > 0)
                    {
                        SortTweensByActionTime(false);
                        SetTime(0, -duration);
                        currentTime = currentTime - duration;
                        RevertSorting();
                    }
                    else
                    {
                        SortTweensByActionTime(true);
                        SetTime(1, duration);
                        currentTime = duration - currentTime;
                        RevertSorting();
                    }

                    break;

                case LoopType.PingPong:
                    if (directionMultiplier > 0)
                    {
                        currentTime = duration - (currentTime - duration);
                    }
                    else
                    {
                        currentTime = -currentTime;
                    }

                    directionMultiplier *= -1;

                    break;
            }

            while (currentTime > duration * 2 && duration > float.Epsilon)
            {
                currentTime -= duration * 2;
            }
        }

        public void SetTime(float time, float deltaTime)
        {
            for (int i = 0; i < tweens.Count; i++)
            {
                if (!muteNonSelectedTweens || tweens[i].isSelected)
                    tweens[i].UpdateTween(time, deltaTime);
            }
        }

        public bool SetTweenLink(TweenBase mainTween, TweenBase dependantTween)
        {
            TweenBase tweenFromChain = mainTween;

            while (!string.IsNullOrEmpty(tweenFromChain.LinkedTweenGuid))
            {
                if (tweenFromChain.LinkedTweenGuid == dependantTween.TweenGuid)
                {
                    Debug.LogError("Circular tween link detected. Tween at index " +
                        tweens.IndexOf(mainTween) +
                        " is already linked to tween at index " +
                        tweens.IndexOf(dependantTween) +
                        ".");
                    return false;
                }

                tweenFromChain = GetTweenById(tweenFromChain.LinkedTweenGuid);
            }

            dependantTween.LinkedTweenGuid = mainTween.TweenGuid;

            dependantTween.IsDirty = true;
            return true;
        }

        public bool HasDirtyTweens()
        {
            for (int i = 0; i < tweens.Count; i++)
            {
                if (tweens[i].IsDirty)
                {
                    return true;
                }
            }

            return false;
        }

        public void RecalculateDelays()
        {
            if (HasDirtyTweens())
            {
                foreach (var tween in tweens)
                {
                    tween.LinkedTweenDelay = GetLinkedDelay(tween);
                    tween.IsDirty = false;
                }
            }
        }

        float GetLinkedDelay(TweenBase tween)
        {
            if (string.IsNullOrEmpty(tween.LinkedTweenGuid))
            {
                return 0;
            }

            TweenBase linkedTween = GetTweenById(tween.LinkedTweenGuid);
            if (linkedTween != null)
            {
                return linkedTween.Delay + linkedTween.Duration + GetLinkedDelay(linkedTween);
            }
            else
            {
                tween.LinkedTweenGuid = string.Empty;
                return 0;
            }
        }

        public TweenBase GetTweenById(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            for (int i = 0; i < tweens.Count; i++)
            {
                if (tweens[i].TweenGuid == guid)
                {
                    return tweens[i];
                }
            }

            return null;
        }

        public TweenBase GetTweenByIndex(int index)
        {
            if (index < 0 || index >= tweens.Count) return null;
            return tweens[index];
        }

        public TweenBase GetTweenByNameOverride(string overrideName)
        {
            for (int i = 0; i < tweens.Count; i++)
            {
                if (tweens[i].NameOverride == overrideName)
                {
                    return tweens[i];
                }
            }

            return null;
        }

        float GetDeltaTime()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return editorDeltaTime;
            }
#endif
            return ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        }

#if UNITY_EDITOR

        public void SubscribeToEditorUpdates()
        {
            editorSubscriptionRetainCount++;
            if (editorSubscriptionRetainCount == 1)
                EditorApplication.update += EditorUpdate;
        }

        public void UnsubscribeFromEditorUpdates()
        {
            editorSubscriptionRetainCount--;
            if (editorSubscriptionRetainCount <= 0)
            {
                EditorApplication.update -= EditorUpdate;
                editorSubscriptionRetainCount = 0;
            }
        }

        void EditorUpdate()
        {
            editorDeltaTime = (float)(EditorApplication.timeSinceStartup - previousEditorTime);
            previousEditorTime = EditorApplication.timeSinceStartup;

            try
            {
                if (!EditorApplication.isPlaying && enabled)
                {
                    Update();
                }
            }
            catch
            {
                //could throw when prefab editor is closed, we don't care
            }
        }
#endif

        #region Obsolete

        [Obsolete("Use Play instead")]
        public void PlayForward(bool animated, bool muteNonSelected)
        {
            Play(animated, muteNonSelectedTweens);
        }

        [Obsolete("Use Play instead")]
        public void PlayForward(bool animated)
        {
            Play(animated);
        }

        [Obsolete("Use Play instead")]
        public void PlayForward()
        {
            Play();
        }

        #endregion
    }
}