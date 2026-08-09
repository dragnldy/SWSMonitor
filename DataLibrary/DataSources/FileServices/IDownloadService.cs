using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary.DataSources.FileServices
{
    public interface IDownloadService
    {
        abstract void DownloadFile(string filename, string contentType, string base64Content);
    }
}
