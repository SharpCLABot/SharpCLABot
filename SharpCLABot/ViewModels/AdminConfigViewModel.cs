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

namespace SharpCLABot.ViewModels
{
    /// <summary>
    /// View model for the admin config panel.
    /// </summary>
    public class AdminConfigViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminConfigViewModel"/> class.
        /// </summary>
        public AdminConfigViewModel()
        {
            Contributors = new List<ContributorViewModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminConfigViewModel"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">config</exception>
        public AdminConfigViewModel(AdminConfig config) : this()
        {
            if (config == null) throw new ArgumentNullException("config");
            Config = config;
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        /// TODO: Too lazy to make a full view model of AdminConfig
        [Required]
        public AdminConfig Config { get; set; }

        /// <summary>
        /// Gets the contributors.
        /// </summary>
        /// <value>The contributors.</value>
        public List<ContributorViewModel> Contributors { get; private set; }
    }
}