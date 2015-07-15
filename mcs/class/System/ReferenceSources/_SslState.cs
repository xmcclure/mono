//
// Mono-specific additions to Microsoft's _SslState.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
namespace System.Net.Security
{
	using System.IO;
	using System.Threading;
	using System.Net.Sockets;

	partial class SslState
	{
		internal bool IsClosed {
			get { return Context.IsClosed; }
		}

		internal ProtocolToken CreateShutdownMessage ()
		{
			return Context.CreateShutdownMessage ();
		}
	}
}
#endif
