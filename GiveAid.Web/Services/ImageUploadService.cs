namespace GiveAid.Web.Services;

/// <summary>
/// Service for safely handling image file uploads.
/// Validates file extensions, MIME signatures, and size limits.
/// Generates unique server-side filenames to prevent path traversal and collisions.
/// </summary>
public class ImageUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;

    /// <summary>
    /// Allowed image extensions and their corresponding magic byte signatures.
    /// FR-037 & FR-047: .jpg, .jpeg, .png, .webp only.
    /// </summary>
    private static readonly Dictionary<string, byte[][]> AllowedSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",  new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png",  new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } // RIFF header
    };

    /// <summary>
    /// Default maximum allowed file size: 5 MB (FR-047, configurable per upload).
    /// </summary>
    public const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024;

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Validates and saves an uploaded image file to the specified subfolder under wwwroot/uploads/.
    /// Returns the relative URL path on success, or null with error message on failure.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="subfolder">Subfolder name under uploads/ (e.g., "ngos", "partners", "programmes", "gallery").</param>
    /// <param name="maxSizeBytes">Optional maximum file size in bytes (defaults to 5 MB).</param>
    /// <returns>Tuple of (relativePath, errorMessage). One will be null.</returns>
    public async Task<(string? RelativePath, string? ErrorMessage)> SaveImageAsync(
        IFormFile file,
        string subfolder,
        long maxSizeBytes = DefaultMaxFileSizeBytes)
    {
        if (file == null || file.Length == 0)
            return (null, "No file was uploaded.");

        // 1. Validate file size against configurable limit
        if (file.Length > maxSizeBytes)
        {
            var maxMb = maxSizeBytes / (1024 * 1024.0);
            return (null, $"File size ({file.Length / (1024 * 1024.0):F1} MB) exceeds the maximum allowed size of {maxMb:F0} MB.");
        }

        // 2. Validate extension (do not trust browser-supplied filename alone)
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedSignatures.ContainsKey(extension))
            return (null, $"File type '{extension}' is not allowed. Permitted formats: .jpg, .jpeg, .png, .webp.");

        // 3. Validate file content magic bytes (do not trust MIME type alone)
        var isValidContent = await ValidateFileSignatureAsync(file, extension);
        if (!isValidContent)
            return (null, "The uploaded file content does not match the expected image format. Please upload a genuine image file.");

        // 4. Generate safe unique filename (prevents path traversal and collisions)
        var safeFileName = $"{Guid.NewGuid():N}{extension}";

        // 5. Ensure upload directory exists
        var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadDir);

        // 6. Save file
        var fullPath = Path.Combine(uploadDir, safeFileName);
        try
        {
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save uploaded image to {Path}", fullPath);
            return (null, "An error occurred while saving the uploaded file. Please try again.");
        }

        // 7. Return relative URL path for database storage
        var relativePath = $"/uploads/{subfolder}/{safeFileName}";
        _logger.LogInformation("Image uploaded successfully: {Path}", relativePath);
        return (relativePath, null);
    }

    /// <summary>
    /// Deletes an uploaded image file from disk given its relative URL path.
    /// Validates the path to prevent deletion of unrelated files.
    /// </summary>
    public bool DeleteImage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        // Ensure the path is within the uploads directory (prevent directory traversal)
        if (!relativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Attempted to delete file outside uploads directory: {Path}", relativePath);
            return false;
        }

        // Additional safety: reject paths with directory traversal sequences
        if (relativePath.Contains("..") || relativePath.Contains('\\'))
        {
            _logger.LogWarning("Rejected suspicious delete path: {Path}", relativePath);
            return false;
        }

        var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));

        if (!File.Exists(fullPath))
            return false;

        try
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted uploaded image: {Path}", relativePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image at {Path}", fullPath);
            return false;
        }
    }

    /// <summary>
    /// Validates that the file content matches expected magic byte signatures for the given extension.
    /// </summary>
    private static async Task<bool> ValidateFileSignatureAsync(IFormFile file, string extension)
    {
        if (!AllowedSignatures.TryGetValue(extension, out var signatures))
            return false;

        using var reader = new BinaryReader(file.OpenReadStream());
        var maxSigLength = signatures.Max(s => s.Length);
        var headerBytes = reader.ReadBytes(maxSigLength);

        // Reset stream position for subsequent reads
        file.OpenReadStream().Position = 0;

        return signatures.Any(signature =>
            headerBytes.Length >= signature.Length &&
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
