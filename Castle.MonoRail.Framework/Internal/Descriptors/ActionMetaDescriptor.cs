// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.Internal
{
	using System;
	using System.Collections;

	/// <summary>
	/// Holds the meta information for a specific action
	/// </summary>
	[Serializable]
	public class ActionMetaDescriptor : BaseMetaDescriptor
	{
		private SkipRescueAttribute skipRescue;
		private AccessibleThroughAttribute accessibleThrough;
		private IList skipFilters = new ArrayList();
		private IList cacheConfigurers = new ArrayList();
		private TransformFilterDescriptor[] transformFilters = new TransformFilterDescriptor[] {};
		
		public SkipRescueAttribute SkipRescue
		{
			get { return skipRescue; }
			set { skipRescue = value; }
		}

		public AccessibleThroughAttribute AccessibleThrough
		{
			get { return accessibleThrough; }
			set { accessibleThrough = value; }
		}

		public IList SkipFilters
		{
			get { return skipFilters; }
		}

		public IList CacheConfigurers
		{
			get { return cacheConfigurers; }
		}

		public TransformFilterDescriptor[] TransformFilters
		{
			get { return transformFilters; }
			set { transformFilters = value; }
		}
	}
}