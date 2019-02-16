﻿using System;
using System.Text;
using System.Collections;
using System.IO;

namespace Txt2Pdf.Utilities
{
    public class PdfPage
    {
        public PdfPage() { }

        private int page_id = 1;
        private int nextId = 1;
        private int new_num_pid = 1;


        public int GetNextId()
        {
            nextId = this.new_num_pid + 3;
            return nextId;
        }

        public int pageId
        {
            get
            {
                return this.page_id;
            }
            set
            {
                this.page_id = value;
            }

        }
    }

    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class PdfWriter
    {
        //Initiation
        public float pageWidth = 842.0f;
        public float pageDepth = 1190.0f;
        public float margin = 30.0f;
        public float leadSize = 10.0f;
        public float fontSize = 10.0f;
        private int numPages = 0;
        public int numXrefs = 0;
        private int objectID = 1;


        //page
        public int pageTreeID;
        public long pInsertPage;
        public long start_xref = 0;
        public long pXrefs = 0;

        public float yPos; //y-position

        public int size = 0;
        public Byte[] bytes;

        //stream
        private int mStreamID;
        private int mStreamLenID;
        private long mStreamStart;

        //files
        public FileStream outFileStream;


        //private Encoding encoding = Encoding.GetEncoding("gb2312");
        private Encoding outEncoding = Encoding.GetEncoding("gb2312");
        public Encoding GetFileEncodeType(string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
            Byte[] buffer = br.ReadBytes(2);
            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                {
                    return System.Text.Encoding.UTF8;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    return System.Text.Encoding.BigEndianUnicode;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    return System.Text.Encoding.Unicode;
                }
                else
                {
                    return System.Text.Encoding.Default;
                }
            }
            else
            {
                int len = (int)fs.Length;
                byte[] bs = new byte[len];
                fs.Position = 0;
                fs.Read(bs, 0, len);
                if (IsUTF8(bs))
                {
                    return Encoding.UTF8;
                }
                else
                {
                    return System.Text.Encoding.Default;
                }
            }
        }

