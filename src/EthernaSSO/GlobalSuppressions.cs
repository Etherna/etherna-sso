// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822: Member 'NormalizeUsername' does not access instance data and can be marked as static", Justification = "Static methods stink...", Scope = "namespaceanddescendants", Target = "Etherna.SSOServer")]