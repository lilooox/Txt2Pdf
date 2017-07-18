using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Txt2Pdf.Utilities;
using System.Web;
using System.Text.RegularExpressions;

namespace Txt2Pdf
{
    class Program
    {
        static void Main(string[] args)
        {
            ////The text file 
            string filePath = @"C:\Export\110689.txt";
            string outputPath = @"C:\Export\export.pdf";


            // Create a new PdfWriter
            PdfWriter pdfWriter = new PdfWriter(900.0f, 1190.0f, 20.0f, 12.0f);

            //Write to a PDF format file
            pdfWriter.Write(filePath, outputPath);




        }
    }

}
