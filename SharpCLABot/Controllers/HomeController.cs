// Copyright (c) 2014, Alexandre Mutel
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted 
// provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this list of conditions 
//    and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions
//    and the following disclaimer in the documentation and/or other materials provided with the 
//    distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
//  THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Octokit;
using SharpCLABot.Helpers;
using SharpCLABot.Templates;
using SharpCLABot.ViewModels;

namespace SharpCLABot.Controllers
{
    /// <summary>
    /// Home controller used to sign the CLA.
    /// </summary>
    public class HomeController : CLABotControllerBase
    {
        private readonly GitHubClient github;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController()
        {
            github = new GitHubClient(new ProductHeaderValue("SharpCLABot"), new Uri("https://github.com/"));
        }

        /// <summary>
        /// Main url for handling CLA signing
        /// </summary>
        /// <returns>Task{ActionResult}.</returns>
        public ActionResult Index()
        {
            // Redirect to the admin configuration if we are not yet configured.
            if (!AdminConfig.Instance.IsValid)
            {
                return RedirectToAdmin();
            }

            return View();
        }

        /// <summary>
        /// Main url for handling CLA signing
        /// </summary>
        /// <returns>Task{ActionResult}.</returns>
        public async Task<ActionResult> Sign()
        {
            // Redirect to the admin configuration if we are not yet configured.
            if (!AdminConfig.Instance.IsValid)
            {
                return RedirectToAdmin();
            }

            return await RequireOAuth(async () =>
            {
                // The following requests retrieves all of the user's repositories and
                // requires that the user be logged in to work.
                var currentUser = await github.User.Current();
                var emails = await github.User.Email.GetAll();

                var contributor = Db.GetOrCreateContributor(currentUser.Login);

                // Setup the default name from github
                if (string.IsNullOrWhiteSpace(contributor.FullName) && currentUser.Name != null)
                {
                    contributor.FullName = currentUser.Name;
                }

                // Setup the default location from github
                if (string.IsNullOrWhiteSpace(contributor.City) && currentUser.Location != null)
                {
                    contributor.City = currentUser.Location;
                }

                // Setup the default email from github
                var mainEmail = emails.FirstOrDefault(email => email.Primary);
                if (string.IsNullOrWhiteSpace(contributor.Email) && mainEmail != null)
                {
                    contributor.Email = mainEmail.Email;
                }

                if (!contributor.Accepted)
                {
                    contributor.SignatureDate = DateTime.Now;
                }

                // We are not validating on save here, but only on submit
                Db.SaveChanges();

                return View(new AgreementViewModel(contributor));
            });
        }

        public async Task<ActionResult> Submit(AgreementViewModel agreement)
        {
            // Redirect to the admin configuration if we are not yet configured.
            if (!AdminConfig.Instance.IsValid)
            {
                return RedirectToAdmin();
            }

            return await RequireOAuth(async () =>
            {
                // The following requests retrieves all of the user's repositories and
                // requires that the user be logged in to work.
                var currentUser = await github.User.Current();
                var contributor = Db.GetOrCreateContributor(currentUser.Login);

                if (ModelState.IsValid && !contributor.Accepted)
                {
                    var submiContributor = agreement.Contributor;
                    contributor.FullName = submiContributor.FullName;
                    contributor.Email = submiContributor.Email;
                    contributor.Address = submiContributor.Address;
                    contributor.ZipCode = submiContributor.ZipCode;
                    contributor.City = submiContributor.City;
                    contributor.Phone = submiContributor.Phone;
                    contributor.Accepted = true;
                    contributor.SignatureDate = submiContributor.SignatureDate;

                    Db.SaveChanges();

                    // Send a comment back to the client if it was originated from a pull request
                    if (contributor.IsFromPullRequest)
                    {
                        try
                        {
                            // Render the comment
                            var commentBody = Templatizer.RenderReplySignedDone(contributor.Id);

                            // Send back a comment to github pull request
                            var token = AdminConfig.Instance.GitHubAdminApplicationToken;
                            var githubClientOwner = GithubHelper.Connect(token);

                            // Post a comment to the contributor original pull request
                            var githubComment = await githubClientOwner.PostCommentToContributor(contributor, commentBody);

                            // Redirect to the original pull request and created github comment
                            return Redirect(githubComment.HtmlUrl.ToString());
                        }
                        catch (Exception ex)
                        {
                            // TODO log error, unable to send back a feedback to the user
                        }
                    }

                }
                return RedirectToAction("Sign");
            });
        }

        private ActionResult RedirectToAdmin()
        {
            return RedirectToAction("Index", "Admin");
        }

        private async Task<ActionResult> RequireOAuth(Func<Task<ActionResult>> action)
        {
            var accessToken = Session["OAuthToken"] as string;
            if (accessToken != null)
            {
                // This allows the client to make requests to the GitHub API on the user's behalf
                // without ever having the user's OAuth credentials.
                github.Credentials = new Credentials(accessToken);
            }
            try
            {
                return await action();
            }
            catch (AuthorizationException)
            {
                // Either the accessToken is null or it's invalid. This redirects
                // to the GitHub OAuth login page. That page will redirect back to the
                // Authorize action.
                return Redirect(GetOAuthLoginUrl());
            }
        }

        // This is the Callback URL that the GitHub OAuth Login page will redirect back to.
        public async Task<ActionResult> Authorize(string code, string state)
        {
            if (!String.IsNullOrEmpty(code))
            {
                var expectedState = Session["CSRF:State"] as string;
                if (state != expectedState) throw new InvalidOperationException("SECURITY FAIL!");
                Session["CSRF:State"] = null;

                var clientId = AdminConfig.Instance.GitHubApplicationClientId;
                var clientSecret = AdminConfig.Instance.GitHubApplicationClientSecret;

                var token = await github.Oauth.CreateAccessToken(new OauthTokenRequest(clientId, clientSecret, code));
                Session["OAuthToken"] = token.AccessToken;
            }
            return RedirectToAction("Sign");
        }

        private string GetOAuthLoginUrl()
        {
            var state = Membership.GeneratePassword(24, 1);
            Session["CSRF:State"] = state;
            // 1. Redirect users to request GitHub access
            var clientId = AdminConfig.Instance.GitHubApplicationClientId;
            var request = new OauthLoginRequest(clientId)
            {
                Scopes = { "user:email" },
                State = state
            };
            var oauthLoginUrl = github.Oauth.GetGitHubLoginUrl(request);
            return oauthLoginUrl.ToString();
        }
    }
}
