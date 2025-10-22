using NullOps.ScribanPlayground;
using Scriban;
using Scriban.Runtime;

var scriptObject = new ScriptObject();
scriptObject.Import(typeof(ScribanFunctions));

var templateContext = new TemplateContext();
templateContext.PushGlobal(scriptObject);

var template = Template.Parse("Random UUID: {{ generate_uuid }}!");
var result = template.Render(templateContext);

Console.WriteLine(result);
