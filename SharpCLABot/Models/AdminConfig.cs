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
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml.Serialization;
using SharpCLABot.Database;

namespace SharpCLABot
{
    [XmlRoot("AdminConfig", Namespace = NS)]
    public sealed class AdminConfig : IDisposable
    {
        internal const string NS = "urn:SharpCLABot.AdminConfig";

        private const string DefaultFile = "AdminConfig.xml";
        private const string ReplyNotSignedFile = @"Templates\ReplyNotSigned.md";
        private const string ReplySignedDoneFile = @"Templates\ReplySignedDone.md";
        private const string IndividualCLAFile = @"Views\Home\IndividualCLA.cshtml";
        private const string InformationUsFile = @"Views\Home\InformationUs.cshtml";
        private static readonly object SaveLocker = new object();

        private readonly CountdownEvent countdownHookEvent;

        public const string DefaultLocalDbConnectionString = @"data source=(LocalDb)\v11.0;initial catalog=SharpCLABot.Database.DbCLABot;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

        /// <summary>
        /// The default instance.
        /// </summary>
        public static AdminConfig Instance { get; private set; }

        static AdminConfig()
        {
            Instance = Load();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminConfig"/> class.
        /// </summary>
        public AdminConfig()
        {
            Hooks = new List<GitHubRepositoryHook>();
            countdownHookEvent = new CountdownEvent(0);
        }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a project name")]
        public string ProjectName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a project url")]
        [DataType(DataType.Url)]
        public string ProjectUrl { get; set; }

        [DataType(DataType.Url)]
        public string GitHubWebHookCallbackUrl { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a GitHub WebHook Callback Secret")]
        [DataType(DataType.Password)]
        public string GitHubWebHookCallbackSecret { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a GitHub Admin Application Token")]
        [DataType(DataType.Password)]
        public string GitHubAdminApplicationToken { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid client id")]
        [DataType(DataType.Password)]
        public string GitHubApplicationClientId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid client secret")]
        [DataType(DataType.Password)]
        public string GitHubApplicationClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the text used to post a comment for a user that hasn't yet signed the CLA.
        /// </summary>
        /// <value>The text used to post a comment for a user that hasn't yet signed the CLA.</value>
        [XmlIgnore]
        [DataType(DataType.Text)]
        [AllowHtml]
        public string ReplyCLANotSigned { get; set; }

        /// <summary>
        /// Gets or sets the text used to post a comment for a user that has just signed the CLA.
        /// </summary>
        /// <value>The text used to post a comment for a user that has just signed the CLA.</value>
        [XmlIgnore]
        [DataType(DataType.Text)]
        [AllowHtml]
        public string ReplyCLASignedDone { get; set; }

        /// <summary>
        /// Gets or sets the HTML license for an individual.
        /// </summary>
        /// <value>The HTML license for an individual.</value>
        [XmlIgnore]
        [DataType(DataType.Html)]
        [AllowHtml]
        public string IndividualCLA { get; set; }

        /// <summary>
        /// Gets or sets the HTML for the Us Information.
        /// </summary>
        /// <value>The HTML for the Us Information.</value>
        [XmlIgnore]
        [DataType(DataType.Html)]
        [AllowHtml]
        public string InformationUs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is configured.
        /// </summary>
        /// <value><c>null</c> if [is configured] contains no value, <c>true</c> if [is configured]; otherwise, <c>false</c>.</value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the github repositories that are watch by SharpCLABot.
        /// </summary>
        /// <value>The repositories.</value>
        public List<GitHubRepositoryHook> Hooks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use a localdb instance.
        /// </summary>
        /// <value><c>true</c> if [local database]; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool IsLocalDb { get; set; }

        /// <summary>
        /// Gets or sets the connection string database.
        /// </summary>
        /// <value>The connection string database.</value>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid db connection string")]
        public string ConnectionStringDb { get; set; }

        internal void BeginHookUpdate()
        {
            try
            {
                countdownHookEvent.Reset();
            }
            catch (Exception)
            {
            }
        }

        internal void AddHookRequest()
        {
            try
            {
                countdownHookEvent.AddCount();
            }
            catch (Exception)
            {
            }
        }

        internal void EndHookUpdate()
        {
            try
            {
                countdownHookEvent.Wait(500);
            }
            catch (Exception)
            {
            }
        }

        internal void NotifyHookUpdated()
        {
            if (countdownHookEvent.CurrentCount > 0)
            {
                try
                {
                    countdownHookEvent.Signal();
                }
                catch (Exception)
                {
                }
            }
        }

        public bool IsTokenConfigured
        {
            get { return !string.IsNullOrWhiteSpace(GitHubAdminApplicationToken); }
        }

        /// <summary>
        /// Loads the default settings for this instance.
        /// </summary>
        public static AdminConfig Load()
        {
            var deserializer = new XmlSerializer(typeof(AdminConfig));
            var config = new AdminConfig();
            try
            {
                var file = Path.Combine(HttpRuntime.AppDomainAppPath, DefaultFile);
                if (File.Exists(file))
                {
                    config = (AdminConfig) deserializer.Deserialize(new StringReader(File.ReadAllText(file)));
                }
            }
            catch (Exception ex)
            {
                // TODO log an exception if we really can't load it
            }

            try
            {
                LoadFile(ReplyNotSignedFile, s => config.ReplyCLANotSigned = s);
                LoadFile(ReplySignedDoneFile, s => config.ReplyCLASignedDone = s);
                LoadFile(IndividualCLAFile, s => config.IndividualCLA = s);
                LoadFile(InformationUsFile, s => config.InformationUs = s);
            }
            catch (Exception)
            {
                // TODO log an exception if we really can't load it
            }

            // Check if database connection is LocalDb
            if (string.IsNullOrWhiteSpace(config.ConnectionStringDb))
            {
                config.ConnectionStringDb = DefaultLocalDbConnectionString;
            }
            config.IsLocalDb = config.ConnectionStringDb == DefaultLocalDbConnectionString;

            // Make sure we have a project name
            config.ProjectName = config.ProjectName ?? "SharpCLABot";

            return config;
        }

        public void Save()
        {
            lock (SaveLocker)
            {
                var serialzier = new XmlSerializer(typeof (AdminConfig));
                var file = Path.Combine(HttpRuntime.AppDomainAppPath, DefaultFile);
                using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    serialzier.Serialize(stream, this);
                }

                WriteFile(ReplyNotSignedFile, () => ReplyCLANotSigned);
                WriteFile(ReplySignedDoneFile, () => ReplyCLASignedDone);
                WriteFile(IndividualCLAFile, () => IndividualCLA);
                WriteFile(InformationUsFile, () => InformationUs);

                // Save the current new instance
                Instance = this;
            }
        }

        private static void LoadFile(string file, Action<string> set)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (set == null) throw new ArgumentNullException("set");
            try
            {
                set(File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, file)));
            }
            catch (Exception)
            {
                // TODO log an exception if we really can't load it
            }
        }

        private static void WriteFile(string file, Func<string> get)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (get == null) throw new ArgumentNullException("get");
            var fullpath = Path.Combine(HttpRuntime.AppDomainAppPath, file);
            string previousContent = null;
            if (File.Exists(fullpath))
            {
                previousContent = File.ReadAllText(fullpath);
            }

            var newContent = get() ?? string.Empty;

            // Only write when content has changed
            if (previousContent != newContent)
            {
                File.WriteAllText(fullpath, newContent, Encoding.UTF8);
            }
        }

        public void Dispose()
        {
            if (countdownHookEvent != null)
            {
                countdownHookEvent.Dispose();
            }
        }
    }
}