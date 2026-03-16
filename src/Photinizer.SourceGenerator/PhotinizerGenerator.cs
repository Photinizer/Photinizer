using Microsoft.CodeAnalysis;

namespace Photinizer.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class PhotinizerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

    }

}
