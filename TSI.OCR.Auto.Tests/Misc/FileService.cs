using System.IO;
using System.Linq;

namespace TSI.OCR.Auto.Tests.Misc; 

public class FileService {
    private readonly string sourceFolderPath;

    public FileService(string sourceFolderPath) {
        this.sourceFolderPath = sourceFolderPath;
    }

    public void SaveImage(string fileName, byte[] imageByteArray) {
        if (imageByteArray == null) return;

        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        fs.Write(imageByteArray, 0, imageByteArray.Length);
    }

    public string GetSourceFilesPath() {
        //return Directory.GetFiles(sourceFolderPath, "*.pdf").FirstOrDefault();
        return Directory
            .GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories)
            .FirstOrDefault(s => s.EndsWith(".pdf") || s.EndsWith(".html"));
    }

    public string[] GetSourceFilesPaths() {
        //return Directory.GetFiles(sourceFolderPath, "*.pdf").FirstOrDefault();
        return Directory
            .GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".pdf") || s.EndsWith(".html")).ToArray();
    }

    public string GetFeatureFlagsFilePath() {
        return Directory.GetFiles(sourceFolderPath, "*.txt").FirstOrDefault();
    }

    public string GetApkgFilePath() {
        return Directory.GetFiles(sourceFolderPath, "*.apkg").FirstOrDefault();
    }
}