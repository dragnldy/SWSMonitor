import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

debugger;

// Export googleAuth for JSImport
import { triggerGoogleFedCM } from './fedCM.js';
globalThis.googleAuth = triggerGoogleFedCM;

// ============================================================================
// Run the .NET application
// Note: localStorage functions are now in storage.js to avoid circular dependency
// ============================================================================

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
