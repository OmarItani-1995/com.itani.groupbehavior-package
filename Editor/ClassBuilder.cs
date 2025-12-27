using System;
using System.Collections.Generic;
using System.Text;

namespace GroupBehavior.Editors
{
    public class ClassBuilder
    {
        private List<string> usings = new List<string>();
        private string namespaceName;
        private string className;
        private string baseClassName;
        private List<string> baseClassGenericTypes = new List<string>();
        private List<FunctionBuilder> functions = new List<FunctionBuilder>();
    
        public ClassBuilder AddUsing(string usingNamespace)
        {
            usings.Add(usingNamespace);
            return this;
        }
    
        public ClassBuilder SetNamespace(string ns)
        {
            namespaceName = ns;
            return this;
        }
    
        public ClassBuilder SetClassName(string name)
        {
            className = name;
            return this;
        }
    
        public ClassBuilder SetBaseClass(string baseName, params string[] genericTypes)
        {
            baseClassName = baseName;
            baseClassGenericTypes.AddRange(genericTypes);
            return this;
        }
    
        public ClassBuilder AddFunction(Action<FunctionBuilder> builder)
        {
            FunctionBuilder function = new FunctionBuilder();
            builder.Invoke(function);
            functions.Add(function);
            return this;
        }
    
        public string Build()
        {
            StringBuilder code = new StringBuilder();
            int tabbingIndex = 0;
        
            foreach (var u in usings)
            {
                code.Append($"using {u};\n");
            }
        
            if (!string.IsNullOrEmpty(namespaceName))
            {
                code.Append($"\nnamespace {namespaceName}\n{{\n");
                tabbingIndex++;
            }
        
            code.Append($"{GetTabs(tabbingIndex)}public class {className}");
        
            if (!string.IsNullOrEmpty(baseClassName))
            {
                code.Append($" : {baseClassName}");
                if (baseClassGenericTypes.Count > 0)
                {
                    code.Append("<" + string.Join(", ", baseClassGenericTypes) + ">");
                }
            }
        
            code.Append($"{GetTabs(tabbingIndex)}\n{GetTabs(tabbingIndex)}{{\n");
            tabbingIndex++;
            foreach (var function in functions)
            {
                code.Append(function.Build(tabbingIndex));
                code.Append("\n");
            }
            tabbingIndex--;
            code.Append($"{GetTabs(tabbingIndex)}}}");
        
            if (!string.IsNullOrEmpty(namespaceName))
            {
                tabbingIndex--;
                code.Append($"{GetTabs(tabbingIndex)}\n}}");
            }
        
            return code.ToString();
        }

        private string GetTabs(int tabbingIndex)
        {
            return new string('\t', tabbingIndex);
        }
    }

    public class FunctionBuilder
    {
        private string functionName;
        private List<string> parameters = new List<string>();
        private string returnType = "void";
        private string modifier = "public";
        private List<string> bodyLines = new List<string>();
        private List<string> attributes = new List<string>();
    
        public FunctionBuilder SetName(string name)
        {
            functionName = name;
            return this;
        }
    
        public FunctionBuilder AddParameter(string parameter)
        {
            parameters.Add(parameter);
            return this;
        }
    
        public FunctionBuilder SetReturnType(string type)
        {
            returnType = type;
            return this;
        }
    
        public FunctionBuilder SetModifier(string mod)
        {
            modifier = mod;
            return this;
        }
    
        public FunctionBuilder AddBodyLine(string line)
        {
            bodyLines.Add(line);
            return this;
        }
    
        public FunctionBuilder AddAttribute(string attribute)
        {
            attributes.Add(attribute);
            return this;
        }
    
        public string Build(int tabbingIndex = 1)
        {
            StringBuilder code = new StringBuilder();
            string tabs = new string('\t', tabbingIndex);
        
            foreach (var attr in attributes)
            {
                code.Append($"{tabs}[{attr}]\n");
            }
        
            code.Append($"{tabs}{modifier} {returnType} {functionName}(");
            code.Append(string.Join(", ", parameters));
            code.Append($")\n{tabs}{{\n");
        
            foreach (var line in bodyLines)
            {
                code.Append($"{tabs}\t{line}\n");
            }
        
            code.Append($"{tabs}}}");
        
            return code.ToString();
        }
    }
}