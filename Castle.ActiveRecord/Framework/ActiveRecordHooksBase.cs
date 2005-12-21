// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
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

namespace Castle.ActiveRecord
{
	using System;
	using System.Collections;

	using NHibernate;
	using NHibernate.Expression;

	using Castle.ActiveRecord.Framework;
	
	public abstract class ActiveRecordHooksBase
    {
        #region Overridable Hooks -  copied from ActiveRecordBase

        /// <summary>
        /// Hook to change the object state
        /// before saving it.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Return <c>true</c> if you have changed the state. <c>false</c> otherwise</returns>
        protected internal virtual bool BeforeSave(System.Collections.IDictionary state) {
            return false;
        }

        /// <summary>
        /// Hook to transform the read data 
        /// from the database before populating 
        /// the object instance
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns>Return <c>true</c> if you have changed the state. <c>false</c> otherwise</returns>
        protected internal virtual bool BeforeLoad(System.Collections.IDictionary adapter) {
            return false;
        }

        /// <summary>
        /// Hook to perform additional tasks 
        /// before removing the object instance representation
        /// from the database.
        /// </summary>
        /// <param name="adapter"></param>
        protected internal virtual void BeforeDelete(System.Collections.IDictionary adapter) {
        }

        #endregion
    }

}
