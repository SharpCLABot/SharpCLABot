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
    /// Connection status.
    /// </summary>
    public class DbConnectionStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionStatus"/> class.
        /// </summary>
        public DbConnectionStatus()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionStatus"/> class.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        public DbConnectionStatus(bool isValid)
        {
            IsValid = isValid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionStatus"/> class.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="message">The message.</param>
        public DbConnectionStatus(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is valid.
        /// </summary>
        /// <value><c>true</c> if the connection is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the message indicating wether the connection is valid.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
    }
}