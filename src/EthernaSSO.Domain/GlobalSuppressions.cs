// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "Overridability is needed for permit to use proxy models. Setting methods for data validation", Scope = "namespaceanddescendants", Target = "~N:Etherna.SSOServer.Domain.Models")]
