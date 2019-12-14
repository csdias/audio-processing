using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciController : ControllerBase
    {
        private readonly ILogger<FibonacciController> _logger;

        public FibonacciController(ILogger<FibonacciController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public int Get(int x)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var metadataFilePath = string.Empty;

            foreach(var subfolder in Directory.GetDirectories($@"{currentDirectory}\wav-files")){
                metadataFilePath = Path.Combine(subfolder, @"metadata.json");
                Console.WriteLine($"Reading {metadataFilePath}");

                //var options = new JsonSerializerOptions(){
                    //PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                //}
                
                var metadataCollection = GetMetadataCollection(metadataFilePath);

                foreach(var metadata in metadataCollection){
                    var audioFilePath = Path.Combine(subfolder, metadata.File.FileName);
                    var md5Checksum = GetCheckSum(audioFilePath);

                    if (md5Checksum.Replace("-", "").ToLower() != metadata.File.Md5Checksum)
                    {
                        throw new Exception("Checksum not valid! File corrupted?");
                    }

                    var uniqueId = Guid.NewGuid();

                    metadata.File.FileName = $"{uniqueId}.WAV";
                    var newPath = Path.Combine(currentDirectory, "ready-for-transcription", $"{uniqueId}.WAV"); 

                    CreateCompressedFile(audioFilePath, newPath);

                    SaveSingleMetadata(metadata, newPath + ".json");

                }

            }

             if (x < 0) throw new ArgumentNullException(
                 "Must be at leat 0", nameof(x));

            // return Fib(x).current;

             (int current, int previous) = Fib(x);
             return previous;

            (int current, int previous) Fib(int i) {
                    if (i == 0) return (0, 1);      

                    var (current, previous) = Fib(i - 1);

                    return (current + previous, current);
             }


        }

        static void CreateCompressedFile(string inputFilePath, string outputFilePath){
            outputFilePath += ".gz";
            Console.WriteLine($"Compressing file {outputFilePath}");
            using var inputFileStream = System.IO.File.Open(inputFilePath, FileMode.Open);
            var outputFileStream = System.IO.File.Create(outputFilePath);
            var gzipStream = new GZipStream(outputFileStream, CompressionLevel.Optimal);
            inputFileStream.CopyTo(outputFileStream);
        }

        static string GetCheckSum(string filePath){
            using var fileStream = System.IO.File.Open(filePath, FileMode.Open);
            var md5 = System.Security.Cryptography.MD5.Create();
            var md5Hash = md5.ComputeHash(fileStream);
            return BitConverter.ToString(md5Hash);
        }

        static List<Metadata> GetMetadataCollection(string metadataFilePath){
            var text = System.IO.File.ReadAllText(metadataFilePath);
            var metadataCollection = JsonSerializer.Deserialize<List<Metadata>>(text);

            return metadataCollection;
        }

        static void SaveSingleMetadata(Metadata metadata, string metadataFilePath){
            //todo: esta gravando em branco

            var uniqueId = Guid.NewGuid();

            Console.WriteLine($"Creating a metadata file |{metadataFilePath}{uniqueId}");

            var metadataJson = JsonSerializer.Serialize<Metadata>(metadata);

            var stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(metadataJson));

            var metadataFileStream = System.IO.File.Open(metadataFilePath, FileMode.Create);            
            metadataFileStream.CopyTo(stream);

        }        
       
    }
}
