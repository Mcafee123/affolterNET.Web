namespace affolterNET.Web.Core.Options;

public interface IConfigurableOptions<in T> where T : class
{
    static abstract string SectionName { get; }
    static abstract Action<T>? GetConfigureAction();
    void CopyTo(T target);
}