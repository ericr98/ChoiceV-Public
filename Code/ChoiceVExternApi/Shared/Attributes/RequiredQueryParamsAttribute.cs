using System.Net;
using System.Reflection;

namespace ChoiceVExternApi.Shared.Attributes;

/// <summary>
/// Attribute to specify required query parameters for a method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequiredQueryParamsAttribute(params string[] queryParams) : Attribute {
    public string[] QueryParams { get; set; } = queryParams;
}
/// <summary>
/// Provides extension methods for <see cref="RequiredQueryParamsAttribute"/>.
/// </summary>
public static class RequiredQueryParamsAttributeExtensions {
    /// <summary>
    /// Checks if the method decorated with <see cref="RequiredQueryParamsAttribute"/> has the required query parameters.
    /// </summary>
    /// <param name="handler">The handler function that requires the check.</param>
    /// <param name="context">The HTTP listener context containing query parameters.</param>
    /// <returns>True if all required query parameters are present; otherwise, false.</returns>
    public static bool checkRequiredQueryParamsAttribute(this Func<HttpListenerContext, string[], Task> handler, HttpListenerContext context) {
        var attributes = handler.GetMethodInfo().GetCustomAttributes(typeof(RequiredQueryParamsAttribute), false);
        if(attributes.Length != 1) return true;

        var attribute = (RequiredQueryParamsAttribute)attributes.First();

        if(context.isQueryNullOrEmpty()) return false;

        foreach(var queryParam in attribute.QueryParams) {
            if(context.getQueryParameter(queryParam) is null) {
                return false;
            }
        }

        return true;
    }
}
