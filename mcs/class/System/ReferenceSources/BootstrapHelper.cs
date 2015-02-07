//
// BootstrapHelper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if !SECURITY_DEP
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;

//
// This file is only used during the bootstrap build, to bring a few
// APIs that will be used from the referencesource during the full
// build step.
//

namespace System.Net.Security
{
	[Flags]
	public enum SslPolicyErrors
	{
		None = 0x0,
		RemoteCertificateNotAvailable = 0x1,
		RemoteCertificateNameMismatch = 0x2,
		RemoteCertificateChainErrors = 0x4
	}

	public enum EncryptionPolicy
	{
		// Prohibit null ciphers (current system defaults)
		RequireEncryption = 0,

		// Add null ciphers to current system defaults
		AllowNoEncryption,

		// Request null ciphers only
		NoEncryption
	}

	public delegate bool RemoteCertificateValidationCallback (
		object sender,
		X509Certificate certificate,
		X509Chain chain,
		SslPolicyErrors sslPolicyErrors);
}
#endif