        public static bool IsUTF8(byte[] values)
        {
            string stringValue = Encoding.UTF8.GetString(values);
            byte[] newValues = Encoding.UTF8.GetBytes(stringValue);
            if (values.Length != newValues.Length)
            {
                return false;
            }
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != newValues[i])
                {
                    return false;
                }
            }
            return true;
        }

        public PdfWriter(float width, float depth, float margin, float lead)
        {

            this.pageWidth = width;
            this.pageDepth = depth;
            this.margin = margin;
            this.leadSize = lead;

            this.numPages = 0;
            this.numXrefs = 0;
            this.objectID = 1;
            this.fontSize = 12.0f;

        }

        /// <summary>
        /// Write
        /// </summary>
        public int Convert(string inFilename, string outFilename)
        {
            outFileStream = new FileStream(outFilename, FileMode.Create, FileAccess.Write);

            int i, catalog_id, font_id;

            string str1 = "%PDF-1.4\r\n";
            FileStreamWrite(outFileStream, str1);

            //Increment pageTreeID
            pageTreeID = objectID++;

            Encoding encoding = GetFileEncodeType(inFilename);//TextFileEncodingDetector.DetectTextFileEncoding(inFilename, Encoding.Default);//
            StreamReader sr = new StreamReader(inFilename, encoding);

            //Invoke the DoText function
            DoText(sr);

            /****************/

            var copyrightId = objectID++;
            StartObject(copyrightId);

            var str = "\r\n<</Registry(Adobe)/Ordering(GB1)/Supplement 2>>\r\nendobj\r\n";
            FileStreamWrite(outFileStream, str);

            var FontDescriptor = objectID++;
            StartObject(FontDescriptor);

            str = "\r\n<</Type/FontDescriptor/FontName/FangSong/Flags 33/FontBBox[ -8 -164 1004 859]/ItalicAngle 0/Ascent 859/Descent -164/CapHeight 500/StemV 80>>\r\nendobj\r\n";
            FileStreamWrite(outFileStream, str);


            var CIDSystemInfo = objectID++;
            StartObject(CIDSystemInfo);

            str = string.Format("\r\n<</W[ 7716 7716 500 814 907 500]/Type/Font/Subtype/CIDFontType2/BaseFont/FangSong/CIDSystemInfo {0} 0 R /FontDescriptor {1} 0 R >>\r\nendobj\r\n", copyrightId, FontDescriptor);
            FileStreamWrite(outFileStream, str);

            //var Font = objectID++;
            //StartObject(Font);

            //str = string.Format("\r\n<</Type/Font/Subtype/Type0/BaseFont/NSimSun/Encoding/GBK-EUC-H/DescendantFonts[ {0} 0 R ]>>\r\nendobj\r\n", CIDSystemInfo);
            //FileStreamWrite(outFileStream, str);
            /******************/

            //Increment font_id
            font_id = objectID++;
            StartObject(font_id);

            //string strBaseFont = "\r\n<</Type/Font/Subtype/Type1/BaseFont/Courier/Encoding/WinAnsiEncoding>>\r\nendobj\r\n";
            //FileStreamWrite(outFileStream, strBaseFont);
            str = string.Format("\r\n<</Type/Font/Subtype/Type0/BaseFont/FangSong/Encoding/GBK-EUC-H/DescendantFonts[ {0} 0 R ]>>\r\nendobj\r\n", CIDSystemInfo);
            FileStreamWrite(outFileStream, str);

            //Invoke the StartObject function
            StartObject(pageTreeID);

            //Write out page number
            string strPageCount = "<</Type /Pages /Count " + this.numPages + "\r\n";
            FileStreamWrite(outFileStream, strPageCount);

            string strKids = "/Kids[\r\n";
            FileStreamWrite(outFileStream, strKids);

            int nextid = 0;
            for (int x = 1; x <= this.numPages; x++)
            {
                nextid = x * 3 + 1;
                string strPage_id = string.Format("{0} 0 R ", nextid) + "\r\n";

                FileStreamWrite(outFileStream, strPage_id);
            }


            //String strKidsClose
            string strKidsClose = "]\r\n";
            FileStreamWrite(outFileStream, strKidsClose);

            //Fond id
            if (font_id < 0)
            {
                font_id = 1;
            }


            string strResources = string.Format("/Resources<</ProcSet[/PDF/Text]/Font<</F0 {0} 0 R>> >>", font_id, "\r\n");
            FileStreamWrite(outFileStream, strResources);

            //String MediaBox
            string strMediaBox = string.Format("\r\n/MediaBox [ 0 0 {0} {1} ]\r\n", this.pageWidth, this.pageDepth);
            FileStreamWrite(outFileStream, strMediaBox);

            //String endobj
            string strEndobj = ">>\r\n endobj\r\n";
            FileStreamWrite(outFileStream, strEndobj);


            //Catalog ID
            catalog_id = objectID++;

            //Invoke the StartObject function
            StartObject(catalog_id);

            //String Catalog
            string strCatalog = string.Format("<</Type/Catalog/Pages {0} 0 R>>\r\nendobj", pageTreeID, "\r\n");
            FileStreamWrite(outFileStream, strCatalog);

            //String xref
            string strXref = "\nxref\n";
            FileStreamWrite(outFileStream, strXref);

            start_xref = outFileStream.Length;


            //String ObjectID
            string strObjectID = string.Format("0 {0} ", pageTreeID, "\n");
            FileStreamWrite(outFileStream, strObjectID);

            //String ObjectID
            string str65535 = "0000000000 65535 f \n";
            FileStreamWrite(outFileStream, str65535);

            for (i = 1; i < objectID; i++)
            {
                string str010ld = string.Format("{0} 00000 n \r\n", (pXrefs + i));
                FileStreamWrite(outFileStream, str010ld);
            }

            string strTrailer = string.Format("trailer\r\n<<\r\n/Size {0}\r\n/Root {1} 0 R\r\n>>", objectID, catalog_id);
            FileStreamWrite(outFileStream, strTrailer);

            //xref
            start_xref = 100;
            string strEOF = string.Format("startxref\r\n{0}\r\n%%EOF\r\n", start_xref);
            FileStreamWrite(outFileStream, strEOF);

            outFileStream.Close();

            return 0;

        }

        /// <summary>
        /// StartPage
        /// </summary>
        private void StartPage()
        {
            mStreamID = objectID++;
            mStreamLenID = objectID++;

            StartObject(mStreamID);

            //String Length
            string strLength = string.Format("<< /Length {0} 0 R >>\r\n", mStreamLenID);
            FileStreamWrite(outFileStream, strLength);

            //String Stream
            string strStream = "stream\r\n";
            FileStreamWrite(outFileStream, strStream);

            mStreamStart = outFileStream.Length;

            //String BT
            string strBT = string.Format("BT\r\n/F0 {0} Tf\r\n", this.fontSize.ToString("G"));
            FileStreamWrite(outFileStream, strBT);

            //Calculate yPos
            yPos = this.pageDepth - this.margin;

            //String Td
            string strTd = string.Format("{0} {1} Td\r\n", this.margin.ToString("G"), yPos.ToString("G"));
            FileStreamWrite(outFileStream, strTd);

            //String TL
            string strTL = string.Format("{0} TL\r\n", leadSize.ToString("G"));
            FileStreamWrite(outFileStream, strTL);
        }

        /// <summary>
        /// EndPage
        /// </summary>
        private void EndPage()
        {
            long stream_len = 0;
            int page_id = objectID++;

            //Invoke the StorePage function
            StorePage(page_id);

            //String ET
            string strTL = "\r\nET\r\n";
            FileStreamWrite(outFileStream, strTL);

            //Calculate stream length
            stream_len = outFileStream.Length - mStreamStart;

            //String Endstream
            string strEndstream = "endstream\nendobj\r\n";
            FileStreamWrite(outFileStream, strEndstream);

            //Invoke the "StartObject" function
            StartObject(mStreamLenID);

            string strEndobj = string.Format("{0}\r\nendobj\r\n", stream_len);
            FileStreamWrite(outFileStream, strEndobj);

            //Invoke the "StartObject"
            StartObject(page_id);

            //String Parent
            string strParent = string.Format("<</Type/Page/Parent {0} 0 R/Contents {1} 0 R>>\r\nendobj\r\n", pageTreeID, mStreamID);
            FileStreamWrite(outFileStream, strParent);
        }

        /// <summary>
        /// DoText
        /// </summary>
        /// <param name="arData">ArrayList</param>
        private void DoText(StreamReader sr)
        {
            string strLine = string.Empty;

            //Start Page
            StartPage();

            try
            {
                while (sr.Peek() >= 0)
                {
                    //Get one string at a time from the input text file
                    strLine = sr.ReadLine() + "\r\n";

                    //If yPos <= this.margin?
                    if (yPos <= this.margin)
                    {
                        //Invoke EndPage and StartPage functions
                        EndPage();
                        StartPage();
                    }

                    if (strLine == "" || strLine == null)
                    {
                        FileStreamWrite(outFileStream, @"T*\r\n");
                    }
                    else
                    {
                        //Is there a page break "1"?
                        int cmpPageVal = String.Compare(strLine.Substring(0, 1), "1");

                        //Is there a Formfeed?
                        int cmpfVal = String.Compare(strLine.Substring(0, 1), "\f");

                        bool bl = false;

                        //Formfeed
                        if (cmpfVal == 0)
                        {
                            //Invoke EndPage and StartPage functions
                            EndPage();
                            StartPage();
                        }
                        else
                        {
                            //If there is a page break "1".
                            if (cmpPageVal == 0)
                            {
                                //Invoke EndPage and StartPage functions
                                EndPage();
                                StartPage();

                                //Remove the page break "1"
                                strLine = strLine.Remove(0, 1);
                            }

                            FileStreamWrite(outFileStream, @"(");

                            //Convert "strLine" to a char array
                            char[] textchars = strLine.ToCharArray();

                            for (int index = 0; index < textchars.Length; index++)
                            {
                                char c = textchars[index];

                                //If there is page break?
                                if (c == '1' && strLine.Length == 2)
                                {
                                    EndPage();
                                    StartPage();
                                }
                                //new line
                                else if (c == '\n')
                                {
                                    if (!bl)
                                        FileStreamWrite(outFileStream, @")'");
                                    else
                                        FileStreamWrite(outFileStream, @"T*\n");

                                    bl = true;
                                }
                                else
                                {
                                    FileStreamWrite(outFileStream, c.ToString());
                                    bl = false;
                                }
                            }

                            if (!bl)
                                FileStreamWrite(outFileStream, @")\r\n");
                        }

                    }

                    //Set yPos
                    yPos -= leadSize;
                }//for loop

                //Close file
                sr.Close();
                sr = null;

                //End Page
                EndPage();

            }
            catch (Exception ex)
            {
                string error = ("The process failed: " + ex.Message);
            }
        }


        /// <summary>
        /// StartObject
        /// </summary>
        /// <param name="id">int</param>
        private void StartObject(int id)
        {
            if (id >= numXrefs)
            {
                long new_xrefs = 0;
                int delta, new_num_xrefs;
                delta = numXrefs / 5;

                if (delta < 1000)
                    delta += 1000;

                new_num_xrefs = numXrefs + delta;

                new_xrefs = (long)new_num_xrefs * (new_xrefs + 8);

                if (new_xrefs == 0)
                {
                    string stderr = string.Format("Unable to allocate array for object {0}.", id);
                    FileStreamWrite(outFileStream, stderr);
                }
                else
                {
                    pXrefs = new_xrefs;
                    numXrefs = new_num_xrefs;
                }
            }

            pXrefs = outFileStream.Length;

            //String objId
            string strobjId = string.Format("{0} 0 obj\r\n", id);
            FileStreamWrite(outFileStream, strobjId);

        }

        /// <summary>
        /// StorePage
        /// </summary>
        /// <param name="id">int</param>
        public void StorePage(int id)
        {
            PdfPage pp = new PdfPage();

            long n = pp.GetNextId();

            if (n == 0)
            {
                string stderr = string.Format("Unable to allocate array for page {0}.", this.numPages + 1);
                FileStreamWrite(outFileStream, stderr);
            }

            pp.pageId = id;

            pInsertPage = this.numPages * 3 + 1;
            this.numPages++;

        }


        /// <summary>
        /// FileStreamWrite
        /// </summary>
        /// <param name="outFileStream">FileStream</param>
        /// <param name="str1">string</param>
        private void FileStreamWrite(FileStream outFileStream, string str1)
        {
            Byte[] buffer = null;
            buffer = outEncoding.GetBytes(str1);
            outFileStream.Write(buffer, 0, buffer.Length);
        }
    }
}
