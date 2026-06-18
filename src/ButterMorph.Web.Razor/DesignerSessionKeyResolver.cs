namespace ButterMorph.Web.Razor;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

// Resolves a stable per-browser designer session key.
internal static class DesignerSessionKeyResolver
{
    internal static string Resolve(PageModel pageModel)
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
