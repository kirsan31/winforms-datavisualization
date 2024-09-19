// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	ImageLoader utility class loads specified image and 
//              caches it in the memory for the future use.
//

#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security;

using Size = System.Drawing.Size;

namespace System.Windows.Forms.DataVisualization.Charting.Utilities;

/// <summary>
/// ImageLoader utility class loads and returns specified image 
/// form the File, URI, Web Request or Chart Resources. 
/// Loaded images are stored in the internal dictionary which 
/// allows to improve performance if image need to be used 
/// several times.
/// </summary>
internal sealed class ImageLoader : IDisposable, IServiceProvider
{
    #region Fields

    // Image storage
    private Dictionary<string, Image>? _imageData;

    // Reference to the service container
    private readonly IServiceContainer? _serviceContainer;

    #endregion

    #region Constructors and Initialization

    /// <summary>
    /// Default constructor is not accessible.
    /// </summary>
    private ImageLoader()
    {
    }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    /// <param name="container">Service container.</param>
    public ImageLoader(IServiceContainer container)
    {
        _serviceContainer = container ?? throw new ArgumentNullException(SR.ExceptionImageLoaderInvalidServiceContainer);
    }

    /// <summary>
    /// Returns Image Loader service object
    /// </summary>
    /// <param name="serviceType">Requested service type.</param>
    /// <returns>Image Loader service object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(ImageLoader))
        {
            return this;
        }

