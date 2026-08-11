//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using SchoolERP.IntegrationTests.TestBase;

//namespace SchoolERP.IntegrationTests;

//public class TenantIsolationTests : TestBase
//{
//    public TenantIsolationTests(CustomWebApplicationFactory factory, DatabaseFixture dbFixture)
//        : base(factory, dbFixture) { }

//    [Fact]
//    public async Task Students_From_Different_Tenants_Should_Be_Isolated()
//    {
//        // 1. Create students for tenant1 and tenant2
//        var tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
//        var tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

//        var student1 = BogusData.GenerateFakeStudent();
//        student1.TenantId = tenant1Id;

//        var student2 = BogusData.GenerateFakeStudent();
//        student2.TenantId = tenant2Id;

//        await DbContext.Students.AddRangeAsync(student1, student2);
//        await DbContext.SaveChangesAsync();

//        // 2. Query as tenant1
//        var tenant1Students = await DbContext.Students
//            .Where(s => s.TenantId == tenant1Id)
//            .ToListAsync();

//        // 3. Assert
//        tenant1Students.Should().Contain(s => s.Id == student1.Id);
//        tenant1Students.Should().NotContain(s => s.Id == student2.Id);
//    }

//    [Fact]
//    public async Task EF_GlobalQueryFilter_Should_Isolate_Data_Automatically()
//    {
//        // 🔥 Test: Global Query Filter automatically adds TenantId WHERE clause
//        // Implementation: Use a mock ICurrentTenantService to set tenant1 context
//        // Then query Students table and assert only tenant1 students are returned
//    }

//    [Fact]
//    public async Task DapperRepository_Should_Isolate_Data_By_Tenant()
//    {
//        // 🔥 Test: Dapper's tenant-aware repository should auto-filter
//        // Similar to above but using IDapperRepository<Student>
//    }
//}