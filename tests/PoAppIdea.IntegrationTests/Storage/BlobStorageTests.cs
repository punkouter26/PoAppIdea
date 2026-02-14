using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Testcontainers.Azurite;

namespace PoAppIdea.IntegrationTests.Storage;

/// <summary>
/// Integration tests for Azure Blob Storage operations.
/// Uses Testcontainers Azurite for ephemeral storage.
/// Verifies: Upload, download, metadata operations.
/// </summary>
public sealed class BlobStorageTests : IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.35.0")
        .WithCommand("--skipApiVersionCheck")
        .Build();

    private BlobServiceClient _blobServiceClient = null!;

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
        _blobServiceClient = new BlobServiceClient(_azuriteContainer.GetConnectionString());
        Console.WriteLine("[Test] Azurite Blob Storage container started");
    }

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    [Fact]
    public async Task BlobClient_ShouldCreate_Container()
    {
        // Arrange
        var containerName = $"artifacts-{Guid.NewGuid():N}";

        // Act
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        // Assert
        var exists = await containerClient.ExistsAsync();
        exists.Value.Should().BeTrue();
    }

    [Fact]
    public async Task BlobClient_ShouldUpload_TextContent()
    {
        // Arrange
        var containerName = $"prds-{Guid.NewGuid():N}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = "prd-12345.md";
        var content = "# Product Requirements Document\n\n## Overview\nThis is a test PRD.";

        // Act
        var blobClient = containerClient.GetBlobClient(blobName);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true);

        // Assert
        var downloadResult = await blobClient.DownloadContentAsync();
        var downloadedContent = downloadResult.Value.Content.ToString();
        downloadedContent.Should().Contain("Product Requirements Document");
    }

    [Fact]
    public async Task BlobClient_ShouldUpload_WithMetadata()
    {
        // Arrange
        var containerName = $"images-{Guid.NewGuid():N}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = "mockup-12345.png";
        var metadata = new Dictionary<string, string>
        {
            { "sessionId", Guid.NewGuid().ToString() },
            { "artifactType", "mockup" },
            { "generatedBy", "dall-e-3" }
        };

        // Act
        var blobClient = containerClient.GetBlobClient(blobName);
        using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]); // PNG magic bytes
        await blobClient.UploadAsync(stream, new BlobUploadOptions { Metadata = metadata });

        // Assert
        var properties = await blobClient.GetPropertiesAsync();
        properties.Value.Metadata.Should().ContainKey("sessionId");
        properties.Value.Metadata["artifactType"].Should().Be("mockup");
    }

    [Fact]
    public async Task BlobClient_ShouldList_Blobs()
    {
        // Arrange
        var containerName = $"specs-{Guid.NewGuid():N}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        for (int i = 0; i < 3; i++)
        {
            var blobClient = containerClient.GetBlobClient($"spec-{i}.yaml");
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes($"openapi: 3.0.{i}"));
            await blobClient.UploadAsync(stream);
        }

        // Act
        var blobs = new List<string>();
        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            blobs.Add(blobItem.Name);
        }

        // Assert
        blobs.Should().HaveCount(3);
        blobs.Should().Contain("spec-0.yaml");
    }

    [Fact]
    public async Task BlobClient_ShouldDelete_Blob()
    {
        // Arrange
        var containerName = $"temp-{Guid.NewGuid():N}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient("to-delete.txt");
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("temporary"));
        await blobClient.UploadAsync(stream);

        // Act
        await blobClient.DeleteAsync();

        // Assert
        var exists = await blobClient.ExistsAsync();
        exists.Value.Should().BeFalse();
    }
}
