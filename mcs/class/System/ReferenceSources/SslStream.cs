//
// Mono-specific additions to Microsoft's SslStream.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
using Mono.Net.Security;

namespace System.Net.Security
{
	using System.Net.Sockets;
	using System.IO;

	partial class SslStream
	{
		#if SECURITY_DEP
		SSPIConfiguration _Configuration;

		internal SslStream (Stream innerStream, bool leaveInnerStreamOpen, ICertificateValidator certificateValidator, EncryptionPolicy encryptionPolicy, MonoTlsSettings settings)
			: base (innerStream, leaveInnerStreamOpen)
		{
			if (encryptionPolicy != EncryptionPolicy.RequireEncryption && encryptionPolicy != EncryptionPolicy.AllowNoEncryption && encryptionPolicy != EncryptionPolicy.NoEncryption)
				throw new ArgumentException (SR.GetString (SR.net_invalid_enum, "EncryptionPolicy"), "encryptionPolicy");

			_Configuration = new MyConfiguration (certificateValidator, settings);
			_SslState = new SslState (innerStream, null, null, encryptionPolicy, _Configuration);
		}

		class MyConfiguration : SSPIConfiguration
		{
			ICertificateValidator validator;
			MonoTlsSettings settings;

			public MyConfiguration (ICertificateValidator validator, MonoTlsSettings settings)
			{
				this.validator = validator;
				this.settings = settings;
			}

			public ICertificateValidator CertificateValidator {
				get { return validator; }
			}

			public MonoTlsSettings Settings {
				get { return settings; }
			}
		}
		#endif

		internal bool IsClosed {
			get { return _SslState.IsClosed; }
		}

		internal Exception LastError {
			get { return _SslState.LastError; }
		}

		internal IAsyncResult BeginShutdown (bool waitForReply, AsyncCallback asyncCallback, object asyncState)
		{
			return _SslState.BeginShutdown (waitForReply, asyncCallback, asyncState);
		}

		internal void EndShutdown (IAsyncResult asyncResult)
		{
			_SslState.EndShutdown (asyncResult);
		}
	}
}
#endif
