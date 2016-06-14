﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rezolver {
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
    public class ExceptionResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal ExceptionResources() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Rezolver.ExceptionResources", typeof(ExceptionResources).GetTypeInfo().Assembly);
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
        ///    Looks up a localized string similar to Cyclic dependency detected in targets - current target of type {0} with DeclaredType of {1} has tried to include itself in its expression..
        /// </summary>
        public static string CyclicDependencyDetectedInTargetFormat {
            get {
                return ResourceManager.GetString("CyclicDependencyDetectedInTargetFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The declared type {0} is not compatible with the type {1}.
        /// </summary>
        public static string DeclaredTypeIsNotCompatible_Format {
            get {
                return ResourceManager.GetString("DeclaredTypeIsNotCompatible_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The body of the lambda &quot;{0}&quot; is not a NewExpression.
        /// </summary>
        public static string LambdaBodyIsNotNewExpressionFormat {
            get {
                return ResourceManager.GetString("LambdaBodyIsNotNewExpressionFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The expression {0} does not represent calling a constructor of the type {1}.
        /// </summary>
        public static string LambdaBodyNewExpressionIsWrongTypeFormat {
            get {
                return ResourceManager.GetString("LambdaBodyNewExpressionIsWrongTypeFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to More than one constructor for {0} qualifies as a target for Auto construction.
        /// </summary>
        public static string MoreThanOneConstructorFormat {
            get {
                return ResourceManager.GetString("MoreThanOneConstructorFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to More than one matching object was found in the scope.
        /// </summary>
        public static string MoreThanOneObjectFoundInScope {
            get {
                return ResourceManager.GetString("MoreThanOneObjectFoundInScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to No constructor has been set on the NewExpression - this is not allowed..
        /// </summary>
        public static string NoConstructorSetOnNewExpression {
            get {
                return ResourceManager.GetString("NoConstructorSetOnNewExpression", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The type {0} has no default constructor, nor any constructors where all the parameters are optional..
        /// </summary>
        public static string NoDefaultOrAllOptionalConstructorFormat {
            get {
                return ResourceManager.GetString("NoDefaultOrAllOptionalConstructorFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to No public constructors declared on the type {0}.
        /// </summary>
        public static string NoPublicConstructorsDefinedFormat {
            get {
                return ResourceManager.GetString("NoPublicConstructorsDefinedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to This method is not to be called at run-time - it is only used for static expression analysis in creating IRezolveTargets for an IRezolveBuilder.
        /// </summary>
        public static string NotRuntimeMethod {
            get {
                return ResourceManager.GetString("NotRuntimeMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to path&apos;s Next must not be null - pass path as null once it&apos;s reached the last item.
        /// </summary>
        public static string PathIsAtEnd {
            get {
                return ResourceManager.GetString("PathIsAtEnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The path {0} is invalid.  All path steps must contain non-whitespace characters and be at least one character in length.
        /// </summary>
        public static string PathIsInvalid {
            get {
                return ResourceManager.GetString("PathIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to No more targets can be added to this entry.
        /// </summary>
        public static string RezolverTargetEntryHasBeenRealised {
            get {
                return ResourceManager.GetString("RezolverTargetEntryHasBeenRealised", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to A lifetime scope is required for a scoped singleton.
        /// </summary>
        public static string ScopedSingletonRequiresAScope {
            get {
                return ResourceManager.GetString("ScopedSingletonRequiresAScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The target does not support the type {0}.
        /// </summary>
        public static string TargetDoesntSupportType_Format {
            get {
                return ResourceManager.GetString("TargetDoesntSupportType_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The type {0} is not a nullable type.
        /// </summary>
        public static string TargetIsNullButTypeIsNotNullable_Format {
            get {
                return ResourceManager.GetString("TargetIsNullButTypeIsNotNullable_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Target of type {0} returned a null expression for context {1} - implementation is invalid, targets must never return a null expression..
        /// </summary>
        public static string TargetReturnedNullExpressionFormat {
            get {
                return ResourceManager.GetString("TargetReturnedNullExpressionFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The type {0} has already been registered.
        /// </summary>
        public static string TypeIsAlreadyRegistered {
            get {
                return ResourceManager.GetString("TypeIsAlreadyRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Unable to resolve type {0} from builder.
        /// </summary>
        public static string UnableToResolveTypeFromBuilderFormat {
            get {
                return ResourceManager.GetString("UnableToResolveTypeFromBuilderFormat", resourceCulture);
            }
        }
    }
}