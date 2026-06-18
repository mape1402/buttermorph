namespace ButterMorph.Web.Razor;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

// Resolves a stable per-browser designer session key.
internal static class DesignerSessionKeyResolver
{
    internal static string Resolve(PageModel pageModel)
    {
        return Resolve(pageModel, new ButterMorphRazorDesignerOptions());
    }

    internal static string Resolve(PageModel pageModel, ButterMorphRazorDesignerOptions options)
    {
        string contextKey = ResolveContextKey(pageModel, options);

        if (!string.IsNullOrWhiteSpace(contextKey))
        {
            return "context:" + contextKey;
        }

        return ResolveCookieKey(pageModel);
    }

    internal static string ResolveContextKey(PageModel pageModel, ButterMorphRazorDesignerOptions options)
    {
        if (pageModel.Request.Query.TryGetValue(options.ContextQueryParameter, out StringValues values))
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return string.Empty;
    }

    private static string ResolveCookieKey(PageModel pageModel)
    {
        if (pageModel.Request.Cookies.TryGetValue(DesignerSessionKeys.CookieName, out string sessionKey) && !string.IsNullOrWhiteSpace(sessionKey))
        {
            return sessionKey;
        }

        sessionKey = Guid.NewGuid().ToString("N");
        pageModel.Response.Cookies.Append(DesignerSessionKeys.CookieName, sessionKey, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

        return sessionKey;
    }
}
