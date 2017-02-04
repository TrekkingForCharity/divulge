using System.Linq;
using Mono.Cecil;

public class ModuleWeaver
{
    public ModuleDefinition ModuleDefinition { get; set; }

    public void Execute()
    {
        var types = ModuleDefinition.GetTypes().Where(x => x.HasProperties);
        foreach (var typeDefinition in types)
        {
            var properties = typeDefinition.Properties.Where(x => x.SetMethod.IsPrivate);

            foreach (var propertyDefinition in properties)
                propertyDefinition.SetMethod.IsPublic = true;
        }
    }
}