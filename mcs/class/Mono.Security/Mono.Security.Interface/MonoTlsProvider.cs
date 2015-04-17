//
// MonoTlsProvider.cs
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

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;

namespace Mono.Security.Interface
{
	/*
	 * Unfortunately, we can't use the public definitions from System.dll here, so we need to
	 * copy these.
	 *
	 * The @MonoRemoteCertificateValidationCallback also has an additional 'targetHost' argument.
	 *
	 */

	[Flags]
	public enum MonoSslPolicyErrors
	{
		None = 0,
		RemoteCertificateNotAvailable = 1,
		RemoteCertificateNameMismatch = 2,
		RemoteCertificateChainErrors = 4,
	}

	public enum MonoEncryptionPolicy
	{
		// Prohibit null ciphers (current system defaults)
		RequireEncryption = 0,

		// Add null ciphers to current system defaults
		AllowNoEncryption,

		// Request null ciphers only
		NoEncryption
	}

	public delegate bool MonoRemoteCertificateValidationCallback (
		string targetHost, X509Certificate certificate, X509Chain chain, MonoSslPolicyErrors sslPolicyErrors);

	public delegate X509Certificate MonoLocalCertificateSelectionCallback (
		string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate,
		string[] acceptableIssuers);

	public abstract class MonoTlsProvider
	{
#region HttpsClientStream

		/*
		 * This section reflects how Mono's web stack currently accesses
		 * the TLS code.  Since that's currently using the obsolete @HttpsClientStream,
		 * this section needs to be cleaned up.  It is therefor marked as [Obsolete].
		 *
		 * It is only used in WebClient.cs.
		 *
		 */

		public abstract bool SupportsHttps {
			get;
		}

		#pragma warning disable 618
		public abstract bool IsHttpsStream (Stream stream);

		public abstract IMonoHttpsStream GetHttpsStream (Stream stream);

		public abstract IMonoHttpsStream CreateHttpsClientStream (
			Stream innerStream, HttpWebRequest request, byte[] buffer);
		#pragma warning restore

#endregion

#region SslStream

		/*
		 * This section abstracts the @SslStream class.
		 *
		 */

		public abstract bool SupportsSslStream {
			get;
		}

		/*
		 * Whether or not this TLS Provider supports Mono-specific extensions
		 * (via @MonoTlsSettings).
		 */
		public abstract bool SupportsMonoExtensions {
			get;
		}

		/*
		 * Obtain a @MonoSslStream instance.
		 *
		 * The 'settings' argument is ignored unless this provider 'SupportsMonoExtensions'.
		 */
		public abstract MonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			ICertificateValidator certificateValidator,
			MonoTlsSettings settings = null);

#endregion

#region Manged SSPI

		/*
		 * The managed SSPI implementation from the new TLS code.
		 */

		public abstract bool SupportsTlsContext {
			get;
		}

		public abstract IMonoTlsContext CreateTlsContext (
			string hostname, bool serverMode, TlsProtocols protocolFlags,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool remoteCertRequired, MonoEncryptionPolicy encryptionPolicy,
			ICertificateValidator certificateValidator,
			MonoTlsSettings settings);

#endregion
	}
}
