﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebTestFramework {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal ErrorMessages() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WebTestFramework.ErrorMessages", typeof(ErrorMessages).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Overrides the current thread's CurrentUICulture property for all
        ///    resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to {0} must be between {1} and {2}|{0} must be greater than {1}|{0} must be less than {2}|{0} is not in the required range.
        /// </summary>
        public static string DynamicRangeAttribute {
            get {
                return ResourceManager.GetString("DynamicRangeAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to non è email.
        /// </summary>
        public static string Mail {
            get {
                return ResourceManager.GetString("Mail", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to value is not in the required range.
        /// </summary>
        public static string RangeAttribute {
            get {
                return ResourceManager.GetString("RangeAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to {0} non può essere vuoto.
        /// </summary>
        public static string Richiesto {
            get {
                return ResourceManager.GetString("Richiesto", resourceCulture);
            }
        }
    }
}
