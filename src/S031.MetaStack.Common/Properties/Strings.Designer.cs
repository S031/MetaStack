﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace S031.MetaStack.Common.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("S031.MetaStack.Common.Properties.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {CRITICAL}.
        /// </summary>
        internal static string LogLevel_Critical {
            get {
                return ResourceManager.GetString("LogLevel.Critical", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {DEBUG}.
        /// </summary>
        internal static string LogLevel_Debug {
            get {
                return ResourceManager.GetString("LogLevel.Debug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {ERROR}.
        /// </summary>
        internal static string LogLevel_Error {
            get {
                return ResourceManager.GetString("LogLevel.Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {INFO}.
        /// </summary>
        internal static string LogLevel_Info {
            get {
                return ResourceManager.GetString("LogLevel.Info", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string LogLevel_None {
            get {
                return ResourceManager.GetString("LogLevel.None", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {TRACE}.
        /// </summary>
        internal static string LogLevel_Trace {
            get {
                return ResourceManager.GetString("LogLevel.Trace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password length can not be less than 6 characters.
        /// </summary>
        internal static string PasswordGenerator_Generate_1 {
            get {
                return ResourceManager.GetString("PasswordGenerator.Generate.1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to For specified conditions (PasswordGeneratorOptions), the password length can not be less than {0} characters.
        /// </summary>
        internal static string PasswordGenerator_Generate_2 {
            get {
                return ResourceManager.GetString("PasswordGenerator.Generate.2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {WARNING}.
        /// </summary>
        internal static string Warning {
            get {
                return ResourceManager.GetString("Warning", resourceCulture);
            }
        }
    }
}
