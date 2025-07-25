using System;

namespace Clingies.Tests.Integration;

/// <summary>
/// Collection fixture for ensuring tests are run serial, not parallell
/// and not incur in racing conditions for the in-memory database
/// </summary>
[CollectionDefinition("ClingiesRepositoriesTests")]
public class RepositoryTestsCollection : ICollectionFixture<ClingyRepositoryTests>
{

}
