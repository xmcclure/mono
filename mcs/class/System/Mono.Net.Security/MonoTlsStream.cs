//
// MonoTlsStream.cs
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

#if SECURITY_DEP && MONO_X509_ALIAS
extern alias PrebuiltSystem;
using X509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;

namespace Mono.Net.Security
{
	class MonoTlsStream
	{
		readonly IMonoTlsProvider provider;
		readonly HttpWebRequest request;
		readonly NetworkStream networkStream;

		IMonoSslStream sslStream;
		WebExceptionStatus status;
		bool hasCertificates;

		internal HttpWebRequest Request {
			get { return request; }
		}

		internal WebExceptionStatus ExceptionStatus {
			get { return status; }
		}

#if SECURITY_DEP
		ChainValidationHelper validationHelper;

		public MonoTlsStream (HttpWebRequest request, NetworkStream networkStream)
		{
			this.request = request;
			this.networkStream = networkStream;

			provider = request.TlsProvider ?? MonoTlsProviderFactory.GetProviderInternal ();
			status = WebExceptionStatus.SecureChannelFailure;

			validationHelper = new ChainValidationHelper (this);
		}

		internal X509Certificate SelectClientCertificate (string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			if (localCertificates == null || localCertificates.Count == 0)
				return null;
			return localCertificates [0];
		}

		internal bool CheckCertificates ()
		{
			if (hasCertificates)
				return true;
			if (sslStream == null)
				return false;

			var client = sslStream.LocalCertificate;
			var server = sslStream.RemoteCertificate;
			request.ServicePoint.SetCertificates (client, server);

			hasCertificates = true;
			return true;
		}

		internal Stream CreateStream (byte[] buffer)
		{
			sslStream = provider.CreateSslStream (networkStream, false, validationHelper);

			try {
				sslStream.AuthenticateAsClient (
					request.Address.Host, (X509CertificateCollection)(object)request.ClientCertificates,
					(SslProtocols)ServicePointManager.SecurityProtocol,
					ServicePointManager.CheckCertificateRevocationList);
			} catch {
				status = WebExceptionStatus.TrustFailure;
				sslStream = null;
				throw;
			}

			try {
				if (buffer != null)
					sslStream.Write (buffer, 0, buffer.Length);
			} catch {
				status = WebExceptionStatus.SendFailure;
				sslStream = null;
				throw;
			}

			return sslStream.AuthenticatedStream;
		}
#endif
	}
}
