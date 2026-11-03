namespace ButterMorph.Web.Razor;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

// Resolves a stable per-browser designer session key.
internal static class DesignerSessionKeyResolver
{
    private const string EditorSessionItemKey = "ButterMorph.EditorSessionId";
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

    // Resolves an opaque per-editor session identifier. A context is deliberately not used as
    // the storage key because two tabs editing the same host document must remain isolated.
    internal static string ResolveEditorSessionId(PageModel pageModel, ButterMorphRazorDesignerOptions options)
    {
        if (pageModel.HttpContext.Items.TryGetValue(EditorSessionItemKey, out object cached) && cached is string cachedSessionId && !string.IsNullOrWhiteSpace(cachedSessionId))
        {
            return cachedSessionId;
        }

        if (pageModel.Request.Query.TryGetValue(options.Sessions.SessionQueryParameter, out StringValues values))
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    pageModel.HttpContext.Items[EditorSessionItemKey] = value;
                    return value;
                }
            }
        }

        string sessionId = ResolveCookieKey(pageModel);
        pageModel.HttpContext.Items[EditorSessionItemKey] = sessionId;
        return sessionId;
    }

    // Creates the URL for the first request to a host-owned context. The redirect makes the
    // opaque session id durable in the tab URL while the cookie keeps legacy form posts working.
    internal static bool TryCreateEditorSessionRedirect(PageModel pageModel, ButterMorphRazorDesignerOptions options, out string url)
    {
        url = string.Empty;
        if (string.IsNullOrWhiteSpace(ResolveContextKey(pageModel, options)) || pageModel.Request.Query.ContainsKey(options.Sessions.SessionQueryParameter))
        {
            return false;
        }

        string sessionId = Guid.NewGuid().ToString("N");
        pageModel.Response.Cookies.Append(DesignerSessionKeys.CookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

        QueryString query = pageModel.Request.QueryString.Add(options.Sessions.SessionQueryParameter, sessionId);
        url = pageModel.Request.PathBase + pageModel.Request.Path + query;
        return true;
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
