using System.Windows.Forms.VisualStyles;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Scrollbar : MonoBehaviour
{
    private float _horizontalOffset = 0.0f;
    private float _currentPosition = 0.0f;

    private bool _hovering = false;
    private bool _pressed = false;

    public UnityEvent<float> onHeld;
    
    private Button _scrollButton;
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        _scrollButton = root.Q<VisualElement>("ScrollableWaveformContainer")
                .Q<VisualElement>("VisualElement").Q<Button>("Scroll");
        
        // _currentPosition = _scrollButton.
        // Maybe just make this an image instead of button
        
        _scrollButton.clickable = null;
        
        // _scrollButton.RegisterCallback((MouseOverEvent evt) => _hovering = true);
        // _scrollButton.RegisterCallback((MouseOutEvent evt) => _hovering = false);
        _scrollButton.RegisterCallback((MouseDownEvent evt) => OnPressed());
        // _scrollButton.RegisterCallback((MouseDownEvent evt) => OnReleased());
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && _pressed)
        {
            OnReleased();
        }
    
        if (_pressed)
        {
            float value = Input.mousePosition.x - _horizontalOffset;
            float clampedValue = Mathf.Clamp(value, 0.0f, Screen.width * 0.95f);
            
            _scrollButton.style.left = clampedValue;
            
            onHeld.Invoke(clampedValue / (Screen.width * 0.95f));
        }
    }
    
    private void OnPressed()
    {
        if (_pressed) return;
        _pressed = true;
        
        Debug.Log("Pressed");
        Debug.Log($"{_scrollButton.resolvedStyle.left} left position of scroll");
        
        _horizontalOffset = Input.mousePosition.x - _scrollButton.resolvedStyle.left;
    }
    
    private void OnReleased()
    {
        if (!_pressed) return;
        _pressed = false;
        Debug.Log("Released");
    }
}
