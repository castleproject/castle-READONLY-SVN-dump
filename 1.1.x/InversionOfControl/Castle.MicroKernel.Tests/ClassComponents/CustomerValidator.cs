// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel.Tests.ClassComponents
{
	using System;

	/// <summary>
	/// Summary description for IValidator.
	/// </summary>
	public class CustomerValidator : IValidator<ICustomer>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="customer"></param>
		/// <returns></returns>
		public bool IsValid(ICustomer customer)
		{
			return true;
		}
	}

	/// <summary>
	/// Summary description for IValidator.
	/// </summary>
	public class CustomerChainValidator<T> : IValidator<T>
		where T : CustomerChain1
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="customerChain"></param>
		/// <returns></returns>
		public bool IsValid(T customerChain)
		{
			return true;
		}
	}
}