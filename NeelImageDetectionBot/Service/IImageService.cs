using System.IO;
using System.Threading.Tasks;

namespace NeelImageDetectionBot.Service
{
    internal interface IImageService
    {
        /// <summary> 
        /// Gets the caption of an image stream. 
        /// </summary> 
        /// <param name="stream">The stream to an image.</param> 
        /// <returns>Description if caption found, null otherwise.</returns> 
        Task<string> GetCaptionAsync(Stream stream);

        /// <summary> 
        /// Gets the caption of an image URL. 
        /// </summary> 
        /// <param name="url">The URL to an image.</param> 
        /// <returns>Description if caption found, null otherwise.</returns> 
        Task<string> GetCaptionAsync(string url);
    }
}