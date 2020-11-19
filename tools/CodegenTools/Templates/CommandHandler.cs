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
    
    #line 1 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class CommandHandler : CommandTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("﻿");
            this.Write("using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Syste" +
                    "m.Threading;\r\nusing System.Threading.Tasks;\r\nusing Microsoft.EntityFrameworkCore" +
                    ";\r\nusing ");
            
            #line 12 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Namespace}.DataAccess.Entities"));
            
            #line default
            #line hidden
            this.Write(";\r\n\r\nnamespace ");
            
            #line 14 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Namespace}.Service.{Group}"));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n    internal sealed class ");
            
            #line 16 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Name}CommandHandler"));
            
            #line default
            #line hidden
            this.Write(" : CommandHandler<");
            
            #line 16 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Name}Command"));
            
            #line default
            #line hidden
            this.Write(">\r\n    {\r\n        public override async Task HandleAsync(");
            
            #line 18 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Name}Command"));
            
            #line default
            #line hidden
            this.Write(@" command, CommandContext context, CancellationToken cancellationToken)
        {
            Entity entity = await context.DbContext.Entities
                .FindAsync(new object[] { command.Id }, cancellationToken).ConfigureAwait(false);

            RequireExisting(entity, c => c.Id);

            // ...
");
            
            #line 26 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"

if (IsProgressReporter)
{

            
            #line default
            #line hidden
            this.Write("\r\n            command.Progress?.Report(new ProgressEventData\r\n            {\r\n    " +
                    "            StatusText = \"Finished\",\r\n                Progress = 1f\r\n           " +
                    " });\r\n");
            
            #line 36 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"

}

            
            #line default
            #line hidden
            this.Write("\r\n            await context.DbContext.SaveChangesAsync(cancellationToken).Configu" +
                    "reAwait(false);\r\n");
            
            #line 41 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"

if (IsKeyGenerator)
{

            
            #line default
            #line hidden
            this.Write("\r\n            command.OnKeyGenerated?.Invoke(command, entity.Id);\r\n");
            
            #line 47 "d:\Dev\_Skeletons\AspNetSkeleton\V2\tools\CodegenTools\Templates\CommandHandler.tt"

}

            
            #line default
            #line hidden
            this.Write("        }\r\n    }\r\n}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
