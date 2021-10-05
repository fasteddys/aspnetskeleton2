﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 16.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace CodegenTools.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class Command : CommandTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("﻿");
            this.Write("using System;\r\nusing System.Collections.Generic;\r\nusing System.Runtime.Serializat" +
                    "ion;\r\n\r\nnamespace ");
            
            #line 10 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Namespace}.Service.{Group}"));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n    [DataContract]\r\n    public class ");
            
            #line 13 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Name}Command"));
            
            #line default
            #line hidden
            this.Write(" : ");
            
            #line 13 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(IsKeyGenerator ? "IKeyGeneratorCommand" : "ICommand"));
            
            #line default
            #line hidden
            
            #line 13 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(IsEventProducer ? ", IEventProducerCommand" : ""));
            
            #line default
            #line hidden
            this.Write("\r\n    {\r\n        [DataMember(Order = 1)] public string Property { get; set; } = n" +
                    "ull!;\r\n");
            
            #line 16 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"

if (IsEventProducer)
{

            
            #line default
            #line hidden
            this.Write("\r\n        public Action<ICommand, Event>? OnEvent { get; set; }\r\n");
            
            #line 22 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"

}

            
            #line default
            #line hidden
            
            #line 25 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"

if (IsKeyGenerator)
{

            
            #line default
            #line hidden
            this.Write("\r\n        public Action<ICommand, object>? OnKeyGenerated { get; set; }\r\n");
            
            #line 31 "D:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\Command.tt"

}

            
            #line default
            #line hidden
            this.Write("    }\r\n}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
