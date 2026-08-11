using Bogus;
using Microsoft.AspNetCore.Identity;
using SchoolERP.Domain.Shared;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.IntegrationTests.Fixtures;

public class BogusDataFixture
{
    private readonly Faker _faker = new Faker();
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly Guid _tenantId;

    public BogusDataFixture(UserManager<ApplicationUser> userManager, AppDbContext dbContext, Guid tenantId)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tenantId = tenantId;
    }

    // 🔥 Fake Student Generator
    //public Student GenerateFakeStudent()
    //{
    //    return new Student
    //    {
    //        Id = Guid.NewGuid(),
    //        TenantId = _tenantId,
    //        FirstName = _faker.Name.FirstName(),
    //        LastName = _faker.Name.LastName(),
    //        DateOfBirth = _faker.Date.Past(10, DateTime.Now.AddYears(-5)),
    //        Gender = _faker.PickRandom(new[] { "Male", "Female", "Other" }),
    //        PersonalEmail = _faker.Internet.Email(),
    //        PersonalMobile = _faker.Phone.PhoneNumber("##########"),
    //        Status = "active"
    //    };
    //}

    // 🔥 Fake User Generator
    public ApplicationUser GenerateFakeUser(string role = "Teacher")
    {
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var email = $"{firstName.ToLower()}.{lastName.ToLower()}@school.com";

        return new ApplicationUser
        {
            Id = _faker.Random.Long(1, 10000),
            UserName = email,
            Email = email,
            Name = $"{firstName} {lastName}",
            TenantId = _tenantId,
            Status = "active"
        };
    }

    // 🔥 Bulk Generate Students
    //public List<Student> GenerateFakeStudents(int count)
    //{
    //    var students = new List<Student>();
    //    for (int i = 0; i < count; i++)
    //    {
    //        students.Add(GenerateFakeStudent());
    //    }
    //    return students;
    //}

    //// 🔥 Save Generated Data to DB
    //public async Task SeedFakeStudentsAsync(int count)
    //{
    //    var students = GenerateFakeStudents(count);
    //    await _dbContext.Students.AddRangeAsync(students);
    //    await _dbContext.SaveChangesAsync();
    //}
}