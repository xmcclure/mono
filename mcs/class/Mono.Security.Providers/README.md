Mono.Security.Interface / Mono.Security.Providers
=================================================

This is not a public API yet, but will eventually become public.


Mono.Security.Interface
-----------------------

Mono.Security.Interface provides an abstraction layer for the TLS
APIs that are currently being used by Mono's class libraries.

The main API entry points are MonoTlsProviderFactory.GetProvider()
and MonoTlsProviderFactory.InstallProvider().

Mono.Net.Security
-----------------

Mono.Net.Security provides the internal implementation and lives
inside System.dll as private and internal APIs.  Mono.Security.dll
uses reflection to access these.

Mono.Security.Providers
-----------------------

Implementations of the 'Mono.Security.Interface.MonoTlsProvider' class
to provide TLS functionality.

The default provider is inside System.dll - it will be used automatically
if you don't explicitly install a custom provider, so simply call
MonoTlsProviderFactory.GetProvider() to use it.

* DotNet:
  Provides System.dll's SslStream implementation, only uses public .NET types.
  
* NewSystemSource:
  Compiles several referencesource files which would normally live inside
  System.dll if we compiled it with their SslStream implementation.
  
  This allows to keep the code in System.dll as-is, while still providing the
  new SslStream, which will be required by the new TLS code.
  
  System.dll needs to make its internals visible and we're using several compiler /
  external alias tricks in here to make this work.
  
  In this configuration, MONO_SYSTEM_ALIAS, MONO_FEATURE_NEW_TLS and
  MONO_FEATURE_NEW_SYSTEM_SOURCE (defining conditional for this configuration)
  are defined.  We do not define MONO_X509_ALIAS here.
  
* NewTls (not included here):
  Provides the new TLS implementation.
  
* TestFramework (not included here):
  Testing stuff.
  
The Mono.Security.Providers directory is not included in the top-level Makefile
and it should only be used to build and test the new TLS code.

Pending changes
---------------

This code is not actually being used in System.dll yet.  I have some
local changes which will switch the existing code in WebClient,
SmptClient and FtpWebRequest over, but these need to be carefully
tested.

At the moment, this work branch only provides new code and build
changes, which shuffle some stuff around.  There are also several
new files which are conditional and not actually being built by
default.

Build Configurations
--------------------

* Normal build:
  Builds everything as before with some new APIs added, but without
  modifying any existing functionality.
  
* Build with the 'newtls' profile:
  Builds System.dll with the new SslStream implementation from the
  referencesource.  This is currently a "build then throw away" profile
  and not enabled from the top-level Makefile.  The resulting System.dll
  won't actually work at runtime since it's missing the new TLS code;
  building it helps find problems and ensures that it will actually
  be possible to build this once we deploy it.

* Mono.Security.Providers:
  This needs to be manually built and installed.  The new TLS code
  requires the newly built System and Mono.Security.Providers.NewSystemSource
  from this branch installed in the system.  It is recommended to install it
  into a custom prefix and set that as defalt runtime in Xamarin Studio.

* Mono.Security.Providers.NewTls and Mono.Security.NewTls (not included here):
  These are Xamarin Studio projects and do not use any Makefiles :-)
  
  They use the installed System and Mono.Security.Providers.NewSystemSource
  assemblies from the current runtime.
  
Last changed Feb 07th, 2015,
Martin Baulig <martin.baulig@xamarin.com>


