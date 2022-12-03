using System.Reflection;

namespace WinForms.DataVisualization.Designer.Client
{
    internal static class Extensions
    {
        public static object? GetPropValue(this object src, string propName)
        {
            return src?.GetType().GetRuntimeProperty(propName)?.GetValue(src);
        }
    }
}
