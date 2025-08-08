using System.Net;
using System.Reflection;

namespace ChoiceVExternApi.Shared.Attributes;

/// <summary>
/// Attribute to indicate that a method should not accept any query parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NoQueryParamsAttribute : Attribute;
/// <summary>
/// Provides extension methods for <see cref="NoQueryParamsAttribute"/>.
/// </summary>
public static class NoQueryParamsAttributeExtensions {
    /// <summary>
    /// Checks if the method decorated with <see cref="NoQueryParamsAttribute"/> has any query parameters.
    /// </summary>
    /// <param name="handler">The handler function that requires the check.</param>
    /// <param name="context">The HTTP listener context containing query parameters.</param>
    /// <returns>True if no query parameters are allowed or present; otherwise, false.</returns>
    public static bool checkNoQueryParamsAttribute(this Func<HttpListenerContext, string[], Task> handler, HttpListenerContext context) {
        var hasNoQueryParamsAttribute = handler.GetMethodInfo().GetCustomAttributes(typeof(NoQueryParamsAttribute), false).Length > 0;

        if(!hasNoQueryParamsAttribute) return true;

        return context.isQueryNullOrEmpty();
    }
}
