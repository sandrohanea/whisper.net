// Licensed under the MIT license: https://opensource.org/licenses/MIT

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Suppressed these because the Memory based oberload is not available in .NET Standard 2.0, which is one of the target frameworks.
[assembly: SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "<Pending>", Scope = "member", Target = "~M:Whisper.net.Wave.WaveParser.GetAvgSamplesAsync(System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Single[]}")]
[assembly: SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "<Pending>", Scope = "member", Target = "~M:Whisper.net.Wave.WaveParser.InitializeCore(System.Boolean)~System.Threading.Tasks.Task")]
