namespace PhotinizerNET.UI.Own;

internal static class ComponentDependencyResolver
{
    public static List<Component> OrderComponents(IEnumerable<Component> components)
    {
        var sorted = new List<Component>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>(); // Для поиска циклов

        var componentsDict = components.ToDictionary(m => m.FilePath);

        void Visit(Component component)
        {
            if (visited.Contains(component.FilePath)) return;
            if (visiting.Contains(component.FilePath))
                throw new Exception($"Обнаружена циклическая зависимость: {component.FilePath}");

            visiting.Add(component.FilePath);

            foreach (var depName in component.Dependencies)
            {
                if (componentsDict.TryGetValue(depName, out var depComponent))
                    Visit(depComponent);
            }

            visiting.Remove(component.FilePath);
            visited.Add(component.FilePath);
            sorted.Add(component);
        }

        foreach (var component in components)
            Visit(component);

        return sorted;
    }
}