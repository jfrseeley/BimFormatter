using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;

namespace BimFormatter.Text
{
    public class TextFormatterRunningDocumentTableEvents : IVsRunningDocTableEvents3
    {
        private readonly DTE _dte;
        private readonly RunningDocumentTable _runningDocumentTable;

        private readonly IReadOnlyDictionary<string, ITextFormatter> _textFormatters;

        public TextFormatterRunningDocumentTableEvents(DTE dte, RunningDocumentTable runningDocumentTable,
            IReadOnlyDictionary<string, ITextFormatter> textFormatters)
        {
            _dte = dte;
            _runningDocumentTable = runningDocumentTable;

            _textFormatters = textFormatters;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //try
            //{
                var documentInfo = _runningDocumentTable.GetDocumentInfo(docCookie);
                var extension = Path.GetExtension(documentInfo.Moniker);

                if (_textFormatters.TryGetValue(extension, out ITextFormatter textFormatter))
                {
                    foreach (Document document in _dte.Documents)
                    {
                        if (document.FullName != documentInfo.Moniker)
                        {
                            continue;
                        }

                        var textDocument = document.Object("TextDocument") as TextDocument;
                        var startPoint = textDocument.StartPoint.CreateEditPoint();

                        var input = startPoint.GetText(textDocument.EndPoint);
                        var output = textFormatter.Format(input);

                        startPoint.ReplaceText(textDocument.EndPoint, output, -1);
                        break;
                    }
                }
            //}
            //catch { /* Suppress exception */ }

            return VSConstants.S_OK;
        }
    }
}
