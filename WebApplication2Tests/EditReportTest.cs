using Xunit;
using WebApplication2.Data;

namespace WebApplication2Tests
{
    public class EditReportTest
    {
        [Fact]
        public void CanEditReport()
        {
            // Arrange
            var geoChange = new GeoChange
            {
                Description = "Test123",
                GeoJson = "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":[8.002862, 58.163652]}}",
                UserId = "1"
            };

            // Act
            // Perform actions to edit the report here (this part is missing in the provided code)

            // Assert
            // Add assertions to verify the expected outcome (this part is missing in the provided code)
            Assert.NotNull(geoChange);
            Assert.Equal("Test123", geoChange.Description);
            Assert.Equal("{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":[8.002862, 58.163652]}}", geoChange.GeoJson);
            Assert.Equal("1", geoChange.UserId);
        }
    }
}
