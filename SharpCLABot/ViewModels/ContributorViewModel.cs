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

namespace SharpCLABot.ViewModels
{
    /// <summary>
    /// The view model of <see cref="Contributor"/>
    /// </summary>
    /// <remarks>
    /// The view model is identical to the model, except that it doesn't allow empty strings.
    /// </remarks>
    public sealed class ContributorViewModel : IContributor
    {
        private readonly Contributor contributor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Contributor"/> class.
        /// </summary>
        public ContributorViewModel()
        {
            contributor = new Contributor()
            {
                FullName = string.Empty,
                Email = string.Empty,
                Address = string.Empty,
                ZipCode = string.Empty,
                City = string.Empty,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContributorViewModel"/> class.
        /// </summary>
        /// <param name="contributor">The contributor.</param>
        public ContributorViewModel(Contributor contributor)
        {
            if (contributor == null) throw new ArgumentNullException("contributor");
            this.contributor = contributor.Clone();
        }

        [MaxLength(255)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid id")]
        public string Id
        {
            get { return contributor.Id; }
            set { contributor.Id = value; }
        }

        public bool Accepted
        {
            get { return contributor.Accepted; }
            set { contributor.Accepted = value; }
        }

        [MaxLength(255)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a name")]
        public string FullName
        {
            get { return contributor.FullName; }
            set { contributor.FullName = value; }
        }

        [MaxLength(254)]
        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter an email")]
        public string Email
        {
            get { return contributor.Email; }
            set { contributor.Email = value; }
        }

        [MaxLength(255)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter an address")]
        public string Address
        {
            get { return contributor.Address; }
            set { contributor.Address = value; }
        }

        [MaxLength(255)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a zipcode")]
        public string ZipCode
        {
            get { return contributor.ZipCode; }
            set { contributor.ZipCode = value; }
        }

        [MaxLength(255)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a city")]
        public string City
        {
            get { return contributor.City; }
            set { contributor.City = value; }
        }

        [MaxLength(255)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a phone")]
        public string Phone
        {
            get { return contributor.Phone; }
            set { contributor.Phone = value; }
        }

        public string PullRequestRepositoryOwner
        {
            get { return contributor.PullRequestRepositoryOwner; }
            set { contributor.PullRequestRepositoryOwner = value; }
        }

        public string PullRequestRepositoryName
        {
            get { return contributor.PullRequestRepositoryName; }
            set { contributor.PullRequestRepositoryName = value; }
        }

        public int PullRequestNumber
        {
            get { return contributor.PullRequestNumber; }
            set { contributor.PullRequestNumber = value; }
        }

        public DateTime SignatureDate
        {
            get { return contributor.SignatureDate; }
            set { contributor.SignatureDate = value; }
        }
    }
}