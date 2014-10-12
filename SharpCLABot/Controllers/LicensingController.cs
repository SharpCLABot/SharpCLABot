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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Xml.XPath;
using Newtonsoft.Json;
using Octokit;
using SharpCLABot.Database;
using SharpCLABot.Helpers;
using SharpCLABot.Templates;

namespace SharpCLABot.Controllers
{
    public class LicensingController : ApiController
    {
        private readonly DbCLABot dbCLA;

        public LicensingController()
        {
            dbCLA = new DbCLABot();
        }

        public static string GetDefaultWebHookCallback(Uri uri)
        {
            return
                new Uri(new Uri(uri.GetLeftPart(UriPartial.Authority)), new Uri("api/licensing/hook", UriKind.Relative)).ToString();
        }

        // POST api/pullrequest
        [HttpPost]
        public async Task<HttpResponseMessage> Hook()
        {
            // Verify the request and get the json content
            var json = await CheckCallbackAndGetJson();

            // Decode the payload
            PingRequest pingRequest;
            PullRequestPayload pullRequest;
            DecodePayload(json, out pullRequest, out pingRequest);

            try
            {
                // Handle only opened and reopened pull requests
                if (pullRequest != null && (string.CompareOrdinal(pullRequest.Action, "opened") == 0 ||
                                            string.CompareOrdinal(pullRequest.Action, "reopened") == 0))
                {
                    await HandlePullRequest(pullRequest);

                } else if (pingRequest != null)
                {
                    HandlePingRequest(pingRequest);
                }
            }
            catch (Exception ex)
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                throw new HttpResponseException(response);
            }

            // Return a NoContent response
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        private async Task HandlePullRequest(PullRequestPayload pullRequest)
        {
            // Check for the login name
            var loginName = pullRequest.UserName;
            var contributor = dbCLA.GetOrCreateContributor(loginName);

            // If the contributor has already accepted the CLA, then we can return immediately
            if (!contributor.Accepted)
            {
                // Create a new auto-response comment on the original pull-request asking for signing the CLA
                contributor.PullRequestRepositoryOwner = pullRequest.RepoOwner;
                contributor.PullRequestRepositoryName = pullRequest.RepoName;
                contributor.PullRequestNumber = pullRequest.Number;
                dbCLA.SaveChanges();

                try
                {
                    // Render the comment
                    var homeSignCLA = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                    var commentBody = Templatizer.RenderReplyNotSigned(pullRequest.UserName, homeSignCLA);

                    // Send back a comment to github pull request
                    var token = AdminConfig.Instance.GitHubAdminApplicationToken;
                    var githubClientOwner = GithubHelper.Connect(token);

                    // Post a comment to the contributor original pull request
                    await githubClientOwner.PostCommentToContributor(contributor, commentBody);
                }
                catch (Exception ex)
                {
                    var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
                    throw new HttpResponseException(response);
                }
            }
        }

        private static void HandlePingRequest(PingRequest pingRequest)
        {
            var config = AdminConfig.Instance;
            foreach (var hook in AdminConfig.Instance.Hooks)
            {
                if (hook.Owner == pingRequest.RepoOwner && hook.Name == pingRequest.RepoName)
                {
                    hook.Validated = true;
                    // Cheap/unreliable way to notify for ping when updating admin config
                    config.NotifyHookUpdated();
                    break;
                }
            }
        }

        private async Task<string> CheckCallbackAndGetJson()
        {
            // Check that media type is valid
            var mediaType = Request.Content.Headers.ContentType.MediaType;
            if (mediaType != "application/json")
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType,
                    string.Format("Unsupported [{0}] content type", mediaType));
                throw new HttpResponseException(response);
            }

            // Check for X-Github-Event and pull_request
            string githubEvent;
            if (!Request.Headers.TryGetSingleValue("X-Github-Event", out githubEvent))
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    string.Format("Invalid X-Github-Event [{0}] header. Expecting only pull_request", githubEvent));
                throw new HttpResponseException(response);
            }

            // Handle only pull_request event
            var isPullRequestEvent = string.CompareOrdinal(githubEvent, "pull_request") != 0;
            var isPingEvent = string.CompareOrdinal(githubEvent, "ping") != 0;

            if (!isPullRequestEvent && !isPingEvent)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NoContent));
            }

            // Read the content
            var json = await Request.Content.ReadAsStringAsync();

            string xhubSignature;
            if (Request.Headers.TryGetSingleValue("X-Hub-Signature", out xhubSignature) && xhubSignature.StartsWith("sha1="))
            {
                string secret = AdminConfig.Instance.GitHubWebHookCallbackSecret;
                if (secret != null)
                {
                    var sha1 = "sha1=" + ComputeHMACSignature(json, secret);
                    if (string.CompareOrdinal(xhubSignature, sha1) != 0)
                    {
                        var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format("Invalid X-Hub-Signature [{0}]", xhubSignature));
                        throw new HttpResponseException(response);
                    }
                }
            }

            return json;
        }

        private void DecodePayload(string json, out PullRequestPayload pullRequest, out PingRequest pingRequest)
        {
            pullRequest = null;
            pingRequest = null;

            try
            {
                // Don't use octokit but directly JSon.NET in order to avoid deserialization issues
                //var serializer = new SimpleJsonSerializer();
                //payload = serializer.Deserialize<DecodePayload>(json);

                var document = JsonConvert.DeserializeXNode(json, "payload");

                if (document.XPathSelectElement("payload/pull_request") != null)
                {
                    pullRequest = new PullRequestPayload
                    {
                        Action = document.XPathSelectElement("payload/action").Value,
                        Number = int.Parse(document.XPathSelectElement("payload/number").Value),
                        UserName = document.XPathSelectElement("payload/pull_request/user/login").Value,
                        RepoName = document.XPathSelectElement("payload/repository/name").Value,
                        RepoOwner = document.XPathSelectElement("payload/repository/owner/login").Value
                    };
                }
                else
                {
                    pingRequest = new PingRequest()
                    {
                        HookId = int.Parse(document.XPathSelectElement("payload/hook_id").Value),
                        RepoName = document.XPathSelectElement("payload/repository/name").Value,
                        RepoOwner = document.XPathSelectElement("payload/repository/owner/login").Value
                    };
                }
            }
            catch (Exception ex)
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
                throw new HttpResponseException(response);
            }
        }

        private static string ComputeHMACSignature(string content, string secret)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(secret)))
            {
                hmac.Initialize();
                return BitConverter.ToString(hmac.ComputeHash(Encoding.ASCII.GetBytes(content))).Replace("-", "").ToLower();
            }
        }

        private class PullRequestPayload
        {
            public string Action { get; set; }

            public int Number { get; set; }

            public string UserName { get; set; }

            public string RepoOwner { get; set; }

            public string RepoName { get; set; }
        }

        private class PingRequest
        {
            public int HookId { get; set; }

            public string RepoOwner { get; set; }

            public string RepoName { get; set; }
        }
    }
}
