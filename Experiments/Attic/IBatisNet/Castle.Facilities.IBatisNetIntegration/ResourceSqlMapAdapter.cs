#region License

// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// -- 
// 
// This facility was a contribution kindly 
// donated by Gilles Bayon &lt;gilles.bayon@gmail.com&gt;
// 
// --

#endregion

namespace Castle.Facilities.IBatisNetIntegration
{
	using Castle.Services.Transaction;
	using IBatisNet.Common;

	public class ResourceSqlMapAdapter : IResource
	{
		private IDalSession _session;

		public ResourceSqlMapAdapter(IDalSession session)
		{
			_session = session;
		}

		public void Start() {}

		public void Commit()
		{
			_session.CommitTransaction(false);
		}

		public void Rollback()
		{
			_session.RollBackTransaction(false);
		}
	}
}