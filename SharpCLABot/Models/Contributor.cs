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
using System.ComponentModel.DataAnnotations;

namespace SharpCLABot
{
    /// <summary>
    /// A Contributor.
    /// </summary>
    public sealed class Contributor : IContributor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contributor"/> class.
        /// </summary>
        public Contributor()
        {
        }

        /// <summary>
        /// Gets or sets the contributor identifier. This is the github login name.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [MaxLength(255)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Contributor"/> has accepted the license.
        /// </summary>
        /// <value><c>true</c> if accepted; otherwise, <c>false</c>.</value>
        public bool Accepted { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        [MaxLength(255)]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        /// http://stackoverflow.com/questions/386294/what-is-the-maximum-length-of-a-valid-email-address
        [MaxLength(254)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>The address.</value>
        [MaxLength(255)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>The zip code.</value>
        [MaxLength(255)]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        [MaxLength(255)]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        [MaxLength(255)]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the pull request repository owner that was originated. null if no pull-request associated to it.
        /// </summary>
        /// <value>The name repository originating the pull request.</value>
        [MaxLength(255)]
        public string PullRequestRepositoryOwner { get; set; }

        /// <summary>
        /// Gets or sets the pull request repository name that was originated. null if no pull-request associated to it.
        /// </summary>
        /// <value>The name repository originating the pull request.</value>
        [MaxLength(255)]
        public string PullRequestRepositoryName { get; set; }

        /// <summary>
        /// Gets or sets the pull request number that was originated. 0 If no pull-request associated to it.
        /// </summary>
        /// <value>The pull request number.</value>
        public int PullRequestNumber { get; set; }


        /// <summary>
        /// Gets a value indicating whether this coordinator is originated from a pull request.
        /// </summary>
        /// <value><c>true</c> if this coordinator is originated from a pull request; otherwise, <c>false</c>.</value>
        public bool IsFromPullRequest
        {
            get
            {
                return !string.IsNullOrWhiteSpace(PullRequestRepositoryOwner)
                       && !string.IsNullOrWhiteSpace(PullRequestRepositoryName) && PullRequestNumber > 0;
            }
        }

        /// <summary>
        /// Gets or sets the last update time.
        /// </summary>
        /// <value>The last update.</value>
        public DateTime SignatureDate { get; set; }

        public Contributor Clone()
        {
            return (Contributor) MemberwiseClone();
        }
    }
}