using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace TSI.OCR.Auto.Tests.Misc {
    public static class PackageService {
        public static int GetPackageRecordsCountByBlockType(string fileName, string blockType) {
            var connection = new SQLiteConnection($"Data Source={fileName}");
            //connection.Open();
            var str = $"SELECT COUNT(*) FROM Blocks WHERE BlockType = '{blockType}'";
            var getCountCommand = connection.Query<int>(str);
            return getCountCommand.First();
        }
        
        public static async Task<List<BlocksTable>> GetPackageRecordsByBlockType(string fileName, string blockType) {
            var connection = new SQLiteConnection($"Data Source={fileName}");
            //connection.Open();
            var result = await connection
                .QueryAsync<BlocksTable>(
                    $"SELECT Page, X, CAST(Y as REAL) AS Y, W, H, Text, NormalizedText, PageWidth, PageHeight, BlockType, Image AS ImageArray, NormLength, IMG_SCALE FROM Blocks WHERE BlockType = '{blockType}'");
            return result.ToList();
        }

        /// <summary>
        /// Return DISTINCT block type kist.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetBlockTypeList(string fileName) {
            var connection = new SQLiteConnection($"Data Source={fileName}");
            //connection.Open();
            //var result = await connection.QueryAsync<string>($"SELECT DISTINCT BlockType FROM Blocks WHERE BlockType IN ('LINE_BLOCK', 'PAGE_BLOCK', 'TEXTBOX_BLOCK')");
            var result = await connection.QueryAsync<string>("SELECT DISTINCT BlockType FROM Blocks WHERE BlockType NOT IN ('WORD_BLOCK')");
            return result.ToList();
        }
    }
}