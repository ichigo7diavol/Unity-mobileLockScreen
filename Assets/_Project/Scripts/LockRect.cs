using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LockScreen
{
    public class LockRect 
        : MonoBehaviour
    {
        [SerializeField] 
        private LockButton[] _buttons;

        [SerializeField] 
        private Transform _lineRoot;

        [SerializeField] private Canvas _canvas;

        [SerializeField] 
        private string _pinString;
        
        [SerializeField] 
        private Color _defaultColor;
        
        [SerializeField] 
        private Color _validColor;

        [SerializeField] 
        private Color _invalidColor;
        
        private List<RectTransform> _lineSegments 
            = new List<RectTransform>();
        
        private List<LockButton> _currentSequence 
            = new List<LockButton>();

        private int[] _pin;
        private bool _isHold;
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (!_isHold)
            {
                return;
            }
            UpdateLine(Input.mousePosition);
        }

        private void Initialize()
        {
            for (var i = 0; i < _buttons.Length; ++i)
            {
                _buttons[i].PointerUp += OnButtonPointerUp;
                _buttons[i].PointerDown += OnButtonPointerDown;
                _buttons[i].PointerEnter += OnButtonPointerEnter;
            }
            CheckPinIsValid();
        }
        
        private void CheckPinIsValid()
        {
            if (_pinString.Length > _buttons.Length)
            {
                Debug.LogError("[LockScreen] Wrong buttons count!");
                
                return;
            }
            var pin = new List<int>();

            for (var i = 0; i < _pinString.Length; i++)
            {
                if (!char.IsDigit(_pinString[i]))
                {
                    Debug.LogError("[LockScreen] Wrong pin string contains not a number symbol!");

                    return;
                }
                pin.Add(_pinString[i] - '0');
            }
            _pin = pin.ToArray();
        }
        
        private void CheckLock()
        {
            if (_currentSequence.Count != _pin.Length)
            {
                OnWrongPin();
            }
            if (RunLuaPinCheck())
            {
                OnCorrectPin();
            }
            else
            {
                OnWrongPin();
            }
        }

        private void OnWrongPin()
        {
            for (var i = 0; i < _buttons.Length; ++i)
            {
                _buttons[i].SetCircleColor(_invalidColor);
            }
            Debug.Log($"[LockScreen] Wrong pin! " +
                      $"\nCurrent: {string.Join(" ", _currentSequence.Select(e => e.ButtonIndex.ToString()))}" +
                      $"\nCorrect: {string.Join(" ", _pin.Select(e => e.ToString()))}");
        }
        
        private void OnCorrectPin()
        {
            for (var i = 0; i < _buttons.Length; ++i)
            {
                _buttons[i].SetCircleColor(_validColor);
            }
            Debug.Log("[LockScreen] Correct pin!");
        }

        private void UpdateLine(Vector3 _targetPosition)
        {
            var lastElementPosition = _currentSequence.Last().InnerCircleRectTransform.position;
            var distance = _targetPosition - lastElementPosition;
            var lastSegment = _lineSegments.Last();
            var rect = lastSegment.sizeDelta;

            lastSegment.transform.rotation = Quaternion.FromToRotation(Vector3.right, 
                distance.normalized);
            
            rect.x = distance.magnitude;
            
            lastSegment.sizeDelta = rect;
            lastSegment.position = lastElementPosition + distance / 2;
        }

        private void OnButtonPointerDown(PointerEventData data, LockButton button)
        {
            if (!_isHold)
            {
                ResetColors();
                CreateSegment(button);
            }
            _isHold = true;
        }
        
        private void OnButtonPointerUp(PointerEventData data, LockButton button)
        {
            var lastSegment = _lineSegments.Last();

            if (lastSegment != null)
            {
                _lineSegments.RemoveAt(_lineSegments.Count - 1);
                
                Destroy(lastSegment.gameObject);
            }
            _isHold = false;
            
            CheckLock();
            
            DoReset();
        }
        
        private void CreateSegment(LockButton button)
        {
            var lastIndex = _lineSegments.Count + 1;
            var segment = new GameObject($"Segment_{lastIndex}");
            
            var rectTransform = segment.AddComponent<RectTransform>();
            var image = segment.AddComponent<Image>();
            
            var delta = rectTransform.sizeDelta;
            
            _lineSegments.Add(rectTransform);
            _currentSequence.Add(button);

            image.raycastTarget = false;
            
            segment.transform.parent = _lineRoot;
            
            delta.y = _currentSequence.First().InnerCircleRectTransform.sizeDelta.y 
                      * _canvas.scaleFactor;

            delta.x = delta.y; 
            
            rectTransform.sizeDelta = delta;

            rectTransform.anchoredPosition 
                = button.InnerCircleRectTransform.position;
            
            lastIndex++;
        }

        private bool RunLuaPinCheck()
        {
            var func = 
            @"
                function CheckPin(current, actual, actual_size)
                    
                    for i=0, actual_size - 1 do
                        
                        if (current[i] ~= actual[i]) then
                            return false 
                        end
                    end
    
                    return true
                end
            ";
            
            var script = new Script();
            
            script.DoString(func);

            return script.Call(script.Globals["CheckPin"], 
                _currentSequence.Select(e => e.ButtonIndex).ToArray(), 
                _pin, 
                _pin.Length).Boolean;
        }

        private void DoReset()
        {
            foreach (var rectTransform in _lineSegments)
            {
                Destroy(rectTransform.gameObject);
            }
            _lineSegments.Clear();
            _currentSequence.Clear();
        }

        private void ResetColors()
        {
            for (var i = 0; i < _buttons.Length; ++i)
            {
                _buttons[i].SetCircleColor(_defaultColor);
            }
        }

        private void OnButtonPointerEnter(PointerEventData data, LockButton button)
        {
            if (!_isHold)
            {
                return;
            }
            if (_currentSequence.Contains(button))
            {
                return;
            }
            if (_currentSequence.Any())
            {
                UpdateLine(button.InnerCircleRectTransform.position);
            }
            CreateSegment(button);
        }
    }
}