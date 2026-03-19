
public class ItemInstance
{
    private readonly SimpleItem _data;

    public ItemInstance(SimpleItem data)
    {
        _data = data ?? throw new System.ArgumentNullException(nameof(data));
    }
    
    public SimpleItem GetData() => _data;
    
    public T GetData<T>() where T : SimpleItem
    {
        return _data as T;
    }
    
    public bool IsType<T>() where T : SimpleItem => _data is T;
}