using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Xunit;
using NSubstitute;
using hobio.api.Services;

namespace hobio.tests.Services;

public class StorageServiceTests
{
    [Fact]
    public void Constructor_WithMissingBucketName_ThrowsArgumentNullException()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["REPORTS_BUCKET_NAME"].Returns((string?)null);

        var credential = GoogleCredential.FromAccessToken("fake-token");

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new StorageService(config, credential));
        Assert.Contains("REPORTS_BUCKET_NAME not set", ex.Message);
    }

    [Fact]
    public void Constructor_WithValidBucketName_Succeeds()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["REPORTS_BUCKET_NAME"].Returns("test-bucket");

        var credential = GoogleCredential.FromAccessToken("fake-token");

        // Act
        var service = new StorageService(config, credential);

        // Assert
        Assert.NotNull(service);
    }
}
