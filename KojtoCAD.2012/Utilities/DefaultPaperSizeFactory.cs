using System;
using System.Drawing.Printing;
using Castle.Core.Logging;
using KojtoCAD.Utilities.Interfaces;

namespace KojtoCAD.Utilities
{
    public class DefaultPaperSizeFactory : IPaperSizeFactory
    {
        private ILogger _logger = NullLogger.Instance;

        public PaperSize CreateOrientedPaperSize(string paperSizeName, string paperOrientation)
        {
            PaperSize paperSize = new PaperSize();
            paperSize.PaperName = paperSizeName;
            
            switch (paperOrientation)
            {
                case "Landscape":
                    switch (paperSizeName)
                    {
                        case "A4":
                            paperSize.Width = 297;
                            paperSize.Height = 210;
                            break;
                        case "A3":
                            paperSize.Width = 420;
                            paperSize.Height = 297;
                            break;
                        case "A2":
                            paperSize.Width = 594;
                            paperSize.Height = 420;
                            break;
                        case "A1":
                            paperSize.Width = 841;
                            paperSize.Height = 594;
                            break;
                        case "A0":
                            paperSize.Width = 1189;
                            paperSize.Height = 841;
                            break;
                        default:
                            throw new ArgumentException("Invlaid paper size.");
                    }

                    break;

                case "Portrait":
                    switch (paperSizeName)
                    {
                        case "A4":
                            paperSize.Width = 210;
                            paperSize.Height = 297;
                            break;
                        case "A3":
                            paperSize.Width = 297;
                            paperSize.Height = 420;
                            break;
                        case "A2":
                            paperSize.Width = 420;
                            paperSize.Height = 594;
                            break;
                        case "A1":
                            paperSize.Width = 594;
                            paperSize.Height = 841;
                            break;
                        case "A0":
                            paperSize.Width = 841;
                            paperSize.Height = 1189;
                            break;
                        default:
                            throw new ArgumentException("Invlaid paper size.");

                    }
                    break;
                default:
                    throw new ArgumentException("Invlaid paper orientation.");               
            }
            return paperSize;
        }
    }
}
