using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LockScreen
{
    public class LockButton 
        : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        public event Action<PointerEventData, LockButton> PointerDown;
        public event Action<PointerEventData, LockButton> PointerUp;
        public event Action<PointerEventData, LockButton> PointerEnter;

        [SerializeField] 
        private int _buttonIndex;
        
        [FormerlySerializedAs("_rectTransform")] 
        [SerializeField] 
        private RectTransform innerCircleRectTransform;

        [SerializeField] 
        private Image _circleImage;
        
        public RectTransform InnerCircleRectTransform => innerCircleRectTransform;
        public int ButtonIndex => _buttonIndex;
        
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
        }
        
        public void SetCircleColor(Color color)
        {
            _circleImage.color = color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"On pointer down: {_buttonIndex}");
            
            PointerDown?.Invoke(eventData, this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"On pointer up: {_buttonIndex}");
            
            PointerUp?.Invoke(eventData, this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"On pointer enter: {_buttonIndex}");

            PointerEnter?.Invoke(eventData, this);
        }
    }
}