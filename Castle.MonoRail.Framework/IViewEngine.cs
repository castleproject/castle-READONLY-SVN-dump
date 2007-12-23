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

namespace Castle.MonoRail.Framework
{
	using System;
	using System.IO;
	using Castle.MonoRail.Framework.JSGeneration;

	/// <summary>
	/// Depicts the contract used by the engine
	/// to process views, in an independent manner.
	/// </summary>
	public interface IViewEngine
	{
		/// <summary>
		/// Gets a value indicating whether the view engine
		/// support the generation of JS.
		/// </summary>
		/// <value>
		/// <c>true</c> if JS generation is supported; otherwise, <c>false</c>.
		/// </value>
		bool SupportsJSGeneration { get; }

		/// <summary>
		/// Evaluates whether the specified template can be used to generate js.
		/// </summary>
		/// <returns><c>true</c> if it exists</returns>
		bool IsTemplateForJSGeneration(String templateName);

		/// <summary>
		/// Gets the JS generator view template file extension.
		/// </summary>
		/// <value>The JS generator file extension.</value>
		String JSGeneratorFileExtension { get; }

		/// <summary>
		/// Implementors should return a generator instance if
		/// the view engine supports JS generation.
		/// </summary>
		/// <param name="context">The request context.</param>
		/// <param name="controller">The controller.</param>
		/// <param name="controllerContext">The controller context.</param>
		/// <returns>A JS generator instance</returns>
		object CreateJSGenerator(IEngineContext context, IController controller, IControllerContext controllerContext);

		/// <summary>
		/// Processes the js generation view template - using the templateName
		/// to obtain the correct template, and using the specified <see cref="TextWriter"/>
		/// to output the result.
		/// </summary>
		/// <param name="output">The output.</param>
		/// <param name="context">The request context.</param>
		/// <param name="controller">The controller.</param>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="templateName">Name of the template.</param>
		void GenerateJS(String templateName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext);

		/// <summary>
		/// Gets or sets a value indicating whether the view engine should set the
		/// content type to xhtml.
		/// </summary>
		/// <value><c>true</c> if the content type should be set to xhtml; otherwise, <c>false</c>.</value>
		bool XHtmlRendering { get; set; }

		/// <summary>
		/// Gets the view template file extension.
		/// </summary>
		/// <value>The view file extension.</value>
		String ViewFileExtension { get; }

		/// <summary>
		/// Evaluates whether the specified template exists.
		/// </summary>
		/// <returns><c>true</c> if it exists</returns>
		bool HasTemplate(String templateName);

		///<summary>
		/// Processes the view - using the templateName 
		/// to obtain the correct template
		/// and writes the results to the <see cref="TextWriter"/>. 
		/// No layout is applied!
		/// </summary>
		void Process(String templateName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext);

		/// <summary>
		/// Wraps the specified content in the layout using
		/// the context to output the result.
		/// </summary>
		/// <param name="context">The request context.</param>
		/// <param name="controller">The controller.</param>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="contents">Static content to output within the layout</param>
		void RenderStaticWithinLayout(String contents, IEngineContext context, IController controller, IControllerContext controllerContext);

		/// <summary>
		/// Should process the specified partial. The partial name must contains
		/// the path relative to the views folder.
		/// </summary>
		/// <param name="output">The output.</param>
		/// <param name="context">The request context.</param>
		/// <param name="controller">The controller.</param>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="partialName">The partial name.</param>
		void ProcessPartial(String partialName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext);
	}
}
