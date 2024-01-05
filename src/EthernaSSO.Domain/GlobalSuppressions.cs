// Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "Overridability is needed for permit to use proxy models. Setting methods for data validation", Scope = "namespaceanddescendants", Target = "~N:Etherna.SSOServer.Domain.Models")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Domain models can", Scope = "namespaceanddescendants", Target = "~N:Etherna.SSOServer.Domain.Models")]
