namespace PoAppIdea.Web.Infrastructure;

public class UIService
{
    private bool _isFocusMode;
    public bool IsFocusMode
    {
        get => _isFocusMode;
        private set
        {
            if (_isFocusMode != value)
            {
                _isFocusMode = value;
                OnFocusModeChanged?.Invoke(value);
            }
        }
    }

    public event Action<bool>? OnFocusModeChanged;

    public void SetFocusMode(bool enabled)
    {
        IsFocusMode = enabled;
    }

    public void ToggleFocusMode()
    {
        IsFocusMode = !IsFocusMode;
    }
}
