using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Exceptions;
using ImageMagick;

namespace APortataDiLibro.Infrastructure.Covers;

/// <summary>
/// Implementazione basata su Magick.NET: normalizza le copertine caricate in
/// JPEG, applica l'orientamento EXIF e produce sia l'immagine a piena
/// risoluzione sia la thumbnail, salvandole sotto <c>wwwroot/covers</c> dove
/// vengono servite come file statici dal layer API.
/// </summary>
public class MagickCoverImageService : ICoverImageService
{
    private readonly string _coversRootPath;

    public MagickCoverImageService()
    {
        _coversRootPath = Path.Combine(Directory.GetCurrentDirectory(), StorageConstants.CoversRootFolder);
    }

    public Task<(string PercorsoCopertina, string PercorsoCopertinaThumbnail)> SalvaCopertinaAsync(
        Guid copiaId,
        Stream immagine,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            using var originale = LeggiImmagine(immagine);

            // L'orientamento EXIF delle foto da smartphone va applicato
            // esplicitamente prima di ridimensionare, altrimenti la copertina
            // risulterebbe ruotata nel browser.
            originale.AutoOrient();
            originale.Format = MagickFormat.Jpg;
            originale.Quality = 85;

            if (originale.Width > StorageConstants.CoverMaxWidth)
            {
                originale.Resize(new MagickGeometry((uint)StorageConstants.CoverMaxWidth) { IgnoreAspectRatio = false });
            }

            Directory.CreateDirectory(_coversRootPath);

            var nomeFile = $"{copiaId:N}.jpg";
            originale.Write(Path.Combine(_coversRootPath, nomeFile));

            using var thumbnail = originale.Clone();
            thumbnail.Resize(new MagickGeometry((uint)StorageConstants.ThumbnailMaxWidth) { IgnoreAspectRatio = false });

            var nomeFileThumbnail = $"{copiaId:N}{StorageConstants.ThumbnailSuffix}.jpg";
            thumbnail.Write(Path.Combine(_coversRootPath, nomeFileThumbnail));

            return (
                PercorsoCopertina: $"{StorageConstants.CoversRequestPath}/{nomeFile}",
                PercorsoCopertinaThumbnail: $"{StorageConstants.CoversRequestPath}/{nomeFileThumbnail}");
        }, cancellationToken);
    }

    public void EliminaCopertina(string? percorsoCopertina, string? percorsoCopertinaThumbnail)
    {
        EliminaFile(percorsoCopertina);
        EliminaFile(percorsoCopertinaThumbnail);
    }

    private void EliminaFile(string? percorsoPubblico)
    {
        if (string.IsNullOrWhiteSpace(percorsoPubblico))
        {
            return;
        }

        var percorsoCompleto = Path.Combine(_coversRootPath, Path.GetFileName(percorsoPubblico));

        if (File.Exists(percorsoCompleto))
        {
            File.Delete(percorsoCompleto);
        }
    }

    private static MagickImage LeggiImmagine(Stream immagine)
    {
        try
        {
            return new MagickImage(immagine);
        }
        catch (MagickException ex)
        {
            throw new ValidationException("Il file caricato non è un'immagine valida.", ex);
        }
    }
}
