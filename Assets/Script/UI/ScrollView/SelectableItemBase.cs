public delegate void SelectedChangedDelegate(bool val);

public class SelectableItemBase
{
    public object content;
    public SelectedChangedDelegate selectedChanged;

    private bool _selected = false;
    public bool Selected
    {
        get { return _selected; }
        set
        {
            // if the value has changed
            if (_selected != value)
            {

                _selected = value;
                selectedChanged?.Invoke(_selected);
            }
        }
    }
}