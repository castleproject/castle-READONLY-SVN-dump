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

namespace Castle.DynamicProxy.Generators.Emitters.SimpleAST
{
	using System;
	using System.Reflection.Emit;

	[CLSCompliant(false)]
	public class ArgumentReference : TypeReference
	{
		private int position = -1;

		public ArgumentReference(Type argumentType)
			: base(argumentType)
		{
		}

		internal int Position
		{
			get { return position; }
			set { position = value; }
		}

		public override void LoadReference(ILGenerator gen)
		{
			if (Position == -1)
			{
				throw new ApplicationException("ArgumentReference unitialized");
			}
			switch (Position)
			{
				case 0:
					gen.Emit(OpCodes.Ldarg_0);
					break;
				case 1:
					gen.Emit(OpCodes.Ldarg_1);
					break;
				case 2:
					gen.Emit(OpCodes.Ldarg_2);
					break;
				case 3:
					gen.Emit(OpCodes.Ldarg_3);
					break;
				default:
					gen.Emit(OpCodes.Ldarg, Position);
					break;
			}
		}

		public override void StoreReference(ILGenerator gen)
		{
			if (Position == -1)
			{
				throw new ApplicationException("ArgumentReference unitialized");
			}
			gen.Emit(OpCodes.Starg, Position);
		}

		public override void LoadAddressOfReference(ILGenerator gen)
		{
			throw new NotSupportedException();
		}
	}
}