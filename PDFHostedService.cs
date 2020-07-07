using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PDFSplit
{
    public class PDFHostedService : IHostedService
    {
        private readonly ILogger<PDFHostedService> logger;
        private readonly IHostApplicationLifetime app;
        public PDFHostedService(ILogger<PDFHostedService> logger, IHostApplicationLifetime app) {
            this.logger = logger;
            this.app = app;
        }

        private async Task SplitFolderOfFiles() {
        GetDirectorytPath:
            logger.LogInformation("Provide The folder path (Folder only)");
            var directoryPath = Console.ReadLine();

            if (!Directory.Exists(directoryPath)) {
                logger.LogError($"Folder cannot be found: ({directoryPath}) Please try again");
                goto GetDirectorytPath;
            }

        GetOutputPath:
            logger.LogInformation("Provide The output path (Folder only)");
            var outputPath = Console.ReadLine();

            if (!Directory.Exists(outputPath)) {
                logger.LogError($"Folder cannot be found: ({outputPath}) Please try again");
                goto GetOutputPath;
            }

            var myFiles = System.IO.Directory.GetFiles(directoryPath, "*.pdf");
            
            logger.LogInformation($"I found {myFiles.Count()}");

            foreach (var pdfFilePath in myFiles)
            {
                int interval = 1;  
                int pageNameSuffix = 0;  

                var reader = new PdfReader(pdfFilePath);              
                var file = new FileInfo(pdfFilePath);  
                var pdfFileName = Path.GetFileNameWithoutExtension(pdfFilePath);
                var documentoutputPath = Path.Combine(outputPath, pdfFileName);
                Directory.CreateDirectory(documentoutputPath);

                var obj = new Program();  
                logger.LogInformation($"Pages Identified: {reader.NumberOfPages}");

                for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber += interval)  
                {  
                    pageNameSuffix++;  
                    string newPdfFileName = $"{pdfFileName}-{pageNameSuffix}";
                    SplitAndSaveInterval(pdfFilePath, documentoutputPath, pageNumber, interval, newPdfFileName);  
                } 
            }
            await Task.CompletedTask;
        }

        private async Task PealApart() {
            logger.LogInformation("Stripping to single pages");
            

            
        GetFilePath:
            logger.LogInformation("Provide The Document FULL PATH including file name");
            var pdfFilePath = Console.ReadLine();

            if (!File.Exists(pdfFilePath)) {
                logger.LogError($"File cannot be found: ({pdfFilePath}) Please try again");
                goto GetFilePath;
            }

        GetOutputPath:
            logger.LogInformation("Provide The output path (Folder only)");
            var outputPath = Console.ReadLine();

            if (!Directory.Exists(outputPath)) {
                logger.LogError($"Folder cannot be found: ({outputPath}) Please try again");
                goto GetOutputPath;
            }

            int interval = 1;  
            int pageNameSuffix = 0;  

            var reader = new PdfReader(pdfFilePath);              
            var file = new FileInfo(pdfFilePath);  
            var pdfFileName = Path.GetFileNameWithoutExtension(pdfFilePath);

            var obj = new Program();  
            logger.LogInformation($"Pages Identified: {reader.NumberOfPages}");

            for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber += interval)  
            {  
                pageNameSuffix++;  
                string newPdfFileName = $"{pdfFileName}-{pageNameSuffix}";
                SplitAndSaveInterval(pdfFilePath, outputPath, pageNumber, interval, newPdfFileName);  
            } 

            await Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var actions = new List<(Action, string)>
            {
                (async () => await PealApart(), "Strip document into single pages"),
                (async () => await SplitFolderOfFiles(), "Split apart all documents in a folder")
            };

            ListOptions(actions);
            for (;;)
            {
                var consoleKeyInfo = Console.ReadKey(true);
                if (consoleKeyInfo.Key == ConsoleKey.Q)
                {
                    logger.LogInformation("Quitting..");
                    app.StopApplication();
                    break;
                }

                if (char.IsNumber(consoleKeyInfo.KeyChar))
                {
                    var product = actions[(int)char.GetNumericValue(consoleKeyInfo.KeyChar)];
                    product.Item1();
                    ListOptions(actions);
                }
            }

            await Task.CompletedTask;
        }

        private void ListOptions(List<(Action, string)> actions)
        {
            logger.LogInformation(@"      
                                          _________
                                         / ======= \
                  PDF Extractor         / __________\
                                       | ___________ |
                                       | | -       | |
                                       | |         | |
                                       | |_________| |________________________
                                       \=____________/   AG                   )
                                       /             \                       /
                                      / ::::::::::::: \                  =D-'
                                     (_________________)
                                     
            ");

            logger.LogInformation("Press Q key to exit");
            logger.LogInformation("Press [0..9] key to do some things!");
            logger.LogWarning(string.Join(Environment.NewLine, actions.Select((x, i) => $"[{i}]: {x.Item2} @ ")));
        }

        private void SplitAndSaveInterval(string pdfFilePath, string outputPath, int startPage, int interval, string pdfFileName)  
        {            
            using (PdfReader reader = new PdfReader(pdfFilePath))  
            {  
                Document document = new Document();  
                Console.WriteLine($"Creating Document: " + Path.Combine(outputPath, pdfFileName + ".pdf"));
                PdfCopy copy = new PdfCopy(document, new FileStream(Path.Combine(outputPath, pdfFileName + ".pdf"), FileMode.Create));  
                document.Open();  
  
                for (int pagenumber = startPage; pagenumber < (startPage + interval); pagenumber++)  
                {  
                    if (reader.NumberOfPages >= pagenumber)  
                    {  
                        copy.AddPage(copy.GetImportedPage(reader, pagenumber));  
                    }  
                    else  
                    {  
                        break;  
                    }  
  
                }  
  
                document.Close();  
            }  
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Quitting..");
            app.StopApplication();
            return Task.CompletedTask;
        }
    }
}