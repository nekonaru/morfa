using Avalonia.Headless.XUnit;
using Morfa.Desktop.Services;

namespace Morfa.Desktop.Tests;

public class SampleFilesTests
{
    [AvaloniaFact]
    public void The_sample_copies_out_as_an_image()
    {
        TestServices.EnsureInitialized("en");
        var path = SampleFiles.Copy();
        Assert.NotNull(path);
        Assert.Equal(SampleFiles.FileName, Path.GetFileName(path));
        Assert.True(File.Exists(path));
        Assert.True(new FileInfo(path).Length > 0);
        Assert.Equal(".png", Path.GetExtension(path));
        File.Delete(path!);
    }
}
