namespace affolterNET.Web.Core.Options;

public interface IConfigurableOptions<T> where T : class
{
    static abstract string SectionName { get; }
    static abstract T CreateDefaults(bool isDev);
    abstract void CopyTo(T options);
}