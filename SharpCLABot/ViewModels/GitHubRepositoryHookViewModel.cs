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
namespace SharpCLABot.ViewModels
{
    /// <summary>
    /// View model for the webhook that are registered.
    /// </summary>
    public class GitHubRepositoryHookViewModel
    {
        /// <summary>
        /// Gets or sets the owner of the repository.
        /// </summary>
        /// <value>The owner of the repository.</value>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the name of the repository.
        /// </summary>
        /// <value>The name of the repository.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this hook is installed.
        /// </summary>
        /// <value><c>true</c> if this instance is hook installed; otherwise, <c>false</c>.</value>
        public bool Installed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the hook is validated.
        /// </summary>
        /// <value><c>true</c> if this instance is hook installed; otherwise, <c>false</c>.</value>
        public bool Validated { get; set; }
    }
}