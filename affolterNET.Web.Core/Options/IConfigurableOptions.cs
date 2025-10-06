using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Core.Options;

public interface IConfigurableOptions<T> where T : class
{
    static abstract string SectionName { get; }
    static abstract T CreateDefaults(AppSettings settings);
    abstract void CopyTo(T options);
}
