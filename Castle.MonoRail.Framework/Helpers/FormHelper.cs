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

namespace Castle.MonoRail.Framework.Helpers
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Data;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using Castle.Core;
	using Castle.Core.Logging;
	using Castle.MonoRail.Framework.Helpers.ValidationStrategy;
	using Castle.MonoRail.Framework.Internal;
	using HtmlTextWriter = System.Web.UI.HtmlTextWriter;
	using Castle.Components.Binder;
	using Castle.Components.Validator;

	public enum RequestContext
	{
		All,
		PropertyBag,
		Flash,
		Session,
		Request, 
		Params
	}

	/// <summary>
	/// The FormHelper allows you to output Html Input elements using the 
	/// conventions necessary to use the DataBinder on the server side. 
	/// <para>
	/// It also query the objects available on the context to show property 
	/// values correctly, saving you the burden of filling text inputs, selects, 
	/// checkboxes and radios.
	/// </para>
	/// <para>
	/// <b>Mask support</b>. 
	/// For most elements, you can use 
	/// the entries <c>mask</c> and optionally <c>mask_separator</c> to define a 
	/// mask for your inputs. Kudos to mordechai Sandhaus - 52action.com
	/// </para>
	/// <para>
	/// For example: mask='2,5',mask_separator='/' will mask the content to '12/34/1234'
	/// </para>
	/// </summary>
	public class FormHelper : AbstractHelper, IServiceEnabledComponent
	{
		protected static readonly BindingFlags PropertyFlags = BindingFlags.GetProperty|BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase;
		protected static readonly BindingFlags FieldFlags = BindingFlags.GetField|BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase;

		private int formCount;
		private string currentFormId;
		private Stack objectStack = new Stack();
		private IWebValidatorProvider validatorProvider = new PrototypeWebValidator();
		private WebValidationConfiguration validationConfig = new WebValidationConfiguration();

		protected ILogger logger = NullLogger.Instance;

		#region IServiceEnabledComponent implementation

		/// <summary>
		/// Invoked by the framework in order to give a chance to
		/// obtain other services
		/// </summary>
		/// <param name="provider">The service proviver</param>
		public void Service(IServiceProvider provider)
		{
			ILoggerFactory loggerFactory = (ILoggerFactory) provider.GetService(typeof(ILoggerFactory));

			if (loggerFactory != null)
			{
				logger = loggerFactory.Create(typeof(FormHelper));
			}

			IWebValidatorProvider validatorProv = (IWebValidatorProvider)
				provider.GetService(typeof(IWebValidatorProvider));

			if (validatorProv != null)
			{
				validatorProvider = validatorProv;
			}
		}

		#endregion

		/// <summary>
		/// Renders a Javascript library inside a single script tag.
		/// </summary>
		/// <returns></returns>
		public string InstallScripts()
		{
			return RenderScriptBlockToSource("/MonoRail/Files/FormHelperScript");
		}

		/// <summary>
		/// Creates a form tag based on the parameters.
		/// Pendent: more documentation
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public string FormTag(IDictionary parameters)
		{
			string url = UrlHelper.For(parameters);

			currentFormId = CommonUtils.ObtainEntryAndRemove(parameters, "id", "form" + ++formCount);

			validationConfig = validatorProvider.CreateConfiguration(parameters);

			string afterFormTag = validationConfig.CreateAfterFormOpened(currentFormId);

			string formContent = "<form action='" + url + "' method='post' id='" + currentFormId + "' " + GetAttributes(parameters) + ">";

			return formContent + afterFormTag;
		}

		public string EndFormTag()
		{
			string beforeEndTag = validationConfig.CreateBeforeFormClosed(currentFormId);

			return beforeEndTag + "</form>";
		}

		public void Push(string target)
		{
			Push(target, null);
		}

		public void Push(string target, IDictionary parameters)
		{
			string disableValidation = CommonUtils.ObtainEntryAndRemove(parameters, "disablevalidation", "false");
			object value = ObtainValue(target);

			if (value != null)
			{
				objectStack.Push(new FormScopeInfo(target, disableValidation == "true"));
			}
			else
			{
				value = ObtainValue(target + "type");

				if (value != null)
				{
					objectStack.Push(new FormScopeInfo(target, disableValidation == "true"));
				}
				else
				{
					throw new ArgumentException("target could not be evaluated during Push operation. Target: " + target);
				}
			}
		}

		public void Pop()
		{
			objectStack.Pop();
		}

		public string Submit(string value)
		{
			return Submit(value, null);
		}

		public string Submit(string value, IDictionary attributes)
		{
			return CreateInputElement("submit", value, attributes);
		}

		public string Button(string value)
		{
			return Submit(value, null);
		}

		public string Button(string value, IDictionary attributes)
		{
			return CreateInputElement("button", value, attributes);
		}

		#region TextFieldValue

		/// <summary>
		/// Generates an input text form element
		/// with the supplied value
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="value">Value to supply to the element (instead of querying the target)</param>
		/// <returns>The generated form element</returns>
		public string TextFieldValue(string target, object value)
		{
			return TextFieldValue(target, value, null);
		}

		/// <summary>
		/// Generates an input text form element
		/// with the supplied value
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="value">Value to supply to the element (instead of querying the target)</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string TextFieldValue(string target, object value, IDictionary attributes)
		{
			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region TextField

		/// <summary>
		/// Generates an input text element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		public string TextField(string target)
		{
			return TextField(target, null);
		}

		/// <summary>
		/// Generates an input text element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string TextField(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			ApplyValidation(InputElementType.Text, target, ref attributes);

			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region NumberField

		/// <summary>
		/// Generates an input text element with a javascript that prevents
		/// chars other than numbers from being entered.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		/// <remarks>
		/// You must invoke <see cref="FormHelper.InstallScripts"/> before using it
		/// </remarks>
		public string NumberField(string target)
		{
			return NumberField(target, null);
		}

		/// <summary>
		/// Generates an input text element with a javascript that prevents
		/// chars other than numbers from being entered.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// <para>
		/// You can optionally pass an <c>exceptions</c> value through the dictionary.
		/// It must be a comma separated list of chars that can be accepted on the field. 
		/// For example:
		/// </para>
		/// <code>
		/// FormHelper.NumberField("product.price", {exceptions='13,10,11'})
		/// </code>
		/// In this case the key codes 13, 10 and 11 will be accepted on the field.
		/// <para>
		/// You can aslo optionally pass an <c>forbid</c> value through the dictionary.
		/// It must be a comma separated list of chars that cannot be accepted on the field. 
		/// For example:
		/// </para>
		/// <code>
		/// FormHelper.NumberField("product.price", {forbid='46'})
		/// </code>
		/// In this case the key code 46 (period) will not be accepted on the field.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		/// <remarks>
		/// You must invoke <see cref="FormHelper.InstallScripts"/> before using it
		/// </remarks>
		public string NumberField(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			attributes = attributes != null ? attributes : new Hashtable();

			ApplyNumberOnlyOptions(attributes);
			ApplyValidation(InputElementType.Text, target, ref attributes);

			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region TextArea

		/// <summary>
		/// Generates a textarea element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		public string TextArea(string target)
		{
			return TextArea(target, null);
		}

		/// <summary>
		/// Generates a textarea element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string TextArea(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			value = value == null ? "" : HtmlEncode(value.ToString());

			string id = CreateHtmlId(target);

			ApplyValidation(InputElementType.Text, target, ref attributes);

			return String.Format("<textarea id=\"{0}\" name=\"{1}\" {2}>{3}</textarea>", 
				id, target, GetAttributes(attributes), value);
		}

		#endregion

		#region PasswordField

		/// <summary>
		/// Generates a password input field.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		public string PasswordField(string target)
		{
			return PasswordField(target, null);
		}

		/// <summary>
		/// Generates a password input field.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string PasswordField(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			ApplyValidation(InputElementType.Text, target, ref attributes);

			return CreateInputElement("password", target, value, attributes);
		}

		#endregion

		#region PasswordNumberField

		/// <summary>
		/// Generates an input password element with a javascript that prevents
		/// chars other than numbers from being entered.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <remarks>
		/// You must invoke <see cref="FormHelper.InstallScripts"/> before using it
		/// </remarks>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		public string PasswordNumberField(string target)
		{
			return PasswordNumberField(target, null);
		}

		/// <summary>
		/// Generates an input password element with a javascript that prevents
		/// chars other than numbers from being entered.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// <para>
		/// You can optionally pass an <c>exceptions</c> value through the dictionary.
		/// It must be a comma separated list of chars that can be accepted on the field. 
		/// For example:
		/// </para>
		/// <code>
		/// FormHelper.NumberField("product.price", {exceptions='13,10,11'})
		/// </code>
		/// In this case the key codes 13, 10 and 11 will be accepted on the field.
		/// <para>
		/// You can aslo optionally pass an <c>forbid</c> value through the dictionary.
		/// It must be a comma separated list of chars that cannot be accepted on the field. 
		/// For example:
		/// </para>
		/// <code>
		/// FormHelper.NumberField("product.price", {forbid='46'})
		/// </code>
		/// In this case the key code 46 (period) will not be accepted on the field.
		/// </summary>
		/// <remarks>
		/// You must invoke <see cref="FormHelper.InstallScripts"/> before using it
		/// </remarks>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string PasswordNumberField(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			attributes = attributes != null ? attributes : new Hashtable();

			ApplyNumberOnlyOptions(attributes);
			ApplyValidation(InputElementType.Text, target, ref attributes);

			return CreateInputElement("password", target, value, attributes);
		}

		#endregion

		#region TextFieldFormat

		/// <summary>
		/// Generates an input text element and formats the value
		/// with the specified format
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="formatString">The format string</param>
		/// <returns>The generated form element</returns>
		[Obsolete("Use the attribute 'textformat' instead")]
		public string TextFieldFormat(string target, string formatString)
		{
			return TextFieldFormat(target, formatString, null);
		}

		/// <summary>
		/// Generates an input text element and formats the value
		/// with the specified format
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="formatString">The format string</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		[Obsolete("Use the attribute 'textformat' instead")]
		public string TextFieldFormat(string target, string formatString, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			if (value != null)
			{
				IFormattable formattable = value as IFormattable;

				if (formattable != null)
				{
					value = formattable.ToString(formatString, null);
				}
			}

			ApplyValidation(InputElementType.Text, target, ref attributes);

			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region LabelFor

		/// <summary>
		/// Generates a label element.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="label">Legend</param>
		/// <returns>The generated form element</returns>
		public string LabelFor(string target, string label)
		{
			return LabelFor(target, label, null);
		}
		
		/// <summary>
		/// Generates a label element.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="label">Legend</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string LabelFor(string target, string label, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			string id = CreateHtmlId(attributes, target);

			StringBuilder sb = new StringBuilder();
			StringWriter sbWriter = new StringWriter(sb);
			HtmlTextWriter writer = new HtmlTextWriter(sbWriter);

			writer.WriteBeginTag("label");
			writer.WriteAttribute("for", id);
			writer.Write(GetAttributes(attributes)); 
			writer.Write(HtmlTextWriter.TagRightChar);
			writer.Write(label);
			writer.WriteEndTag("label");

			return sbWriter.ToString();
		}

		#endregion

		#region HiddenField

		/// <summary>
		/// Generates a hidden form element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		public string HiddenField(string target)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			return CreateInputElement("hidden", target, value, null);
		}

		/// <summary>
		/// Generates a hidden form element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="value">The value for the hidden field</param>
		/// <returns>The generated form element</returns>
		public string HiddenField(string target, object value)
		{
			return CreateInputElement("hidden", target, value, null);
		}
		
		/// <summary>
		/// Generates a hidden form element.
		/// <para>
		/// The value is extracted from the target (if available)
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string HiddenField(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);
			
			string id = CreateHtmlId(attributes, target);
			
			value = value != null ? value : String.Empty;

			return CreateInputElement("hidden", id, target, value.ToString(), attributes);
		}

		#endregion

		#region CheckboxList

		/// <summary>
		/// Creates a <see cref="CheckboxList"/> instance
		/// which is enumerable. For each interaction you can invoke
		/// <see cref="CheckboxList.Item"/> which will correctly render
		/// a checkbox input element for the current element on the supplied set (<c>dataSource</c>).
		/// <para>
		/// The enumerable item will be an element of the <c>dataSource</c>.
		/// </para>
		/// If the <c>dataSource</c>
		/// elements are complex objects (ie not string or primitives), 
		/// supply the parameters <c>value</c> and <c>text</c> to the dictionary to make
		/// the helper use the specified properties to extract the <c>option</c> value and content respectively.
		/// <para>
		/// Usually both the <c>target</c> and obviously the <c>dataSource</c> are sets
		/// with multiple items. The element types tend to be the same. If 
		/// they are not, you might have to specify the <c>suffix</c> parameters on 
		/// the <c>attributes</c> as it would not be inferred.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="dataSource">The set of available elements</param>
		/// <returns>The generated form element</returns>
		public CheckboxList CreateCheckboxList(string target, IEnumerable dataSource)
		{
			return CreateCheckboxList(target, dataSource, null);
		}

		/// <summary>
		/// Creates a <see cref="CheckboxList"/> instance
		/// which is enumerable. For each interaction you can invoke
		/// <see cref="CheckboxList.Item"/> which will correctly render
		/// a checkbox input element for the current element on the supplied set (<c>dataSource</c>).
		/// <para>
		/// The enumerable item will be an element of the <c>dataSource</c>.
		/// </para>
		/// If the <c>dataSource</c>
		/// elements are complex objects (ie not string or primitives), 
		/// supply the parameters <c>value</c> and <c>text</c> to the dictionary to make
		/// the helper use the specified properties to extract the <c>option</c> value and content respectively.
		/// <para>
		/// Usually both the <c>target</c> and obviously the <c>dataSource</c> are sets
		/// with multiple items. The element types tend to be the same. If 
		/// they are not, you might have to specify the <c>suffix</c> parameters on 
		/// the <c>attributes</c> as it would not be inferred.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="dataSource">The set of available elements</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public CheckboxList CreateCheckboxList(string target, IEnumerable dataSource, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);
			
			return new CheckboxList(this, target, value, dataSource, attributes);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="suffix"></param>
		/// <param name="item"></param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		internal string CheckboxItem(int index, string target, string suffix, SetItem item, IDictionary attributes)
		{
			if (item.IsSelected)
			{
				AddChecked(attributes);
			}
			else
			{
				RemoveChecked(attributes);
			}

			target = String.Format("{0}[{1}]", target, index);
			
			string elementId = CreateHtmlId(attributes, target, false);
			
			string computedTarget = target;
			
			if (suffix != null && suffix != String.Empty)
			{
				computedTarget += "." + suffix;
			}

			return CreateInputElement("checkbox", elementId, computedTarget, item.Value, attributes);
		}

		/// <summary>
		/// 
		/// </summary>
		public sealed class CheckboxList : IEnumerable, IEnumerator
		{
			private readonly FormHelper helper;
			private readonly string target;
			private readonly IDictionary attributes;
			private readonly OperationState operationState;
			private readonly IEnumerator enumerator;
			private bool hasMovedNext, hasItem;
			private int index = -1;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="helper"></param>
			/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
			/// <param name="initialSelectionSet"></param>
			/// <param name="dataSource">The set of available elements</param>
			/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
			public CheckboxList(FormHelper helper, string target,
								object initialSelectionSet, IEnumerable dataSource, IDictionary attributes)
			{
				if (dataSource == null) throw new ArgumentNullException("dataSource");

				this.helper = helper;
				this.target = target;
				this.attributes = attributes == null ? new HybridDictionary(true) : attributes;
				
				operationState = SetOperation.IterateOnDataSource(initialSelectionSet, dataSource, attributes);
				enumerator = operationState.GetEnumerator();
			}
			
			/// <summary>
			/// 
			/// </summary>
			/// <returns>The generated form element</returns>
			public string Item()
			{
				if (!hasMovedNext)
				{
					throw new InvalidOperationException("Before rendering a checkbox item, you must use MoveNext");
				}
				
				if (!hasItem)
				{
					// Nothing to render
					return String.Empty;
				}
								
				return helper.CheckboxItem(index, target, operationState.TargetSuffix, CurrentSetItem, attributes);
			}

			public IEnumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				hasMovedNext = true;
				hasItem = enumerator.MoveNext();
				
				if (hasItem) index++;
				
				return hasItem;
			}

			public void Reset()
			{
				index = -1;
				enumerator.Reset();
			}

			public object Current
			{
				get { return CurrentSetItem.Item; }
			}
			
			public SetItem CurrentSetItem
			{
				get { return enumerator.Current as SetItem; }
			}
		}

		#endregion
		
		#region CheckboxField

		/// <summary>
		/// Generates a checkbox field. In fact it generates two as a
		/// way to send a value if the primary checkbox is not checked.
		/// This allow the process the be aware of the unchecked value
		/// and act accordingly.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		public string CheckboxField(string target)
		{
			return CheckboxField(target, null);
		}

		/// <summary>
		/// Generates a checkbox field. In fact it generates two as a
		/// way to send a value if the primary checkbox is not checked.
		/// This allow the process the be aware of the unchecked value
		/// and act accordingly.
		/// <para>
		/// The checked and unchecked values sent to the server defaults
		/// to true and false. You can override them using the 
		/// parameters <c>trueValue</c> and <c>falseValue</c>.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string CheckboxField(string target, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			string trueValue = CommonUtils.ObtainEntryAndRemove(attributes, "trueValue", "true");
			
			bool isChecked;

			if (trueValue != "true")
			{
				isChecked = AreEqual(value, trueValue);
			}
			else
			{
				isChecked = ((value != null && value is bool && ((bool)value)) || 
				             (!(value is bool) && (value != null)));
			}

			if (isChecked)
			{
				if (attributes == null)
				{
					attributes = new HybridDictionary(true);
				}

				AddChecked(attributes);
			}

			string id = CreateHtmlId(attributes, target);
			string hiddenElementId = id + "H";
			string hiddenElementValue = CommonUtils.ObtainEntryAndRemove(attributes, "falseValue", "false");

			string result = CreateInputElement("checkbox", id, target, trueValue, attributes);
			
			result += CreateInputElement("hidden", hiddenElementId, target, hiddenElementValue, null);
			
			return result;
		}

		#endregion

		#region RadioField

		/// <summary>
		/// Generates a radio input type with the specified 
		/// value to send to the served in case the element in checked.
		/// It will automatically check the radio if the target 
		/// evaluated value is equal to the specified <c>valueToSend</c>.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="valueToSend"></param>
		/// <returns>The generated form element</returns>
		public string RadioField(string target, object valueToSend)
		{
			return RadioField(target, valueToSend, null);
		}

		/// <summary>
		/// Generates a radio input type with the specified 
		/// value to send to the served in case the element in checked.
		/// It will automatically check the radio if the target 
		/// evaluated value is equal to the specified <c>valueToSend</c>.
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="valueToSend"></param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string RadioField(string target, object valueToSend, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object value = ObtainValue(target);

			bool isChecked = AreEqual(valueToSend, value);

			if (isChecked)
			{
				if (attributes == null)
				{
					attributes = new HybridDictionary(true);
				}

				AddChecked(attributes);
			}

			return CreateInputElement("radio", target, valueToSend, attributes);
		}

		#endregion

		#region Select

		/// <summary>
		/// Creates a <c>select</c> element and its <c>option</c>s based on the <c>dataSource</c>.
		/// If the <c>dataSource</c>
		/// elements are complex objects (ie not string or primitives), 
		/// supply the parameters <c>value</c> and <c>text</c> to the dictionary to make
		/// the helper use the specified properties to extract the <c>option</c> value and content respectively.
		/// <para>
		/// You can also specify the attribute <c>firstoption</c> to force the first option be
		/// something like 'please select'. You can set the value of <c>firstoption</c> by specifying the attribute
		/// <c>firstoptionvalue</c>. The default value is '0'.
		/// </para>
		/// <para>
		/// Usually the <c>target</c> is a single value and the <c>dataSource</c> is obviously 
		/// a set with multiple items. The element types tend to be the same. If 
		/// they are not, you might have to specify the <c>suffix</c> parameters on 
		/// the <c>attributes</c> as it would not be inferred.
		/// </para>
		/// <para>
		/// The target can also be a set. In this case the intersection will be 
		/// the initially selected elements.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="dataSource">The set of available elements</param>
		/// <returns>The generated form element</returns>
		public string Select(string target, IEnumerable dataSource)
		{
			return Select(target, dataSource, null);
		}

		/// <summary>
		/// Creates a <c>select</c> element and its <c>option</c>s based on the <c>dataSource</c>.
		/// If the <c>dataSource</c>
		/// elements are complex objects (ie not string or primitives), 
		/// supply the parameters <c>value</c> and <c>text</c> to the dictionary to make
		/// the helper use the specified properties to extract the <c>option</c> value and content respectively.
		/// <para>
		/// You can also specify the attribute <c>firstoption</c> to force the first option be
		/// something like 'please select'. You can set the value of <c>firstoption</c> by specifying the attribute
		/// <c>firstoptionvalue</c>. The default value is '0'.
		/// </para>
		/// <para>
		/// Usually the <c>target</c> is a single value and the <c>dataSource</c> is obviously 
		/// a set with multiple items. The element types tend to be the same. If 
		/// they are not, you might have to specify the <c>suffix</c> parameters on 
		/// the <c>attributes</c> as it would not be inferred.
		/// </para>
		/// <para>
		/// The target can also be a set. In this case the intersection will be 
		/// the initially selected elements.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="dataSource">The set of available elements</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string Select(string target, IEnumerable dataSource, IDictionary attributes)
		{
			target = RewriteTargetIfWithinObjectScope(target);

			object selectedValue = ObtainValue(target);

			return Select(target, selectedValue, dataSource, attributes);
		}
		
		/// <summary>
		/// Creates a <c>select</c> element and its <c>option</c>s based on the <c>dataSource</c>.
		/// If the <c>dataSource</c>
		/// elements are complex objects (ie not string or primitives), 
		/// supply the parameters <c>value</c> and <c>text</c> to the dictionary to make
		/// the helper use the specified properties to extract the <c>option</c> value and content respectively.
		/// <para>
		/// You can also specify the attribute <c>firstoption</c> to force the first option be
		/// something like 'please select'. You can set the value of <c>firstoption</c> by specifying the attribute
		/// <c>firstoptionvalue</c>. The default value is '0'.
		/// </para>
		/// <para>
		/// Usually the <c>target</c> is a single value and the <c>dataSource</c> is obviously 
		/// a set with multiple items. The element types tend to be the same. If 
		/// they are not, you might have to specify the <c>suffix</c> parameters on 
		/// the <c>attributes</c> as it would not be inferred.
		/// </para>
		/// <para>
		/// The target can also be a set. In this case the intersection will be 
		/// the initially selected elements.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="selectedValue"></param>
		/// <param name="dataSource">The set of available elements</param>
		/// <returns>The generated form element</returns>
		public string Select(string target, object selectedValue, IEnumerable dataSource)
		{
			return Select(target, selectedValue, dataSource, null);
		}

		/// <summary>
		/// Creates a <c>select</c> element and its <c>option</c>s based on the <c>dataSource</c>.
		/// If the <c>dataSource</c>
		/// elements are complex objects (ie not string or primitives), 
		/// supply the parameters <c>value</c> and <c>text</c> to the dictionary to make
		/// the helper use the specified properties to extract the <c>option</c> value and content respectively.
		/// <para>
		/// You can also specify the attribute <c>firstoption</c> to force the first option be
		/// something like 'please select'. You can set the value of <c>firstoption</c> by specifying the attribute
		/// <c>firstoptionvalue</c>. The default value is '0'.
		/// </para>
		/// <para>
		/// Usually the <c>target</c> is a single value and the <c>dataSource</c> is obviously 
		/// a set with multiple items. The element types tend to be the same. If 
		/// they are not, you might have to specify the <c>suffix</c> parameters on 
		/// the <c>attributes</c> as it would not be inferred.
		/// </para>
		/// <para>
		/// The target can also be a set. In this case the intersection will be 
		/// the initially selected elements.
		/// </para>
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="selectedValue"></param>
		/// <param name="dataSource">The set of available elements</param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		public string Select(string target, object selectedValue, IEnumerable dataSource, IDictionary attributes)
		{
			string id = CreateHtmlId(target);

			ApplyValidation(InputElementType.Select, target, ref attributes);

			StringBuilder sb = new StringBuilder();
			StringWriter sbWriter = new StringWriter(sb);
			HtmlTextWriter writer = new HtmlTextWriter(sbWriter);

			string firstOption = null;
			string firstOptionValue = null;
			string name = target;

			if (attributes != null)
			{
				firstOption = CommonUtils.ObtainEntryAndRemove(attributes, "firstoption");
				firstOptionValue = CommonUtils.ObtainEntryAndRemove(attributes, "firstoptionvalue");
				
				if (attributes.Contains("name"))
				{
					name = (String) attributes["name"];
					attributes.Remove("name");
				}

				if (attributes.Contains("id"))
				{
					id = (String) attributes["id"];
					attributes.Remove("id");
				}
			}

			OperationState state = SetOperation.IterateOnDataSource(selectedValue, dataSource, attributes);

			writer.WriteBeginTag("select");
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.Write(" ");
			writer.Write(GetAttributes(attributes));
			writer.Write(HtmlTextWriter.TagRightChar);
			writer.WriteLine();

			if (firstOption != null)
			{
				writer.WriteBeginTag("option");
				writer.WriteAttribute("value", (firstOptionValue == null) ? "0" : firstOptionValue);
				writer.Write(HtmlTextWriter.TagRightChar);
				writer.Write(firstOption);
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
			
			foreach(SetItem item in state)
			{
				writer.WriteBeginTag("option");
				
				if (item.IsSelected)
				{
					writer.Write(" selected=\"selected\"");
				}

				writer.WriteAttribute("value", item.Value);
				writer.Write(HtmlTextWriter.TagRightChar);
				writer.Write(item.Text);
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
			
			writer.WriteEndTag("select");

			return sbWriter.ToString();
		}

		#endregion

		#region Validation

		/// <summary>
		/// Configures the use of fValidate for form fields validation
		/// </summary>
		public void UsefValidate()
		{
			validatorProvider = new FValidateWebValidator();
		}

		private bool IsValidationEnabledForScope
		{
			get
			{
				if (objectStack.Count == 0) return false;

				return ((FormScopeInfo) objectStack.Peek()).IsValidationEnabled;
			}
		}

		private void ApplyValidation(InputElementType inputType, string target, ref IDictionary attributes)
		{
			bool disableValidation = CommonUtils.ObtainEntryAndRemove(attributes, "disablevalidation", "false") == "true";

			if (!IsValidationEnabledForScope && disableValidation)
			{
				return;
			}

			if (Controller.Validator == null)
			{
				return;
			}

			if (attributes == null)
			{
				attributes = new HybridDictionary(true);
			}

			PropertyInfo property = ObtainTargetProperty(RequestContext.All, target);

			if (property == null)
			{
				if (logger.IsErrorEnabled)
				{
					logger.Error("Could not find property for target '" + target + "'. Skipping automatic validation for this property");
				}

				return;
			}

			IValidator[] validators = Controller.Validator.GetValidators(property.DeclaringType, property);

			IWebValidationGenerator generator = validatorProvider.CreateGenerator(inputType, attributes);

			foreach(IValidator validator in validators)
			{
				if (validator.SupportsWebValidation)
				{
					validator.ApplyWebValidation(validationConfig, inputType, generator, attributes);
				}
			}
		}

		#endregion

		#region protected members

		protected string RewriteTargetIfWithinObjectScope(string target)
		{
			if (objectStack.Count == 0)
			{
				return target;
			}
			else
			{
				return ((FormScopeInfo) objectStack.Peek()).RootTarget + "." + target;
			}
		}

		/// <summary>
		/// Creates the specified input element 
		/// using the specified parameters to supply the name, value, id and others 
		/// html attributes.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="value"></param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		protected string CreateInputElement(string type, string target, Object value, IDictionary attributes)
		{
			if (value == null)
			{
				value = CommonUtils.ObtainEntryAndRemove(attributes, "defaultValue");
			}

			string id = CreateHtmlId(attributes, target);

			return CreateInputElement(type, id, target, FormatIfNecessary(value, attributes), attributes);
		}

		/// <summary>
		/// Creates the specified input element 
		/// using the specified parameters to supply the name, value, id and others 
		/// html attributes.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="id"></param>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <param name="value"></param>
		/// <param name="attributes">Attributes for the FormHelper method and for the html element it generates</param>
		/// <returns>The generated form element</returns>
		protected string CreateInputElement(string type, string id, string target, string value, IDictionary attributes)
		{
			if (Controller.Context != null) // We have a context
			{
				value = HtmlEncode(value);
			}

			if (attributes != null && attributes.Contains("mask"))
			{
				string mask = CommonUtils.ObtainEntryAndRemove(attributes, "mask");
				string maskSep = CommonUtils.ObtainEntryAndRemove(attributes, "mask_separator", "-");

				string onBlur = CommonUtils.ObtainEntryAndRemove(attributes, "onBlur", "void(0)");
				string onKeyUp = CommonUtils.ObtainEntryAndRemove(attributes, "onKeyUp", "void(0)");

				string js = "return monorail_formhelper_mask(event,this,'" + mask + "','" + maskSep + "');";

				attributes["onBlur"] = "javascript:" + onBlur + ";" + js;
				attributes["onKeyUp"] = "javascript:" + onKeyUp + ";" + js;
			}
			
			return String.Format("<input type=\"{0}\" id=\"{1}\" name=\"{2}\" value=\"{3}\" {4}/>", 
			                     type, id, target, value, GetAttributes(attributes));
		}

		protected string CreateInputElement(string type, string value, IDictionary attributes)
		{
			return String.Format("<input type=\"{0}\" value=\"{1}\" {2}/>",
								 type, value, GetAttributes(attributes));
		}

		protected string FormatIfNecessary(object value, IDictionary attributes)
		{
			string formatString = CommonUtils.ObtainEntryAndRemove(attributes, "textformat");

			if (value != null && formatString != null)
			{
				IFormattable formattable = value as IFormattable;

				if (formattable != null)
				{
					value = formattable.ToString(formatString, null);
				}
			}
			else if (value == null)
			{
				value = String.Empty;
			}

			return value.ToString();
		}

		protected PropertyInfo ObtainTargetProperty(RequestContext context, string target)
		{
			string[] pieces;

			Type root = ObtainRootType(context, target, out pieces);

			if (root != null && pieces.Length > 1)
			{
				return QueryPropertyInfoRecursive(root, pieces, 1);
			}

			return null;
		}

		/// <summary>
		/// Queries the context for the target value
		/// </summary>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		protected object ObtainValue(string target)
		{
			return ObtainValue(RequestContext.All, target);
		}

		/// <summary>
		/// Queries the context for the target value
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		protected object ObtainValue(RequestContext context, string target)
		{
			string[] pieces;

			object rootInstance = ObtainRootInstance(context, target, out pieces);

			if (rootInstance != null && pieces.Length > 1)
			{
				return QueryPropertyRecursive(rootInstance, pieces, 1);
			}

			return rootInstance;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target">The object to get the value from and to be based on to create the element name.</param>
		/// <returns>The generated form element</returns>
		protected object ObtainRootInstance(RequestContext context, string target)
		{
			object rootInstance = null;

			if (context == RequestContext.All || context == RequestContext.PropertyBag)
			{
				rootInstance = Controller.PropertyBag[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Flash))
			{
				rootInstance = Controller.Context.Flash[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Session))
			{
				rootInstance = Controller.Context.Session[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Params))
			{
				rootInstance = Controller.Params[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Request))
			{
				rootInstance = Controller.Context.UnderlyingContext.Items[target];
			}

			return rootInstance;
		}

		protected object ObtainRootInstance(RequestContext context, string target, out string[] pieces)
		{
			pieces = target.Split(new char[] {'.'});

			string root = pieces[0];

			int index;

			bool isIndexed = CheckForExistenceAndExtractIndex(ref root, out index);

			object rootInstance = ObtainRootInstance(context, root);

			if (rootInstance == null)
			{
				return null;
			}

			if (isIndexed)
			{
				AssertIsValidArray(rootInstance, root, index);
			}
	
			if (!isIndexed && pieces.Length == 1)
			{
				return rootInstance;
			}
			else if (isIndexed)
			{
				rootInstance = GetArrayElement(rootInstance, index);
			}

			return rootInstance;
		}

		private Type ObtainRootType(RequestContext context, string target, out string[] pieces)
		{
			pieces = target.Split(new char[] { '.' });

			Type foundType = (Type) Controller.PropertyBag[pieces[0] + "type"];

			if (foundType == null)
			{
				object root = ObtainRootInstance(context, target, out pieces);

				if (root != null)
				{
					foundType = root.GetType();
				}
			}

			return foundType;
		}

		private PropertyInfo QueryPropertyInfoRecursive(Type type, string[] propertyPath, int piece)
		{
			string property = propertyPath[piece]; int index;

			bool isIndexed = CheckForExistenceAndExtractIndex(ref property, out index);

			PropertyInfo propertyInfo = type.GetProperty(property, PropertyFlags);

			if (propertyInfo == null)
			{
				if (logger.IsErrorEnabled)
				{
					logger.Error("No public property '{0}' found on type '{1}'", property, type.FullName);
				}

				return null;
			}

			if (!propertyInfo.CanRead)
			{
				throw new BindingException("Property '{0}' for type '{1}' can not be read",
					propertyInfo.Name, type.FullName);
			}

			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new BindingException("Property '{0}' for type '{1}' has indexes, which are not supported",
					propertyInfo.Name, type.FullName);
			}

			type = propertyInfo.PropertyType;

			if (typeof(ICollection).IsAssignableFrom(type))
			{
				return null;
			}

			if (isIndexed)
			{
				type = type.GetElementType();
			}

			if (piece + 1 == propertyPath.Length)
			{
				return propertyInfo;
			}

			return QueryPropertyInfoRecursive(type, propertyPath, piece + 1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootInstance"></param>
		/// <param name="propertyPath"></param>
		/// <param name="piece"></param>
		/// <returns>The generated form element</returns>
		protected object QueryPropertyRecursive(object rootInstance, string[] propertyPath, int piece)
		{
			string property = propertyPath[piece]; int index;

			Type instanceType = rootInstance.GetType();

			bool isIndexed = CheckForExistenceAndExtractIndex(ref property, out index);

			PropertyInfo propertyInfo = instanceType.GetProperty(property, PropertyFlags);

			object instance = null;

			if (propertyInfo == null)
			{
				FieldInfo fieldInfo = instanceType.GetField(property, FieldFlags);

				if (fieldInfo != null)
				{
					instance = fieldInfo.GetValue(rootInstance);
				}
			}
			else
			{
				if (!propertyInfo.CanRead)
				{
					throw new BindingException("Property '{0}' for type '{1}' can not be read", 
						propertyInfo.Name, instanceType.FullName);
				}
				
				if (propertyInfo.GetIndexParameters().Length != 0)
				{
					throw new BindingException("Property '{0}' for type '{1}' has indexes, which are not supported", 
						propertyInfo.Name, instanceType.FullName);
				}

				instance = propertyInfo.GetValue(rootInstance, null);
			}

			if (isIndexed && instance != null)
			{
				AssertIsValidArray(instance, property, index);

				instance = GetArrayElement(instance, index);
			}

			if (instance == null || piece + 1 == propertyPath.Length)
			{
				return instance;
			}

			return QueryPropertyRecursive(instance, propertyPath, piece + 1);
		}

		/// <summary>
		/// Creates the HTML id.
		/// </summary>
		/// <param name="attributes">The attributes.</param>
		/// <param name="target">The target.</param>
		/// <returns>The generated form element</returns>
		protected static string CreateHtmlId(IDictionary attributes, string target)
		{
			return CreateHtmlId(attributes, target, true);
		}
		
		/// <summary>
		/// Creates the HTML id.
		/// </summary>
		/// <param name="attributes">The attributes.</param>
		/// <param name="target">The target.</param>
		/// <param name="removeEntry">if set to <c>true</c> [remove entry].</param>
		/// <returns>The generated form element</returns>
		protected static string CreateHtmlId(IDictionary attributes, string target, bool removeEntry)
		{
			string id;
			
			if (removeEntry)
			{
				id = CommonUtils.ObtainEntryAndRemove(attributes, "id");
			}
			else
			{
				id = CommonUtils.ObtainEntry(attributes, "id");
			}

			if (id == null)
			{
				id = CreateHtmlId(target);
			}
			
			return id;
		}

		#endregion

		#region private helpers

		private static void ApplyNumberOnlyOptions(IDictionary attributes)
		{
			string list = CommonUtils.ObtainEntryAndRemove(attributes, "exceptions", String.Empty);
			string forbid = CommonUtils.ObtainEntryAndRemove(attributes, "forbid", String.Empty);

			attributes["onKeyPress"] = "return monorail_formhelper_numberonly(event, [" + list + "], [" + forbid + "]);";
		}

		private void AssertIsValidArray(object instance, string property, int index)
		{
			Type instanceType = instance.GetType();

			IList list = instance as IList;

			bool validList = false;

#if DOTNET2
			if (list == null && instanceType.IsGenericType)
			{
				Type[] genArgs = instanceType.GetGenericArguments();

				Type genList = typeof(System.Collections.Generic.IList<>).MakeGenericType(genArgs);
				Type genTypeDef = instanceType.GetGenericTypeDefinition().MakeGenericType(genArgs);

				validList = genList.IsAssignableFrom(genTypeDef);
			}
#endif
			
			if (!validList && list == null)
			{
				throw new RailsException("The property {0} is being accessed as " + 
					"an indexed property but does not seem to implement IList. " + 
					"In fact the type is {1}", property, instanceType.FullName);
			}

			if (index < 0)
			{
				throw new RailsException("The specified index '{0}' is outside the bounds " + 
					"of the array. Property {1}", index, property);
			}
		}

		private object GetArrayElement(object instance, int index)
		{
			IList list = instance as IList;

#if DOTNET2
			if (list == null && instance != null && instance.GetType().IsGenericType)
			{
				Type instanceType = instance.GetType();

				Type[] genArguments = instanceType.GetGenericArguments();

				Type genType = instanceType.GetGenericTypeDefinition().MakeGenericType(genArguments);
				
				// I'm not going to retest for IList implementation as 
				// if we got here, the AssertIsValidArray has run successfully

				PropertyInfo countPropInfo = genType.GetProperty("Count");

				int count = (int) countPropInfo.GetValue(instance, null);
				
				if (count == 0 || index + 1 > count)
				{
					return null;
				}

				PropertyInfo indexerPropInfo = genType.GetProperty("Item");

				return indexerPropInfo.GetValue(instance, new object[] { index });
			}
#endif
			
			if (list == null || list.Count == 0 || index + 1 > list.Count)
			{
				return null;
			}

			return list[index];
		}

		private static bool CheckForExistenceAndExtractIndex(ref string property, out int index)
		{
			bool isIndexed = property.IndexOf('[') != -1;

			index = -1;

			if (isIndexed)
			{
				int start = property.IndexOf('[') + 1;
				int len = property.IndexOf(']', start) - start;

				string indexStr = property.Substring(start, len);

				try
				{
					index = Convert.ToInt32(indexStr);
				}
				catch(Exception)
				{
					throw new RailsException("Could not convert (param {0}) index to Int32. Value is {1}", 
						property, indexStr);
				}

				property = property.Substring(0, start - 1);
			}

			return isIndexed;
		}

		private static bool AreEqual(object left, object right)
		{
			if (left == null || right == null) return false;

			if (left is string || right is String)
			{
				return String.Compare(left.ToString(), right.ToString()) == 0;
			}

			if (left.GetType() == right.GetType())
			{
				return right.Equals(left);
			}

			IConvertible convertible = left as IConvertible;

			if (convertible != null)
			{
				try
				{
					object newleft = convertible.ToType(right.GetType(), null);
					return (newleft.Equals(right));
				}
				catch(Exception)
				{
					// Do nothing
				}
			}

			return left.ToString().Equals(right.ToString());
		}

		/// <summary>
		/// Determines whether the present value matches the value on 
		/// the initialSetValue (which can be a single value or a set)
		/// </summary>
		/// <param name="value">Value from the datasource</param>
		/// <param name="initialSetValue">Value from the initial selection set</param>
		/// <param name="propertyOnInitialSet">Optional. Property to obtain the value from</param>
		/// <param name="isMultiple"><c>true</c> if the initial selection is a set</param>
		/// <returns><c>true</c> if it's selected</returns>
		protected internal static bool IsPresent(object value, object initialSetValue, 
		                                         ValueGetter propertyOnInitialSet, bool isMultiple)
		{
			if (!isMultiple)
			{
				object valueToCompare = initialSetValue;
				
				if (propertyOnInitialSet != null)
				{
					// propertyOnInitialSet.GetValue(initialSetValue, null);
					valueToCompare = propertyOnInitialSet.GetValue(initialSetValue); 
				}
				
				return AreEqual(value, valueToCompare);
			}
			else
			{
				foreach(object item in (IEnumerable) initialSetValue)
				{
					object valueToCompare = item;

					if (propertyOnInitialSet != null)
					{
						// valueToCompare = propertyOnInitialSet.GetValue(item, null);
						valueToCompare = propertyOnInitialSet.GetValue(item); 
					}

					if (AreEqual(value, valueToCompare))
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		// <summary>
		// Gets the property get method.
		// </summary>
		// <param name="elem">Object specifying the type for which to get the method.</param>
		// <param name="property">Property name.</param>
		// <returns><see cref="MethodInfo"/> to be used to retrieve the property value.
		// If <paramref name="property"/> is <c>null</c> <c>null</c> is returned.</returns>
		// <remarks>This method is used to get the <see cref="MethodInfo"/> to retrieve
		// specified property from the specified type.</remarks>
		// <exception cref="ArgumentNullException">Thrown is <paramref name="elem"/> is <c>null</c>.</exception>
//		protected internal static PropertyInfo GetMethoda(object elem, string property)
//		{
//			if (elem == null) throw new ArgumentNullException("elem");
//			if (property == null) return null;
//
//			return GetMethod(elem.GetType(), property);
//		}
//
//		protected internal static PropertyInfo GetMethoda(Type type, string property)
//		{
//			return type.GetProperty(property, PropertyFlags);
//		}

		private static void AddChecked(IDictionary attributes)
		{
			attributes["checked"] = "checked";
		}

		private static void RemoveChecked(IDictionary attributes)
		{
			attributes.Remove("checked");
		}

		private static string CreateHtmlId(string name)
		{
			StringBuilder sb = new StringBuilder(name.Length);

			bool canUseUnderline = false;

			foreach(char c in name.ToCharArray())
			{
				switch(c)
				{
					case '.':
					case '[':
					case ']':
						if (canUseUnderline)
						{
							sb.Append('_');
							canUseUnderline = false;
						}
						break;
					default:
						canUseUnderline = true;
						sb.Append(c);
						break;
				}
				
			}

			return sb.ToString();
		}

		public abstract class ValueGetter
		{
			public abstract string Name { get; }

			public abstract object GetValue(object instance);
		}

		public class ReflectionValueGetter : ValueGetter
		{
			private PropertyInfo propInfo;

			public ReflectionValueGetter(PropertyInfo propInfo)
			{
				this.propInfo = propInfo;
			}

			public override string Name
			{
				get { return propInfo.Name; }
			}

			public override object GetValue(object instance)
			{
				return propInfo.GetValue(instance, null);
			}
		}

		public class DataRowValueGetter : ValueGetter
		{
			private readonly string columnName;

			public DataRowValueGetter(string columnName)
			{
				this.columnName = columnName;
			}

			public override string Name
			{
				get { return columnName; }
			}

			public override object GetValue(object instance)
			{
				DataRow row = (DataRow) instance;

				return row[columnName];
			}
		}

		public class DataRowViewValueGetter : ValueGetter
		{
			private readonly string columnName;

			public DataRowViewValueGetter(string columnName)
			{
				this.columnName = columnName;
			}

			public override string Name
			{
				get { return columnName; }
			}

			public override object GetValue(object instance)
			{
				DataRowView row = (DataRowView)instance;

				return row[columnName];
			}
		}

		public class NoActionGetter : ValueGetter
		{
			public override string Name
			{
				get { return string.Empty; }
			}

			public override object GetValue(object instance)
			{
				return null;
			}
		}

		public class ValueGetterAbstractFactory
		{
			public static ValueGetter Create(Type targetType, string keyName)
			{
				if (targetType == null)
				{
					return new NoActionGetter();
				}
				else if (targetType == typeof(DataRow))
				{
					return new DataRowValueGetter(keyName);
				}
				else if (targetType == typeof(DataRowView))
				{
					return new DataRowViewValueGetter(keyName);
				}
				else
				{
					PropertyInfo info = targetType.GetProperty(keyName, PropertyFlags);
					
					if (info != null)
					{
						return new ReflectionValueGetter(info);
					}

					return null;
				}
			}
		}

		#endregion

		#region FormScopeInfo

		class FormScopeInfo
		{
			private readonly string target;
			private readonly bool isValidationEnabled;

			public FormScopeInfo(string target, bool isValidationEnabled)
			{
				this.target = target;
				this.isValidationEnabled = isValidationEnabled;
			}

			public string RootTarget
			{
				get { return target; }
			}

			public bool IsValidationEnabled
			{
				get { return isValidationEnabled; }
			}
		}

		#endregion
	}
}