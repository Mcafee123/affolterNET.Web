namespace affolterNET.Web.Core.Models;

public class ConfigureActions
{
    public Dictionary<Type, List<Delegate>> Actions { get; } = new();
    
    public void Add<T>(Action<T>? action)
    {
        if (action == null)
        {
            return;
        }

        if (!Actions.ContainsKey(typeof(T)))
        {
            Actions[typeof(T)] = new List<Delegate>();
        }
        Actions[typeof(T)].Add(action);
    }
    
    public IEnumerable<Delegate> GetActions<T>()
    {
        if (Actions.TryGetValue(typeof(T), out var list))
        {
            return list;
        }
        return [];
    }
}