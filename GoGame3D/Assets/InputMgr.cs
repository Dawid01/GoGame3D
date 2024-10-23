using System;
using UnityEngine;

public class InputMgr : Singleton<InputMgr>
{
    [HideInInspector] public bool isMobile;

    public bool InputDown { get; private set; }
    public bool InputUp { get; private set; }
    public bool IsDragging { get; private set; }
    public bool IsMultiInput { get; private set; }
    public float Zoom { get; private set; }

    public static Action OnLongTouch;
    private float _lastTouchTime;
    private const float LongTouchThreshold = 0.5f; 

    public override void Awake()
    {
        base.Awake();
        isMobile = Application.isMobilePlatform;
    }
    

    public Vector2 InputPosition
    {
        get
        {
            return isMobile ? (Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.one * -1000f) : Input.mousePosition;
        }
    }

    private void Update()
    {
        if (isMobile)
        {
            MobileInput();
        }
        else
        {
            PCInput();
        }
    }

    private void PCInput()
    {
        InputDown = Input.GetMouseButtonDown(0);
        InputUp = Input.GetMouseButtonUp(0);
        IsDragging = Input.GetMouseButton(1);
    }

    private void MobileInput()
    {
        IsMultiInput = Input.touchCount > 1;

        if (Input.touchCount == 1 && !IsMultiInput)
        {
            InputDown = Input.GetTouch(0).phase == TouchPhase.Began;
            InputUp = Input.GetTouch(0).phase == TouchPhase.Ended;

            if (Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                _lastTouchTime += Time.deltaTime;
                if (_lastTouchTime >= LongTouchThreshold)
                {
                    OnLongTouch?.Invoke();
                }
            }
            else
            {
                _lastTouchTime = 0; 
            }

            Zoom = 0; 
        }
        else if (IsMultiInput)
        {
            float pinchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            Zoom = pinchDistance;
            //IsDragging = Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary;
            IsDragging = true;
        }
        else
        {
            InputDown = false;
            InputUp = false;
            _lastTouchTime = 0;
            Zoom = 0;
            IsDragging = false;
        }
    }

}
