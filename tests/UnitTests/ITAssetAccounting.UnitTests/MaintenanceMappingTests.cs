using ITAssetAccounting.MaintenanceService.Entities;
using ITAssetAccounting.MaintenanceService.Mapping;
using ITAssetAccounting.Shared.Enums;
using Xunit;

namespace ITAssetAccounting.UnitTests;

public class MaintenanceMappingTests
{
    [Fact]
    public void ToDto_ShouldIncludeComments()
    {
        var request = new MaintenanceRequest
        {
            Id = 10,
            EquipmentId = 5,
            CreatedByUserId = Guid.NewGuid(),
            Title = "Repair",
            Description = "Broken display",
            Priority = MaintenancePriority.High
        };
        request.Comments.Add(new MaintenanceComment { Id = 1, Text = "Accepted", AuthorUserId = Guid.NewGuid() });

        var dto = request.ToDto();

        Assert.Equal(10, dto.Id);
        Assert.Single(dto.Comments);
        Assert.Equal(MaintenancePriority.High, dto.Priority);
    }
}
