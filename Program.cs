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
            if (args == null || args.Length != 2)
            {
                printHelp();
                return;
            }

            // Create a new PdfWriter
            PdfWriter pdfWriter = new PdfWriter(900.0f, 1190.0f, 20.0f, 12.0f);

            //Write to a PDF format file
            Console.WriteLine("Converting...");
            try
            {
                int r = pdfWriter.Convert(args[0], args[1]);
                Console.WriteLine(r == 0 ? "Successful completion." : "Failed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed: {0}", ex.ToString()));
            }
        }

        private static void printHelp()
        {
            Console.WriteLine("usage: Txt2Pdf inFilename outFilename");
        }
    }

}
