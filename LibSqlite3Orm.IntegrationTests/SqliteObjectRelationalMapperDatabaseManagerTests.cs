using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.IntegrationTests.TestDataModel;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class SqliteObjectRelationalMapperDatabaseManagerTests : IntegrationTestBase<TestDbContext>
{
    private ISqliteConnection testConnection;
    
    [Test]
    public void CreateDatabase_WhenDbNotSetup_DoesNotThrow()
    {
        using var systemUnderTest = CreateSystemUnderTest();

        Assert.DoesNotThrow(systemUnderTest.CreateDatabase);
    }
    
    [Test]
    public void CreateDatabase_WhenDbNotSetup_CreatesTables()
    {
        using var systemUnderTest = CreateSystemUnderTest();

        systemUnderTest.CreateDatabase();
        
        using var orm = CreateOrm<TestDbContext>();

        var records = new List<TestEntityTag>();
        for (var i = 1; i < 10; i++)
            records.Add(CreateTestEntityTagWithRandomValues());
        orm.InsertMany(records);

        var actualRecords = orm.Get<TestEntityTag>().OrderBy(x => x.Id).AllRecords();
        Assert.That(actualRecords.Length, Is.EqualTo(records.Count));
        for (var i = 0; i < records.Count; i++)
            AssertThatRecordsMatch(records[i], actualRecords[i]);
    }
    
    [Test]
    public void CreateDatabase_WhenDbSetup_Throws()
    {
        using var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.CreateDatabase();

        Assert.Throws<InvalidOperationException>(systemUnderTest.CreateDatabase, "The database already created and initialized.");
    }    
    
    [Test]
    public void IsDatabaseInitialized_WhenDbSetup_ReturnsTrue()
    {
        using var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.CreateDatabase();

        var actual = systemUnderTest.IsDatabaseInitialized();
        
        Assert.That(actual, Is.True);
    }
    
    [Test]
    public void IsDatabaseInitialized_WhenDbNotSetup_ReturnsFalse()
    {
        using var systemUnderTest = CreateSystemUnderTest();
        
        var actual = systemUnderTest.IsDatabaseInitialized();
        
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void Migrate_WhenNoSchemaDifference_ReturnsFalse()
    {
        using var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.CreateDatabase();
        
        var actual = systemUnderTest.Migrate();
        
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void Migrate_WhenFormatVersionFieldMissingAndNoSchemaDifference_ReturnsFalse()
    {
        using var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.CreateDatabase();

        using var schemaOrm = CreateOrm<SqliteOrmSchemaContext>();
        var migrationRecord = schemaOrm.Get<SchemaMigration>().OrderByDescending(x => x.Timestamp).AsEnumerable().First();
        migrationRecord.Schema = migrationRecord.Schema.Replace("\"FormatVersion\": 1,", string.Empty);
        schemaOrm.Update(migrationRecord);
        
        var actual = systemUnderTest.Migrate();
        
        Assert.That(actual, Is.False);
    }    
    
    [Test]
    public void Migrate_WhenMostRecentModelSchemaVersionTooOld_Throws()
    {
        using var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.CreateDatabase();

        using var schemaOrm = CreateOrm<SqliteOrmSchemaContext>();
        var migrationRecord = schemaOrm.Get<SchemaMigration>().OrderByDescending(x => x.Timestamp).AsEnumerable().First();
        migrationRecord.Schema = migrationRecord.Schema.Replace("\"FormatVersion\": 1,", "\"FormatVersion\": -1,");
        schemaOrm.Update(migrationRecord);

        var ex = Assert.Throws<InvalidDataException>(() => systemUnderTest.Migrate());
        Assert.That(ex.Message,
            Is.EqualTo(
                "The database is not compatible with this version of LibSqlite3Orm.\n\nDatabase ORM Schema Format Version: -1\nOldest ORM Schema Format Version Supported By Library: 0"));
    }     
    
    [Test]
    public void Migrate_WhenSchemaDifferenceDetected_ReturnsTrueAndMigrationApplied()
    {
        CreateDatabase();

        using var systemUnderTest = CreateSystemUnderTest<TestDbContextAddedTable>();
        
        var actual = systemUnderTest.Migrate();
        Assert.That(actual, Is.True);

        using var orm = CreateOrm<TestDbContextAddedTable>();

        var newRecord = new TestEntityAdded { NewDataField = "Hai!" };
        var recordAdded = orm.Insert(newRecord);
        Assert.That(recordAdded, Is.True);

        var fetchedRecord = orm.Get<TestEntityAdded>().Where(x => x.Id == newRecord.Id).SingleRecord();
        AssertThatRecordsMatch(fetchedRecord, newRecord);
    }

    [Test]
    public void DeleteDatabase_WheInvoked_DeletesDatabaseFile()
    {
        var tempFile = Path.GetTempFileName();
        using var connection = Resolve<Func<ISqliteConnection>>().Invoke();
        connection.OpenReadWrite(tempFile, mustExist: false);
        Assert.That(File.Exists(tempFile), Is.True);
        using var systemUnderTest = CreateSystemUnderTest(connection);
        systemUnderTest.CreateDatabase();
        
        systemUnderTest.DeleteDatabase();
        
        Assert.That(File.Exists(tempFile), Is.False);
    }

    public override void SetUp()
    {
        base.SetUp();
        testConnection = Resolve<Func<ISqliteConnection>>().Invoke();
        testConnection.OpenInMemory();
    }

    public override void TearDown()
    {
        testConnection.Dispose();
        base.TearDown();
    }

    private void CreateDatabase()
    {
        using var dbMan = CreateSystemUnderTest();
        dbMan.CreateDatabase();
    }
    
    private ISqliteObjectRelationalMapperDatabaseManager<TestDbContext> CreateSystemUnderTest(ISqliteConnection connection = null) 
    {
        return CreateSystemUnderTest<TestDbContext>(connection ?? testConnection);
    }    

    private ISqliteObjectRelationalMapperDatabaseManager<TContext> CreateSystemUnderTest<TContext>(ISqliteConnection connection = null) 
        where TContext : ISqliteOrmDatabaseContext
    {
        var result = Resolve<Func<ISqliteObjectRelationalMapperDatabaseManager<TContext>>>().Invoke();
        result.UseConnection(connection ?? testConnection);
        return result;
    }
    
    private ISqliteObjectRelationalMapper<TContext> CreateOrm<TContext>(ISqliteConnection connection = null) 
        where TContext : ISqliteOrmDatabaseContext
    {
        var result = Resolve<Func<ISqliteObjectRelationalMapper<TContext>>>().Invoke();
        result.UseConnection(connection ?? testConnection);
        return result;
    }    
}