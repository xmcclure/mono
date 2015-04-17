//
// CertificateValidationHelper.cs
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
using System.Threading;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;
using MX = Mono.Security.X509;
#if MOBILE
using Mono.Net.Security;
#endif

namespace Mono.Security.Interface
{
	public class ValidationResult
	{
		bool trusted;
		bool user_denied;
		int error_code;
		MonoSslPolicyErrors policy_errors;

		public ValidationResult (bool trusted, bool user_denied, int error_code, MonoSslPolicyErrors policy_errors)
		{
			this.trusted = trusted;
			this.user_denied = user_denied;
			this.error_code = error_code;
			this.policy_errors = policy_errors;
		}

		public bool Trusted {
			get { return trusted; }
		}

		public bool UserDenied {
			get { return user_denied; }
		}

		public int ErrorCode {
			get { return error_code; }
		}

		public MonoSslPolicyErrors PolicyErrors {
			get { return policy_errors; }
		}
	}

	/**
	 * Internal interface - do not implement
	 */
	public interface ICertificateValidator
	{
		CertificateValidationHelper ValidationHelper {
			get;
		}

		MonoTlsSettings Settings {
			get;
		}

		X509Certificate SelectClientCertificate (
			string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate,
			string[] acceptableIssuers);

		ValidationResult ValidateChain (string targetHost, MX.X509CertificateCollection certificates);
	}

	public class CertificateValidationHelper
	{
		public CertificateValidationHelper (MonoTlsSettings settings)
		{
			Settings = settings;
		}

		public MonoTlsSettings Settings {
			get;
			private set;
		}

		/**
		 * Internal API.
		 */
		public CertificateValidationHelper (ICertificateValidator validator)
		{
			this.validator = validator;
			Settings = validator.Settings;
		}

		ICertificateValidator validator;

		#if !INSIDE_SYSTEM
		const string InternalHelperTypeName = "Mono.Net.Security.ChainValidationHelper";
		static readonly Type internalHelperType;
		static readonly MethodInfo createMethod;
		#endif

		static CertificateValidationHelper ()
		{
			#if !INSIDE_SYSTEM
			internalHelperType = Type.GetType (InternalHelperTypeName + ", " + Consts.AssemblySystem, true);
			if (internalHelperType == null)
				throw new NotSupportedException ();
			createMethod = internalHelperType.GetMethod ("Create", BindingFlags.Static | BindingFlags.NonPublic);
			if (createMethod == null)
				throw new NotSupportedException ();
			#endif
		}

		public ICertificateValidator CertificateValidator {
			get {
				if (validator == null)
					Interlocked.CompareExchange<ICertificateValidator> (ref validator, CreateValidator (), null);
				return validator;
			}
		}

		ICertificateValidator CreateValidator ()
		{
			#if INSIDE_SYSTEM
			return ChainValidationHelper.Create (this);
			#else
			return (ICertificateValidator)createMethod.Invoke (null, new object[] { this });
			#endif
		}

		public ValidationResult ValidateChain (string targetHost, MX.X509CertificateCollection certs)
		{
			return CertificateValidator.ValidateChain (targetHost, certs);
		}
	}
}
