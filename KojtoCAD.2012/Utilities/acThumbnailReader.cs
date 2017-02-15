//Copyright 2009 K.E. Blackie
//This work is based in part from a VBA macro
//initially developed by members of www.vbdesign.net
//it may be used freely and may be modified to suit
//your needs as long as credit is given where it is due

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KojtoCAD.Utilities
{
    class acThumbnailReader
    {

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMGREC
        {
            public byte bytType;
            public int lngStart;
            public int lngLen;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        public static Bitmap GetThumbnail(string strFile)
        {
            return GetThumbnail(strFile, true, false, "");
        }

        public static Bitmap GetThumbnail(string strFile, bool boolRetainBackColor)
        {
            return GetThumbnail(strFile, boolRetainBackColor, false, "");
        }

        public static Bitmap GetThumbnail(string strFile, bool boolRetainBackColor, bool boolSaveToFile)
        {
            return GetThumbnail(strFile, boolRetainBackColor, boolSaveToFile, "");
        }

        public static Bitmap GetThumbnail(string strFile, bool boolRetainBackColor, bool boolSaveToFile, string strSaveName)
        {
            Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
            byte bytCnt;
            byte[] bytBMPBuff;
            int lngImgLoc;
            FileStream fs = null;
            BinaryReader br = null;
            int lngCurLoc;
            int lngY;
            int lngX;
            int lngColor;
            int lngCnt;
            short intCnt;
            IMGREC udtRec;
            RGBQUAD[] udtColors;
            RGBQUAD udtColor;
            BITMAPINFOHEADER udtHeader;
            short intRed;
            short intGreen;
            short intBlue;
            try
            {
                if (File.Exists(strFile))
                {
                    fs = File.OpenRead(strFile);
                    using (br = new BinaryReader(fs))
                    {
                        fs.Seek(13, SeekOrigin.Begin);
                        lngImgLoc = br.ReadInt32();
                        fs.Seek(lngImgLoc + 17, SeekOrigin.Begin);
                        lngCurLoc = lngImgLoc + 17;
                        fs.Seek(lngCurLoc + 3, SeekOrigin.Begin);
                        bytCnt = br.ReadByte();
                        if (bytCnt > 1)
                        {
                            for (intCnt = 0; intCnt < bytCnt; intCnt++)
                            {
                                udtRec.bytType = br.ReadByte();
                                udtRec.lngStart = br.ReadInt32();
                                udtRec.lngLen = br.ReadInt32();
                                if (udtRec.bytType == 2)
                                {
                                    fs.Seek(udtRec.lngStart, SeekOrigin.Begin);
                                    udtHeader.biSize = br.ReadInt32();
                                    udtHeader.biWidth = br.ReadInt32();
                                    udtHeader.biHeight = br.ReadInt32();
                                    udtHeader.biPlanes = br.ReadInt16();
                                    udtHeader.biBitCount = br.ReadInt16();
                                    udtHeader.biCompression = br.ReadInt32();
                                    udtHeader.biSizeImage = br.ReadInt32();
                                    udtHeader.biXPelsPerMeter = br.ReadInt32();
                                    udtHeader.biYPelsPerMeter = br.ReadInt32();
                                    udtHeader.biClrUsed = br.ReadInt32();
                                    udtHeader.biClrImportant = br.ReadInt32();
                                    bytBMPBuff = new byte[udtRec.lngLen + 1];
                                    if (udtHeader.biBitCount == 8)
                                    {
                                        udtColors = new RGBQUAD[256];
                                        for (int count = 0; count < 256; count++)
                                        {
                                            udtColors[count].rgbBlue = br.ReadByte();
                                            udtColors[count].rgbGreen = br.ReadByte();
                                            udtColors[count].rgbRed = br.ReadByte();
                                            udtColors[count].rgbReserved = br.ReadByte();
                                        }
                                        fs.Seek(udtRec.lngStart - 1, SeekOrigin.Begin);
                                        for (int count = 0; count <= udtRec.lngLen; count++)
                                        {
                                            bytBMPBuff[count] = br.ReadByte();
                                        }
                                        bmp = new Bitmap(udtHeader.biWidth, udtHeader.biHeight);
                                        lngCnt = 0;
                                        for (lngY = 1; lngY <= udtHeader.biHeight; lngY++)
                                        {
                                            for (lngX = udtHeader.biWidth; lngX >= 1; lngX--)
                                            {
                                                lngColor = bytBMPBuff[bytBMPBuff.GetUpperBound(0) - lngCnt];
                                                udtColor = udtColors[lngColor];
                                                intRed = Convert.ToInt16(udtColor.rgbRed);
                                                intGreen = Convert.ToInt16(udtColor.rgbGreen);
                                                intBlue = Convert.ToInt16(udtColor.rgbBlue);
                                                lngColor = ColorTranslator.ToOle(Color.FromArgb(intRed, intGreen, intBlue));
                                                if (boolRetainBackColor == false)
                                                {
                                                    if (lngColor == ColorTranslator.ToOle(Color.Black))
                                                    {
                                                        lngColor = ColorTranslator.ToOle(Color.White);
                                                    }
                                                    else
                                                    {
                                                        if (lngColor == ColorTranslator.ToOle(Color.White))
                                                        {
                                                            lngColor = ColorTranslator.ToOle(Color.Black);
                                                        }
                                                    }
                                                }
                                                bmp.SetPixel(lngX - 1, lngY - 1, ColorTranslator.FromOle(lngColor));
                                                lngCnt++;
                                            }
                                        }
                                    }
                                    goto Exit_Here;
                                }
                                else
                                {
                                    if (udtRec.bytType == 3)
                                    {
                                        goto Exit_Here;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        Exit_Here:
            //if (br != null)
            //{
            //    br.Close();
            //    fs.Close();
            //    fs.Dispose();
            //}
            if (boolSaveToFile == true)
            {
                if (strSaveName == "")
                {
                    string fName;
                    fName = String.Concat(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()), ".bmp");
                    bmp.Save(fName);
                }
                else
                {
                    bmp.Save(strSaveName);
                }
            }
            return bmp;
        }
    }
}