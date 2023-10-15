using Grooper.CMIS;
using System.ComponentModel;
using System.Runtime.Serialization;

#pragma warning disable CS1591 
namespace GrooperGit
{
    ///<summary>Implements <a target='_blank' href='https://en.wikipedia.org/wiki/OAuth'>OAuth 2.0</a> authentication as defined in 
    ///   <a target='_blank' href='https://tools.ietf.org/html/rfc6749'>RFC-6749</a> against the Microsoft OAuth servers at https://login.microsoftonline.com/common/oauth2.</summary>
    [DataContract, DisplayName("GitHub OAuth")]
    public abstract class GitHubOauth : OAuthAuthentication
    {
        protected override string AuthBaseURL => "https:api.github.com/user";
        protected override string RedirectURL => "https:api.github.com/user";
        protected override string ClientID => "";
    }
}
