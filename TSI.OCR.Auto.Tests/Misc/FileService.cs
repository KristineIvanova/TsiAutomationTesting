using System.IO;
using System.Linq;

namespace TSI.OCR.Auto.Tests.Misc; 

public class FileService {
    private readonly string _sourceFolderPath;

    public FileService(string sourceFolderPath) {
        _sourceFolderPath = sourceFolderPath;
    }

    public string GetSourceFilesPath() {
        //return Directory.GetFiles(sourceFolderPath, "*.pdf").FirstOrDefault();
        return Directory
            .GetFiles(_sourceFolderPath, "*.*", SearchOption.AllDirectories)
            .FirstOrDefault(s => s.EndsWith(".pdf") || s.EndsWith(".html"));
    }

    public string[] GetSourceFilesPaths() {
        //return Directory.GetFiles(sourceFolderPath, "*.pdf").FirstOrDefault();
        return Directory
            .GetFiles(_sourceFolderPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".pdf") || s.EndsWith(".html")).ToArray();
    }

    public string GetFeatureFlagsFilePath() {
        return Directory.GetFiles(_sourceFolderPath, "*.txt").FirstOrDefault();
    }

    public string GetFilePath() {
        return Directory.GetFiles(_sourceFolderPath, "*.apkg").FirstOrDefault();
    }
}