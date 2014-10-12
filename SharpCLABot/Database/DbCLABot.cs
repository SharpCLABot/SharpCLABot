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
namespace SharpCLABot.Database
{
    using System;
    using System.Data.Entity;

    /// <summary>
    /// Main db API.
    /// </summary>
    public class DbCLABot : DbContext
    {
        public const string DefaultKey = "DbCLABot";

        // Your context has been configured to use a 'CLAModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'SharpCLABot.Database.CLAModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'CLAModel' 
        // connection string in the application configuration file.
        public DbCLABot()
            : base("name="+ DefaultKey)
        {
        }

        /// <summary>
        /// Contributors stored in the database.
        /// </summary>
        public virtual DbSet<Contributor> Contributors { get; set; }

        /// <summary>
        /// Gets or create a contributor.
        /// </summary>
        /// <param name="name">The contributor name.</param>
        /// <returns>Contributor.</returns>
        /// <exception cref="ArgumentNullException">name</exception>
        public Contributor GetOrCreateContributor(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            var contributor = Contributors.Find(name);
            if (contributor == null)
            {
                contributor = new Contributor() { Id = name, SignatureDate = DateTime.Now };
                Contributors.Add(contributor);
            }
            return contributor;
        }
    }
}