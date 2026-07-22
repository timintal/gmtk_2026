using System;
using EasyTweens;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Code.UI
{
    /// <summary>
    /// Waits for any button/key press, then disables a target GameObject.
    /// Optionally plays a <see cref="TweenAnimation"/> first (e.g. fade-out that already
    /// contains a DeActivateGameObject tween).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DisableOnAnyInput : MonoBehaviour
    {
        [SerializeField] GameObject _target;
        [Tooltip("If set, played on input. Disable happens via the tween (or onComplete fallback).")]
        [SerializeField] TweenAnimation _hideAnimation;
        [Tooltip("Ignore input for this many seconds after enable (avoids leftover key presses).")]
        [SerializeField] float _listenDelay = 0.15f;
        [Tooltip("When no hide animation is set, disable immediately. When an animation is set, also disable on complete as a safety net.")]
        [SerializeField] bool _disableOnComplete = true;
        [SerializeField] UnityEvent _onTriggered;

        IDisposable _subscription;
        bool _triggered;
        float _listenAfterTime;

        void OnEnable()
        {
            _triggered = false;
            _listenAfterTime = Time.unscaledTime + Mathf.Max(0f, _listenDelay);
            _hideAnimation?.SetInitialState();
            _subscription?.Dispose();
            _subscription = InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
        }

        void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        void OnAnyButtonPress(InputControl _)
        {
            if (_triggered || Time.unscaledTime < _listenAfterTime)
                return;

            Trigger();
        }

        public void Trigger()
        {
            if (_triggered)
                return;

            _triggered = true;
            _subscription?.Dispose();
            _subscription = null;

            _onTriggered?.Invoke();

            if (_hideAnimation != null)
            {
                if (_disableOnComplete)
                    _hideAnimation.Play(DisableTarget);
                else
                    _hideAnimation.Play();
                return;
            }

            DisableTarget();
        }

        void DisableTarget()
        {
            var target = _target != null ? _target : gameObject;
            if (target != null)
                target.SetActive(false);
        }
    }
}
