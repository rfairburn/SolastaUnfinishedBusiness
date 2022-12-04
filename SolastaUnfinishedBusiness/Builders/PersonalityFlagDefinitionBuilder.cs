using System;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.Builders;

[UsedImplicitly]
internal class
    PersonalityFlagDefinitionBuilder : DefinitionBuilder<PersonalityFlagDefinition, PersonalityFlagDefinitionBuilder>
{
    #region Constructors

    protected PersonalityFlagDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected PersonalityFlagDefinitionBuilder(PersonalityFlagDefinition original, string name, Guid namespaceGuid)
        : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
