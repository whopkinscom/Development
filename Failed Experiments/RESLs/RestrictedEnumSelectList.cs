using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using AutoMapper;
using Moonrise.Utils.Standard.Extensions;

namespace Hanson.Magma.MVC.Web.SiteCode
{
    /// <summary>
    ///     Abstract ancestor for generic RestrictedEnumSelectList
    /// </summary>
    /// This allows the MVC templating to work with a concrete type whilst the generics (which the templating doesn't work with) are actually used.
    public abstract class RestrictedEnumSelectList
    {
        /// <summary>
        ///     The list of enum values to restrict the select list to.
        ///     <para>
        ///         Note: The list has to be passed through as a list of ints. e.g.;
        ///     </para>
        ///     <para>
        ///         [RestrictedTo(new[] {(int)EnumType.Value1,
        ///     </para>
        ///     <para>
        ///         (int)EnumType.Value3,
        ///     </para>
        ///     <para>
        ///         (int)EnumType.Value8})]
        ///     </para>
        /// </summary>
        /// <seealso cref="System.Attribute" />
        protected class RestrictedToAttribute : Attribute
        {
            public RestrictedToAttribute(int[] these)
            {
                OnlyThese = new List<int>(these);
            }

            public List<int> OnlyThese { get; set; }
        }

        private IList onlyThese;

        /// <summary>
        ///     The list of enum values that will be available for selection from.
        /// </summary>
        public IList OnlyThese
        {
            get
            {
                if (onlyThese == null)
                {
                    onlyThese = GetOnlyThese();
                }

                return onlyThese;
            }
            set
            {
                onlyThese = value;
            }
        }

        /// <summary>
        ///     Gets or sets the actual enum value - The type will be as defined by the concrete child instance.
        /// </summary>
        public Enum Value
        {
            get
            {
                // Rely on the concrete implementation to return the actual enum value (which will be as per its defined generic enum type)
                return GetEnumValue();
            }
            set
            {
                // Rely on the concrete implementation to set the actual enum value (which will be as per its defined generic enum type)
                SetEnumValue(value);
            }
        }

        /// <summary>
        ///     Implemented by the generic child class to return the enum value
        /// </summary>
        /// <returns>An enum value whose actual type will actually be determined by the child class</returns>
        public abstract Enum GetEnumValue();

        /// <summary>
        ///     Implemented by the generic child class to set the enum value
        /// </summary>
        /// <param name="value">An enum value whose actual type will actually be determined by the child class</param>
        public abstract void SetEnumValue(Enum value);

        /// <summary>
        ///     Implemented by the generic child class to ensure that any <see cref="RestrictedToAttribute" /> gets applied.
        /// </summary>
        /// <returns></returns>
        protected abstract IList GetOnlyThese();
    }

    /// <summary>
    ///     The generic implementation of a RESL
    /// </summary>
    /// NOTE: In order to be able to use this class you will also need to add automapper entries and
    /// model binder entries - typically in Global.asax.cs. e.g.;
    ///        
    /// // We need mappings for converting to and from any RESLs
    /// CreateMap{EnumType, RestrictedEnumSelectList{EnumType}}().ConvertUsing{EnumToRESLConverter{EnumType}}();
    /// CreateMap{RestrictedEnumSelectList{EnumType}, EnumType}().ConvertUsing{RESLToEnumConverter{EnumType}}();
    /// 
    /// // Ensure that the view model binder is able to recognise any new types we may have created.
    /// ModelBinders.Binders.Add(typeof(RestrictedEnumSelectList{EnumType}), new RESLModelBinder{EnumType}());
    /// <typeparam name="T">This needs to be an Enum</typeparam>
    public class RestrictedEnumSelectList<T> : RestrictedEnumSelectList
        where T : IConvertible
    {
        /// <summary>
        ///     Constructs a RESL with the enum set to the default value
        /// </summary>
        public RestrictedEnumSelectList()
        {
            Value = default(T);
        }

        /// <summary>
        ///     Constructs a RESL with the enum set to the value of the parsed string, or the default value if not a valid
        ///     description for the enum
        /// </summary>
        /// <param name="value">The string value of the enum. Either its string value, its Description or its modified Description</param>
        public RestrictedEnumSelectList(string value)
        {
            Value = ParseEnum(value);
        }

        /// <summary>
        ///     Constructs a RESL with the enum set to the passed value
        /// </summary>
        /// <param name="value">The value of the enum</param>
        public RestrictedEnumSelectList(T value)
        {
            Value = value;
        }

        /// <summary>
        ///     Constructs a RESL with a restricted list of values
        /// </summary>
        /// <param name="onlyThese"></param>
        public RestrictedEnumSelectList(IList<T> onlyThese)
        {
            Value = default(T);
            OnlyThese = (IList)onlyThese;
        }

        /// <summary>
        ///     When interacting with an instance of a RESL the type of value will always be the actual Enum type
        /// </summary>
        public new T Value { get; set; }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="RestrictedEnumSelectList{T}" /> to <see cref="T" />.
        /// </summary>
        /// <param name="resl">The resl.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator T(RestrictedEnumSelectList<T> resl)
        {
            return resl.Value;
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="T" /> to <see cref="RestrictedEnumSelectList{T}" />.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator RestrictedEnumSelectList<T>(T enumValue)
        {
            return new RestrictedEnumSelectList<T>(enumValue);
        }

        /// <summary>
        ///     Concrete implementation to return the actual enum of the correct type
        /// </summary>
        /// This whole mechanism is what allows the ParseEnum to work correctly
        /// <returns>The enum value</returns>
        public override Enum GetEnumValue()
        {
            return (Enum)(object)Value;
        }

        /// <summary>
        ///     Parses a string into an actual enum value of the specific enum type.
        /// </summary>
        /// <param name="value">A string representing the value of a particular enum type</param>
        /// The parsing works as per the EnumExtensions.FromString method.
        /// <returns>The parsed value</returns>
        public T ParseEnum(string value)
        {
            object enumValue = Moonrise.Utils.Standard.Extensions.EnumExtensions.FromString(value, default(T));
            return (T)enumValue;
        }

        /// <summary>
        ///     Concrete implementation to set the actual enum of the correct type
        /// </summary>
        /// This whole mechanism is what allows the ParseEnum to work correctly
        /// <param name="value">Although the value is passed as an Enum it will still be the specific enum</param>
        public override void SetEnumValue(Enum value)
        {
            Value = (T)(object)value;
        }

        /// <summary>
        ///     Implemented by the generic child class to ensure that any <see cref="RestrictedToAttribute" /> gets applied.
        /// </summary>
        /// <returns></returns>
        protected override IList GetOnlyThese()
        {
            Type enumType = GetType();
            RestrictedToAttribute attribute = (RestrictedToAttribute)enumType.GetCustomAttribute(typeof(RestrictedToAttribute));
            IList retVal = attribute.OnlyThese.Cast<T>().ToList();
            return retVal;
        }
    }

    /// <summary>
    ///     An MVC model binder that allows a specific enum typed RESL to be populated from a query string
    /// </summary>
    /// This needs to be utilised in the HttpApplication implementation's Application_Start() like so;
    /// 
    /// ModelBinders.Binders.Add(typeof(RestrictedEnumSelectList{EnumType}), new ReslModelBinder{EnumType}());
    /// <typeparam name="T">The specific enum type</typeparam>
    public class ReslModelBinder<T> : DefaultModelBinder
        where T : RestrictedEnumSelectList, new()
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == null)
            {
                return base.BindModel(controllerContext, bindingContext);
            }