        throw new ArgumentException(SR.ExceptionImageLoaderUnsupportedType(serviceType.ToString()));
    }

    /// <summary>
    /// Dispose images in the dictionary.
    /// </summary>
    public void Dispose()
    {
        if (_imageData is null)
            return;

        foreach (var entry in _imageData.Values)
            entry?.Dispose();

        _imageData = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Loads image from URL. Checks if image already loaded (cached).
    /// </summary>
    /// <param name="imageURL">Image name (FileName, URL, Resource).</param>
    /// <returns>Image object.</returns>
    public Image LoadImage(string imageURL)
    {
        return LoadImage(imageURL, true);
    }

    /// <summary>
    /// Loads image from URL. Checks if image already loaded (cached).
    /// </summary>
    /// <param name="imageURL">Image name (FileName, URL, Resource).</param>
    /// <param name="saveImage">True if loaded image should be saved in cache.</param>
    /// <returns>Image object</returns>
    [Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>")]
    public Image LoadImage(string imageURL, bool saveImage)
    {
        // Check if image is defined in the chart image collection
        if (_serviceContainer?.GetService(typeof(Chart)) is Chart chart)
        {
            var img = chart.Images.FindByName(imageURL);
            if (img?.Image is not null)
                return img.Image;
        }

        // Create new dictionary
        _imageData ??= new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        // First check if image with this name already loaded
        _imageData.TryGetValue(imageURL, out Image? image);
        if (image is not null)
            return image;

        // Try to load image from resource
        try
        {
            Assembly? entryAssembly;
            // Check if resource class type was specified
            int columnIndex = imageURL.IndexOf("::", StringComparison.Ordinal);
            if (columnIndex > 0)
            {
                string resourceRootName = imageURL[..columnIndex];
                string resourceName = imageURL[(columnIndex + 2)..];
                ResourceManager resourceManager = new ResourceManager(resourceRootName, Assembly.GetExecutingAssembly());
                image = resourceManager.GetObject(resourceName) as Image;
            }
            else if ((entryAssembly = Assembly.GetEntryAssembly()) is not null)
            {
                // Check if resource class type was specified
                columnIndex = imageURL.IndexOf(':');
                if (columnIndex > 0)
                {
                    string resourceRootName = imageURL[..columnIndex];
                    string resourceName = imageURL[(columnIndex + 1)..];
                    ResourceManager resourceManager = new ResourceManager(resourceRootName, entryAssembly);
                    image = resourceManager.GetObject(resourceName) as Image;
                }
                else
                {
                    // Try to load resource from every type defined in entry assembly
                    foreach (Type type in entryAssembly.GetTypes())
                    {
                        ResourceManager resourceManager = new ResourceManager(type);
                        try
                        {
                            image = resourceManager.GetObject(imageURL) as Image;
                        }
                        catch (ArgumentNullException)
                        {
                        }
                        catch (MissingManifestResourceException)
                        {
                        }

                        // Check if image was loaded
                        if (image is not null)
                        {
                            break;
                        }
                    }
                }
            }
        }
        catch (MissingManifestResourceException)
        {
        }

        // Try to load image using the Web Request
        if (image is null)
        {
            Uri? imageUri = null;
            try
            {
                // Try to create URI directly from image URL (will work in case of absolute URL)
                imageUri = new Uri(imageURL);
            }
            catch (UriFormatException)
            { }


            // Load image from file or web resource
            if (imageUri is not null)
            {
                try
                {
                    WebRequest request = WebRequest.Create(imageUri);
                    image = Image.FromStream(request.GetResponse().GetResponseStream());
                }
                catch (ArgumentException)
                {
                }
                catch (NotSupportedException)
                {
                }
                catch (SecurityException)
                {
                }
            }
        }

        // absolute uri(without Server.MapPath)in web is not allowed. Loading from relative uri Server[Page].MapPath is done above.
        // Try to load as file
        image ??= LoadFromFile(imageURL);

        // Error loading image
        if (image is null)
        {
            throw new ArgumentException(SR.ExceptionImageLoaderIncorrectImageLocation(imageURL));
        }

        // Save new image in cache
        if (saveImage)
        {
            _imageData[imageURL] = image;
        }

        return image;
    }

    /// <summary>
    /// Helper function which loads image from file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns>Loaded image or null.</returns>
    private Image? LoadFromFile(string fileName)
    {
        // Try to load image from file
        try
        {
            return Image.FromFile(fileName);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the image size taking the image DPI into consideration.
    /// </summary>
    /// <param name="name">Image name (FileName, URL, Resource).</param>
    /// <param name="graphics">Graphics used to calculate the image size.</param>
    /// <param name="size">Calculated size.</param>
    /// <returns>false if it fails to calculate the size, otherwise true.</returns>
    internal bool GetAdjustedImageSize(string name, Graphics graphics, ref SizeF size)
    {
        Image image = LoadImage(name);

        if (image is null)
            return false;

        GetAdjustedImageSize(image, graphics, ref size);
        return true;
    }

    /// <summary>
    /// Returns the image size taking the image DPI into consideration.
    /// </summary>
    /// <param name="image">Image for which to calculate the size.</param>
    /// <param name="graphics">Graphics used to calculate the image size.</param>
    /// <param name="size">Calculated size.</param>
    internal static void GetAdjustedImageSize(Image image, Graphics graphics, ref SizeF size)
    {
        if (graphics is not null)
        {
            //this will work in case the image DPI is specified, otherwise the image DPI will be assumed to be same as the screen DPI
            size.Width = image.Width * graphics.DpiX * Chart.DPIScale / image.HorizontalResolution;
            size.Height = image.Height * graphics.DpiY * Chart.DPIScale / image.VerticalResolution;
        }
        else
        {
            size.Width = image.Width;
            size.Height = image.Height;
        }
    }

    /// <summary>
    /// Checks if the image has the same DPI as the graphics object.
    /// </summary>
    /// <param name="image">Image to be checked.</param>
    /// <param name="graphics">Graphics object to be used.</param>
    /// <returns>true if they match, otherwise false.</returns>
    internal static bool DoDpisMatch(Image image, Graphics graphics)
    {
        return graphics.DpiX * Chart.DPIScale == image.HorizontalResolution && graphics.DpiY * Chart.DPIScale == image.VerticalResolution;
    }

    internal static Image GetScaledImage(Image image, Graphics graphics)
    {
        Bitmap scaledImage = new Bitmap(image, new Size((int)(image.Width * graphics.DpiX * Chart.DPIScale / image.HorizontalResolution),
            (int)(image.Height * graphics.DpiY * Chart.DPIScale / image.VerticalResolution)));

        return scaledImage;
    }

    #endregion
}