            Type type = typeof(T);
            Type baseType = type?.BaseType;
            Type genericType = baseType?.GetGenericArguments()[0];

            object enumValue =
                Moonrise.Utils.Standard.Extensions.EnumExtensions.FromString(valueProviderResult.AttemptedValue, null, genericType, true);
            T result = new T();
            result.SetEnumValue((Enum)enumValue);

            return result;
        }
    }

    /// <summary>
    ///     Converts Enums to RESLs(Restricted Enum Select Lists)
    /// </summary>
    public class EnumToRESLConverter<T> : ITypeConverter<T, RestrictedEnumSelectList<T>>
        where T : IConvertible
    {
        public RestrictedEnumSelectList<T> Convert(T source, RestrictedEnumSelectList<T> destination, ResolutionContext context)
        {
            return new RestrictedEnumSelectList<T>(source);
        }
    }

    /// <summary>
    ///     Converts RESLs(Restricted Enum Select Lists) to Enums
    /// </summary>
    public class RESLToEnumConverter<T> : ITypeConverter<RestrictedEnumSelectList<T>, T>
        where T : IConvertible
    {
        public T Convert(RestrictedEnumSelectList<T> source, T destination, ResolutionContext context)
        {
            return source.Value;
        }
    }

    /// <summary>
    ///     Enum extensions for use with MVC
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Converts an enum, or Nullable enum, value into a list of possible values (as per the enum's Description attribute)
        ///     and ensures the current value is selected.
        /// </summary>
        /// Extends an Enum - but has MVC specific stuff, so is in this section of extensions
        /// Use like this;
        /// @Html.DropDownListFor(model => model.EnumVariable, @Model.EnumVariable.ToSelectList())
        /// <typeparam name="TEnum">The enum type being extended</typeparam>
        /// <param name="enumObj">The enum variable to call the extension on</param>
        /// <param name="onlyThese">
        ///     List of enum values that are allowed to be in the select list - defaults to null, meaning ALL
        ///     values are returned
        /// </param>
        /// <returns>An MVC SelectList with the appropriate descriptions and current value selected.</returns>
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, IList onlyThese = null)
            where TEnum : IComparable, IFormattable, IConvertible
        {
            SelectList retVal;
            Type eNum = enumObj.GetType();

            // Just check if it is a Nullable<Enum>
            if (eNum.IsGenericType && (eNum.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                eNum = eNum.GenericTypeArguments[0];
            }

            if (onlyThese == null)
            {
                var values = from TEnum e in Enum.GetValues(eNum)
                             select new
                                    {
                                        Id = e,
                                        Name = ((Enum)(object)e).Description()
                                    };
                retVal = new SelectList(values, "Id", "Name", enumObj);
            }
            else
            {
                var values = from TEnum e in Enum.GetValues(eNum)
                             where onlyThese.Contains(e)
                             select new
                                    {
                                        Id = e,
                                        Name = ((Enum)(object)e).Description()
                                    };
                retVal = new SelectList(values, "Id", "Name", enumObj);
            }

            return retVal;
        }
    }
}
